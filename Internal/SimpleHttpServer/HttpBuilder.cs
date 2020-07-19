using SimpleHttpServer.Models;
using System.Net;

namespace SimpleHttpServer
{
    class HttpBuilder
    {
        public static HttpResponse InternalServerError()
        {
            return new HttpResponse()
            {
                Status = HttpStatusCode.InternalServerError,
                ContentAsUTF8 = null
            };
        }
        
        public static HttpResponse ExpectationFailed()
        {
            return new HttpResponse()
            {
                Status = HttpStatusCode.ExpectationFailed,
                ContentAsUTF8 = null
            };
        }

        public static HttpResponse NotFound()
        {
            return new HttpResponse()
            {
                Status = HttpStatusCode.NotFound,
                ContentAsUTF8 = null
            };
        }

        public static HttpResponse OK(string content = null)
        {
            return new HttpResponse()
            {
                Status = HttpStatusCode.OK,
                ContentAsUTF8 = content
            };
        }

        public static HttpResponse MissingValue(string key)
        {
            return new HttpResponse()
            {
                Status = HttpStatusCode.BadRequest,
                ContentAsUTF8 = $"Value '{key}' is missing."
            };
        }
    }
}
