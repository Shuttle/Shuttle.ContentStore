using RestSharp;

namespace Shuttle.ContentStore.Opswat
{
    public interface IOpswatApi
    {
        T Get<T>(RestRequest request) where T : new();
        IRestResponse GetResponse(RestRequest request);
        string GetFullApiUrl(string path);
    }
}