using App.Core;
using Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace App.Controller
{
    public class HomeController : BaseController
    {
        [HTTPGET]
        public string Index(string path)
        {
            string file = path + "homeIndex.shtml";
            if (File.Exists(file))
            {
                return file;
            }
            return null;
        }
    }
}
