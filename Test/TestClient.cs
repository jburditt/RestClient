using Logging;
using RestSharp;
using System;

namespace RestClient.Tests
{
    public class TestClient : ITestClient
    {
        private readonly RestSharpClient _client;
        private readonly ILogging _logging;
        private readonly TestClientConfig _config;

        public TestClient(ILogging logging, IRestClient client, TestClientConfig config) : base(logging, client, config.BaseUrl)
        {
            _client = client;
            _config = config;
            _logging = logging;

            if (string.IsNullOrEmpty(config.BaseUrl))
                throw new NullReferenceException("PromoServicesHostname AppSettings does not exist.");
        }

        public Tuple<ApiStatusResult, ViewModelResponse> Add(ViewModelRequest model)
        {
            var request = new RestRequest("todo/add", Method.POST);
            request.AddJsonBody(model);
            request.AddHeader("Token", _config.Token);

            var enableTracing = ApplicationSettings.EnableAddTestTracing;

            return _client.ExecuteApi<ViewModelResponse>(request, enableTracing);
        }
    }
}
