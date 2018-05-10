using System.Collections.Generic;
using System.Net;

namespace RestClient
{
    public class ApiResult
    {
        public object Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
