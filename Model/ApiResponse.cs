using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace RestClient.Model
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Errors { get; set; }

        public ApiResponse(HttpStatusCode statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        private static string GetDefaultMessageForStatusCode(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.NotFound:
                    return "Resource not found.";
                case HttpStatusCode.InternalServerError:
                    return "An unhandled error occurred.";
                default:
                    return null;
            }
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Result { get; set; }

        public ApiResponse() : base(HttpStatusCode.OK) { }

        public ApiResponse(T result) : base(HttpStatusCode.OK)
        {
            Result = result;
        }
    }
}
