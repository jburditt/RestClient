using Logging;
using RestSharp;
using System;

namespace RestClient.Tests
{
    public class TestClient : BaseClient, ITestClient
    {
        private readonly ILogging _logging;
        private readonly TestClientConfig _config;

        public TestClient(ILogging logging, IRestClient client, TestClientConfig config) : base(logging, client, config.BaseUrl)
        {
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

            return ExecuteApi<ViewModelResponse>(request, enableTracing);
        }
    }
}
