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
    public class BaseClient
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

        protected ApiStatusResult Execute(IRestRequest request)
        {
            var response = _client.Execute(request);
            ErrorCheck(request, response);

            return BuildApiStatusResult(response);
        }

        protected Tuple<ApiStatusResult, T> Execute<T>(IRestRequest request) where T : new()
        {
            var response = _client.Execute<T>(request);
            ErrorCheck(request, response);

            var result = response.Data;                        // result from service

            // status result
            var apiStatusResult = BuildApiStatusResult(response);

            return new Tuple<ApiStatusResult, T>(apiStatusResult, result);
        }

        protected Tuple<ApiStatusResult, T> ExecuteApi<T>(IRestRequest request, bool enableTracing = false) where T : new()
        {
            if (enableTracing) TraceRequest(request);

            var response = _client.Execute<ApiResult>(request);
            ErrorCheck(request, response);

            var apiResult = response.Data;                        // result from service
            var result = default(T);

            if (response.StatusCode == HttpStatusCode.OK)
                if (apiResult?.Result != null)
                {
                    var json = JsonConvert.SerializeObject(apiResult.Result);
                    result = JsonConvert.DeserializeObject<T>(json); // serialize result field from apiresult
                }

            // this is just apiresult but stripped of json result, to avoid confusion and force use of TResponse
            var statusResponse = new ApiStatusResult
            {
                Errors = apiResult?.Errors,
                Message = apiResult?.Message,
                StatusCode = apiResult?.StatusCode ?? HttpStatusCode.InternalServerError
            };

            return new Tuple<ApiStatusResult, T>(statusResponse, result);
        }

        protected async Task<ApiStatusResult> ExecuteTaskAsync(IRestRequest request)
        {
            var response = await _client.ExecuteTaskAsync(request);
            ErrorCheck(request, response);

            return BuildApiStatusResult(response);
        }

        protected async Task<Tuple<ApiStatusResult, T>> ExecuteTaskAsync<T>(IRestRequest request)
        {
            var response = await _client.ExecuteTaskAsync<T>(request);
            ErrorCheck(request, response);

            var result = response.Data;                        // result from service

            // status result
            var apiStatusResult = BuildApiStatusResult(response);

            return new Tuple<ApiStatusResult, T>(apiStatusResult, result);
        }

        #endregion RestClient override methods

        #region private methods

        private ApiStatusResult BuildApiStatusResult(IRestResponse response)
        {
            // status result, status code and error message
            var apiStatusResult = new ApiStatusResult();
            apiStatusResult.StatusCode = response?.StatusCode ?? HttpStatusCode.BadRequest;
            apiStatusResult.Message = response?.ErrorMessage;

            return apiStatusResult;
        }

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
