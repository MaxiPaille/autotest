// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleHttpServer.Models
{
    public class Route
    {
        
        /// <summary>
        /// Descriptive name (debugging only)
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Regex used to route the request
        /// </summary>
        public string UrlRegex { get; set; }
        
        /// <summary>
        /// Http method (GET, POST, PUT, DELETE, ...)
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// Method called when a request match this route
        /// </summary>
        public Func<HttpRequest, HttpResponse> Callable { get; set; }

        public Route(string name, string method, string urlRegex, Func<HttpRequest, HttpResponse> callable)
        {
            Name = name;
            UrlRegex = urlRegex;
            Method = method;
            Callable = callable;
        }
        
    }
}
