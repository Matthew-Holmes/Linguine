using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Agents.OpenAI
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ApiException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
