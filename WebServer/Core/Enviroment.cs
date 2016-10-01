using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServer.Core
{
    public static class Enviroment
    {
        public const int DEFAULT_PORT = 8080;
        public static int MAX_POST_SIZE = 10 * 1024 * 1024; //10 MB
        public const int BUFFER_SIZE = 4096;
        public static string[]SERVER_VALID_VERBS = { "POST", "GET", "DELETE", "PUT" };

        public enum STATUS_RESPONSE
        {
            OK = 1,
            NOT_FOUND = 2,
            UNAUTHORIZED = 3
        };
    }
}
