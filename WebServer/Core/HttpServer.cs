using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace WebServer.Core
{
    public abstract class HttpServer
    {
        protected int port;
        TcpListener listener;
        bool is_active = true;
        Session _s;

        public HttpServer(int port, Session s)
        {
            this.port = port;
            this._s = s;
        }

        public void listen()
        {
            listener = new TcpListener(port);
            listener.Start();
            while (is_active)
            {
                TcpClient s = listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(s, this);
                Thread thread = new Thread(new ThreadStart(processor.process));
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public Session Session
        {
            get { return this._s; }
        }

        public abstract void HandleRequest(HttpProcessor processor, string verb);
    }
}
