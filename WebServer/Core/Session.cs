using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServer.Core
{
    public class Session
    {
        private int _selected_port;
        private string _app_path;
        private AuthCredentials _credentials;
        private string[] protected_files = { "secret.txt" };

        public Session(int port, string path)
        {
            this._app_path = path;
            this._selected_port = port;
        }

        public void Login(string username, string password)
        {
            this.Credentials = new AuthCredentials { username = username, password = password };
        }

        public void Logout()
        {
            this.Credentials = null;
        }

        public bool isAuthenticated()
        {
            if (Credentials != null)
            {
                if (Credentials.username == "user" && Credentials.password == "12345")
                {
                    return true;
                }
            }

            return false;
        }

        public int SelectedPort
        {
            get
            {
                return this._selected_port;
            }
        }

        public string AppPath
        {
            get
            {
                return this._app_path;
            }
        }

        public AuthCredentials Credentials
        {
            get
            {
                return this._credentials;
            }
            set
            {
                this._credentials = value;
            }
        }

        public string[] ProtectedFiles
        {
            get
            {
                return this.protected_files;
            }
        }


    }
}
