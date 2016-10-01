using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebServer.Core
{
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServer server;
        public Stream inputStream;
        public StreamWriter outputStream;
        public String http_verb;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();
        public List<People> people_database = new List<People>();

        public HttpProcessor(TcpClient client, HttpServer server)
        {
            this.socket = client;
            this.server = server;
        }
        
        public void process()
        {
            inputStream = new BufferedStream(socket.GetStream());
            outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));

            try
            {
                parseRequest();
                GetHeaders();
                HandleRequest(http_verb);
            }
            catch (Exception e)
            {
                Return_Status_Response(Enviroment.STATUS_RESPONSE.NOT_FOUND, null);
            }
            CleanRequest();       
        }

        public void parseRequest()
        {
            String request = Helpers.streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("Invalid HTTP Request");
            }
            http_verb = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_versionstring = tokens[2];
        }

        public void GetHeaders()
        {
            String line;
            while ((line = Helpers.streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("Invalid HTTP Header Line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                httpHeaders[name] = value;
            }

            if (!this.server.Session.isAuthenticated())
            {
                try
                {
                    var auth = Helpers.GetCredentialsFromAuth(this.httpHeaders["Authorization"].ToString());
                    this.server.Session.Login(auth.username, auth.password);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void HandleRequest(string verb)
        {
            server.HandleRequest(this, verb);
        }
        
        public void Return_Status_Response(Enviroment.STATUS_RESPONSE response, string mime_type)
        {
            switch(response)
            {
                case Enviroment.STATUS_RESPONSE.OK:
                    outputStream.WriteLine("HTTP/1.0 200 OK");
                    if(!string.IsNullOrEmpty(mime_type))
                    {
                        outputStream.WriteLine("Content-Type: " + mime_type);
                    }                 
                    outputStream.WriteLine("Connection: close");
                    outputStream.WriteLine("");
                    break;

                case Enviroment.STATUS_RESPONSE.NOT_FOUND:
                    outputStream.WriteLine("HTTP/1.0 404 File not found");
                    if (!string.IsNullOrEmpty(mime_type))
                    {
                        outputStream.WriteLine("Content-Type: " + mime_type);
                    }
                    outputStream.WriteLine("Connection: close");
                    outputStream.WriteLine("");
                    break;

                case Enviroment.STATUS_RESPONSE.UNAUTHORIZED:
                    
                    outputStream.WriteLine("HTTP/1.0 401 Unauthorized");
                    if (!string.IsNullOrEmpty(mime_type))
                    {
                        outputStream.WriteLine("Content-Type: " + mime_type);
                    }
                    outputStream.WriteLine("Connection: close");
                    outputStream.WriteLine("");
                    break;
            }
        }

        private void CleanRequest()
        {
            outputStream.Flush();
            inputStream = null;
            outputStream = null;
            socket.Close();
        }
    }
}
