using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Threading;

namespace Shuttle.ContentStore.McAfee
{
    public class McAfeeApi : IMcAfeeApi, IDisposable
    {
        public event EventHandler LoginCompleted = delegate { };
        public event EventHandler LogoutCompleted = delegate { };
        public event EventHandler HeartbeatCompleted = delegate { };

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IRestClient _client;
        private readonly IMcAfeeConfiguration _configuration;
        private readonly string _url;
        private DateTime _nextHeartbeat = DateTime.MaxValue;
        private DateTime _nextLogin = DateTime.MaxValue;
        private string _session;
        private readonly Task _task;
        private string _token;
        private string _userId;
        private static readonly object Lock = new object();
        private readonly ILog _log;

        public McAfeeApi(IMcAfeeConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
            _url = configuration.Url;

            if (!_url.EndsWith("/"))
            {
                _url += "/";
            }

            _client = new RestClient(configuration.Url);

            _client.AddDefaultHeader("Accept", "application/vnd.ve.v1.0+json");

            _client.UseNewtonsoftJson();

            var cancellationToken = _cancellationTokenSource.Token;

            _task = Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Heartbeat();

                    ThreadSleep.While(1000, cancellationToken);

                    if (DateTime.Now > _nextLogin)
                    {
                        Logout();
                    }
                }
            });

            _log = Log.For(this);
        }

        public bool HasSession => !string.IsNullOrWhiteSpace(_token);

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _task?.Wait(_configuration.HeartbeatInterval);

            Logout();
        }

        public void Logout()
        {
            lock (Lock)
            {
                if (!HasSession)
                {
                    return;
                }

                var request = new RestRequest(GetFullApiUrl("session.php"), Method.DELETE, DataFormat.Json);

                var response = GetResponse(request).AsDynamic();

                if (response.success == null)
                {
                    throw new McAfeeException("Could not retrieve the 'success' status from the logout response.");
                }

                if (!Convert.ToBoolean(response.success.ToString()))
                {
                    throw new McAfeeException("Could not log out.");
                }

                ResetSession();

                OnLogoutCompleted();

                _log.Debug($"[mcafee] : logged out");
            }
        }

        private void OnLogoutCompleted()
        {
            LogoutCompleted(this, EventArgs.Empty);
        }

        public T Get<T>(RestRequest request) where T : new()
        {
            lock(Lock)
            {

                Safeguard(request);

                var response = _client.Execute<T>(request);

                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }

                BumpNextHeartbeat();

                return response.Data;
            }
        }

        private void Safeguard(RestRequest request)
        {
            Guard.AgainstNull(request, nameof(request));

            if (!HasSession && !request.Resource.Contains("session.php"))
            {
                Login();
            }

            if (request.Parameters.Find(item =>
                    item.Name.Equals("VE-SDK-API", StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                request.AddHeader("VE-SDK-API", _token);
            }
        }

        public IRestResponse GetResponse(RestRequest request)
        {
            lock(Lock)
            {
                Safeguard(request);

                var response = _client.Execute(request);

                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }

                BumpNextHeartbeat();

                return response;
            }
        }

        public string GetFullApiUrl(string path)
        {
            Guard.AgainstNullOrEmptyString(path, nameof(path));

            return path.StartsWith("/")
                ? $"{_url}{path.Substring(1)}"
                : $"{_url}{path}";
        }

        public void Heartbeat()
        {
            lock (Lock)
            {
                if (!HasSession || DateTime.Now < _nextHeartbeat)
                {
                    return;
                }

                var request = new RestRequest(GetFullApiUrl("heartbeat.php"), Method.GET, DataFormat.Json);

                var response = GetResponse(request).AsDynamic();

                if (response.success == null)
                {
                    throw new McAfeeException("Could not retrieve the 'success' status from the logout response.");
                }

                if (!Convert.ToBoolean(response.success.ToString()))
                {
                    ResetSession();
                }

                OnHeartbeatCompleted();

                BumpNextHeartbeat();
            }
        }

        private void OnHeartbeatCompleted()
        {
            HeartbeatCompleted(this, EventArgs.Empty);
        }

        private void BumpNextHeartbeat()
        {
            _nextHeartbeat = DateTime.Now.Add(_configuration.HeartbeatInterval);
        }

        public void Login()
        {
            lock (Lock)
            {
                if (HasSession)
                {
                    return;
                }

                var request = new RestRequest(GetFullApiUrl("session.php"), Method.GET, DataFormat.Json);

                request.AddHeader("VE-SDK-API",
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{_configuration.Username}:{_configuration.Password}")));

                var response = GetResponse(request).AsDynamic();

                if (response.success == null)
                {
                    throw new McAfeeException("Could not retrieve the 'success' status from the logout response.");
                }

                if (!Convert.ToBoolean(response.success.ToString()))
                {
                    throw new McAfeeException("Could not log in.");
                }

                if (response.results == null)
                {
                    throw new McAfeeException("Could not retrieve the 'results' object from the logout response.");
                }

                if (response.results.session == null)
                {
                    throw new McAfeeException("Could not retrieve the 'session' from the login response.");
                }

                if (response.results.userId == null)
                {
                    throw new McAfeeException("Could not retrieve the 'userId' from the login response.");
                }

                _session = response.results.session.ToString();
                _userId = response.results.userId.ToString();
                _token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_session}:{_userId}"));
                _nextLogin = DateTime.Now.Add(_configuration.SessionDuration);

                OnLoginCompleted();

                _log.Debug($"[mcafee] : logged in");
            }
        }

        private void OnLoginCompleted()
        {
            LoginCompleted(this, EventArgs.Empty);
        }

        private void ResetSession()
        {
            _token = string.Empty;
            _userId = string.Empty;
            _session = string.Empty;
        }
    }
}