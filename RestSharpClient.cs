using Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RestClient
{
    /// <summary>
    /// based on https://exceptionnotfound.net/extending-restsharp-to-handle-timeouts-in-asp-net-mvc/
    /// </summary>
    public class RestSharpClient
    {
        private IRestClient _client;
        private ILogging _logging;

        public BaseClient(ILogging logging, IRestClient client, string baseUrl)
        {
            _client = client;
            _client.BaseUrl = new Uri(baseUrl);

            _logging = logging;
        }

                #region RestClient override methods

        public Tuple<HttpStatusCode, string> Execute(IRestRequest request, bool enableTracing = false)
        {
            if (enableTracing) TraceRequest(request);

            var response = _client.Execute(request);
            ErrorCheck(request, response);

            return new Tuple<HttpStatusCode, string>(response.StatusCode, response.ErrorMessage);
        }

        /// <summary>
        /// T must be ApiResponse<TResponse>
        /// </summary>
        public T Execute<T>(IRestRequest request, bool enableTracing = false) where T : new()
        {
            if (enableTracing) TraceRequest(request);

            var response = _client.Execute<T>(request);
            ErrorCheck(request, response);

            // if client returns anything unusual, use reflection to pass this back to T (ApiResponse)
            if (response.Data == null && response.StatusCode != HttpStatusCode.OK)
            {
                response.Data = new T();
                response.Data.SetProperty("StatusCode", response.StatusCode);
                response.Data.SetProperty("Message", response.ErrorMessage);
            }

            return response.Data;
        }

        public async Task<Tuple<HttpStatusCode, string>> ExecuteAsync(IRestRequest request, bool enableTracing = false)
        {
            if (enableTracing) TraceRequest(request);

            var response = await _client.ExecuteTaskAsync(request);
            ErrorCheck(request, response);

            return new Tuple<HttpStatusCode, string>(response.StatusCode, response.ErrorMessage);
        }

        /// <summary>
        /// T must be ApiResponse<TResponse>
        /// </summary>
        public async Task<T> ExecuteAsync<T>(IRestRequest request, bool enableTracing = false) where T : new()
        {
            if (enableTracing) TraceRequest(request);

            var response = await _client.ExecuteTaskAsync<T>(request);
            ErrorCheck(request, response);

            // if client returns anything unusual, use reflection to pass this back to T (ApiResponse)
            if (response.Data == null && response.StatusCode != HttpStatusCode.OK)
            {
                response.Data = new T();
                response.Data.SetProperty("StatusCode", response.StatusCode);
                response.Data.SetProperty("Message", response.ErrorMessage);
            }

            return response.Data;
        }

        #endregion RestClient override methods

        #region private methods

        private void LogError(IRestRequest request, IRestResponse response)
        {
            string parameters = ParseParameters(request);

            //Set up the information message with the URL, the status code, and the parameters.
            string info = $@"
                    Request to {_client.BaseUrl.AbsoluteUri}{request.Resource} failed with status code {response.StatusCode}, 
                    parameters: {parameters}, and content: {response.Content}, Timeout is set to {request.Timeout}ms.";

            //Acquire the actual exception
            Exception ex;
            if (response != null && response.ErrorException != null)
            {
                ex = response.ErrorException;
            }
            else
            {
                ex = new Exception(info);
                info = string.Empty;
            }

            //Log the exception and info message
            _logging.LogErrorException(info, ex);
        }

        private void TraceRequest(IRestRequest request)
        {
            var parameters = ParseParameters(request);
            var path = _client.BaseUrl.AbsoluteUri + request.Resource;

            _logging.LogTraceMessage($"Request {path} sent with parameters: {parameters}");
        }

        private void ErrorCheck(IRestRequest request, IRestResponse response)
        {
            // timeout code from restclient
            if (response?.StatusCode == 0)
            {
                // overwrite the statuscode with the correct statuscode
                response.StatusCode = HttpStatusCode.RequestTimeout;
            }

            // something went wrong
            if (response?.StatusCode == null || response.StatusCode != HttpStatusCode.OK)
            {
                LogError(request, response);
            }
        }

        private string ParseParameters(IRestRequest request)
        {
            return string.Join(", ", request.Parameters.Select(x => x.Name.ToString() + "=" + ((x.Value == null) ? "NULL" : x.Value))
                .ToArray());
        }

        #endregion private methods
    }
}
