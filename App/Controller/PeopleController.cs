using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework;
using App.Core;
using System.IO;

namespace App.Controller
{
    public class PeopleController : BaseController
    {
        public PeopleController()
        { }

        [HTTPGET]
        public string peopleGet(string path)
        {
            if (File.Exists($"{path}peopleGet.shtml"))
            {
                return $"{path}peopleGet.shtml";
            }
            return null;
        }

        [HTTPPOST]
        public string peoplePost(string name, string lastname, string age)
        {
            return name + lastname + age;
        }

        [HTTPPUT]
        public string peoplePut(string path)
        {
            if (File.Exists($"{path}peoplePut.shtml"))
            {
                using (Stream filestream = File.Open($"{path}peoplePut.shtml", FileMode.Open))
                    return $"{path}peoplePut.shtml";
            }
            return null;
        }

        [HTTPDELETE]
        public string peopleDelete(string path)
        {
            if (File.Exists($"{path}peopleDelete.shtml"))
            {
                using (Stream filestream = File.Open($"{path}peopleDelete.shtml", FileMode.Open))
                    return $"{path}peopleDelete.shtml";
            }
            return null;
        }

    }
}
