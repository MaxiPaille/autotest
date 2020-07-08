using SimpleHttpServer.Models;
using System.Net;

namespace SimpleHttpServer
{
    class HttpBuilder
    {
        public static HttpResponse InternalServerError()
        {
            //string content = File.ReadAllText("Resources/Pages/500.html"); 

            return new HttpResponse()
            {
                Status = HttpStatusCode.InternalServerError,
                ContentAsUTF8 = null
            };
        }

        public static HttpResponse NotFound()
        {
            //string content = File.ReadAllText("Resources/Pages/404.html");

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
