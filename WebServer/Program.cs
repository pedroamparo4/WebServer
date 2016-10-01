using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WebServer.Core;

namespace WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer httpServer;
            Session session;
            int port = 0;
            string path = string.Empty;
            
            if (args.GetLength(0) > 0)
            {
                port = Convert.ToInt16(args[0]);         
            }
            if (args.GetLength(0) > 1)
            {
                path = args[1];
            }

            session = new Session
                (
                    port: (port == 0 ? Enviroment.DEFAULT_PORT : port), 
                    path: (string.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : path)
                );

            httpServer = new MyServer(session.SelectedPort, session);

            Console.WriteLine("SERVER RUNNING ON PORT: "+ session.SelectedPort);
            Console.WriteLine("STARTED ON: " + DateTime.Now);
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();

        }
    }
}
