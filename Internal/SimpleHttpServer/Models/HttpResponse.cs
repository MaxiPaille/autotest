// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using System.Collections.Generic;
using System.Net;
using System.Text;

// NOTE: two consequences of this simplified response model are:
//
//      (a) it's not possible to send 8-bit clean responses (like file content)
//      (b) it's 
//       must be loaded into memory in the the Content property. If you want to send large files,
//       this has to be reworked so a handler can write to the output stream instead. 

namespace SimpleHttpServer.Models
{

    public class HttpResponse
    {
        public HttpStatusCode Status { get; set; }
        public byte[] Content { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string ContentAsUTF8
        {
            set
            {
                this.setContent(value, encoding: Encoding.UTF8);
            }
        }
        public void setContent(string content, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            
            if (content != null)
                Content = encoding.GetBytes(content);
        }

        public HttpResponse()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public HttpResponse AddHeader(string key, string value)
        {
            Headers[key] = value;
            return this;
        }
        
        // informational only tostring...
        public override string ToString()
        {
            return $"HTTP status {(int)Status} - {Status}";
        }
    }
}
