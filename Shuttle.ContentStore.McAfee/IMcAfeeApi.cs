using System;
using RestSharp;

namespace Shuttle.ContentStore.McAfee
{
    public interface IMcAfeeApi
    {
        event EventHandler LoginCompleted;
        event EventHandler LogoutCompleted;
        event EventHandler HeartbeatCompleted;

        void Login();
        void Logout();

        T Get<T>(RestRequest request) where T : new();
        IRestResponse GetResponse(RestRequest request);
        string GetFullApiUrl(string path);
    }
}