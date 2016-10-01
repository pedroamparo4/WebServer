using Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WebServer.Core
{
    public class MyServer : HttpServer
    {
        public MyServer(int port, Session session)
            : base(port, session) {
        }
        public override void HandleRequest(HttpProcessor processor, string verb)
        {
            string MIME_TYPE = string.Empty;
            string[] url_splitted;
            string application_name = string.Empty;
            string controller_name = string.Empty;
            string dll_app_path = string.Empty;

            switch (verb)
            {
                case "GET":
                    #region GET CODE
                    url_splitted = processor.http_url.Split('/');

                    if (url_splitted.Count()  == 2)//IS A FILE
                    {
                        SendFileToClient(processor, url_splitted[1], null);
                        return;
                    }

                    #region CONTROLLERS CODE
                    // Parsing url
                    if (url_splitted[1] != null && url_splitted[1] != "")
                    {
                        application_name = url_splitted[1];
                    }
                    if (url_splitted[2] != null && url_splitted[2] != "")
                    {
                        controller_name = url_splitted[2];
                    }

                    //IF DLL FILE DOESN'T EXISTS RETURN NOT FOUND TO CLIENT
                    dll_app_path = Session.AppPath + "\\" + application_name + ".dll";
                    if (!File.Exists(dll_app_path))
                    {
                        throw new Exception("APP NOT FOUND");
                    }

                    // Loading application file
                    var assembly = Assembly.LoadFile(dll_app_path);
                    if (assembly == null)
                    {
                        throw new Exception("APP NOT FOUND");
                    }

                    // Checking if is a valid httpVerb
                    if (!Enviroment.SERVER_VALID_VERBS.Contains(verb))
                    {
                        processor.Return_Status_Response(Enviroment.STATUS_RESPONSE.NOT_FOUND, null);
                        processor.outputStream.WriteLine("HTTP VERB NOT SUPPORTED");
                        return;
                    }

                    var controllerType = typeof(BaseController);
                    string controllers_routed_name = $"{application_name}.Controller.{controller_name}Controller";
                    // Getting the Controller type
                    Type tController = assembly.GetTypes().FirstOrDefault(
                        t => t.FullName == controllers_routed_name &&
                        controllerType.IsAssignableFrom(t));

                    if (tController == null)
                    {
                        processor.Return_Status_Response(Enviroment.STATUS_RESPONSE.NOT_FOUND, null);
                        processor.outputStream.WriteLine($"CONTROLLER [{controller_name}] NOT FOUND, PLEASE CHECK YOUR ROUTE");
                        return;
                    }

                    // Get HTTP verb type
                    Type tVerb = assembly.GetType($"{application_name}.Core.HTTP{verb}");

                    // Getting Controller method by verb attribute
                    var method = tController.GetMethods().FirstOrDefault(m => m.GetCustomAttributes(tVerb, false).Length > 0);

                    if (method == null)
                    {
                        processor.Return_Status_Response(Enviroment.STATUS_RESPONSE.NOT_FOUND, null);
                        processor.outputStream.WriteLine($"METHOD <strong>{method}</strong> NOT FOUND ON <strong>{controller_name}</strong>, PLEASE CHECK YOUR ROUTE");
                        return;
                    }

                    // Creating an instance for the controller
                    object classInstance = Activator.CreateInstance(tController, null);
                    ParameterInfo[] parameters = method.GetParameters();
                    object[] parametersArray = { Session.AppPath + "\\" };
                    string filepath = (string)method.Invoke(classInstance, parametersArray);

                    SendFileToClient(processor, filepath, filepath);
                    #endregion
                    #endregion
                    break;

                case "POST":
                    MIME_TYPE = "text/html";
                    #region POST CODE

                    url_splitted = processor.http_url.Split('/');
                    if (url_splitted.Count() == 2)//IS A FILE
                    {
                        SendFileToClient(processor, url_splitted[1], null);
                        return;
                    }

                    int content_len = 0;
                    MemoryStream ms = new MemoryStream();
                    if (processor.httpHeaders.ContainsKey("Content-Length"))
                    {
                        content_len = Convert.ToInt32(processor.httpHeaders["Content-Length"]);
                        if (content_len > Enviroment.MAX_POST_SIZE)
                        {
                            throw new Exception(
                                String.Format("CONTENT IS TOO BIG",
                                  content_len));
                        }
                        byte[] buf = new byte[Enviroment.BUFFER_SIZE];
                        int to_read = content_len;
                        while (to_read > 0)
                        {
                            int numread = processor.inputStream.Read(buf, 0, Math.Min(Enviroment.BUFFER_SIZE, to_read));
                            Console.WriteLine("read finished, numread={0}", numread);
                            if (numread == 0)
                            {
                                if (to_read == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    throw new Exception("client disconnected durin post");
                                }
                            }
                            to_read -= numread;
                            ms.Write(buf, 0, numread);
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                    }

                    StreamReader inputData = new StreamReader(ms);
                    string data = inputData.ReadToEnd();

                    //RETORNAR LO QUE DEVUELVA LA APP
                    var list = Helpers.ParseRequestParams(data);
                    var people = new People
                    {
                        id = System.Guid.NewGuid().ToString(),
                        name = list.Where(p => p.key == "name").FirstOrDefault().value,
                        lastname = list.Where(p => p.key == "lastname").FirstOrDefault().value,
                        age = list.Where(p => p.key == "age").FirstOrDefault().value,
                    };
                    processor.people_database.Add(people);

                    processor.outputStream.WriteLine("You have save the contact successfully");
                    processor.outputStream.WriteLine("-Id: " +people.id);
                    processor.outputStream.WriteLine("-Name: " +people.name);
                    processor.outputStream.WriteLine("-Lastname: " +people.lastname);
                    processor.outputStream.WriteLine("-Age: " +people.age);
                    #endregion
                    break;

                case "PUT":
                    break;

                case "DELETE":
                    break;


            }

        }

        private void SendFileToClient(HttpProcessor processor, string filename, string path)
        {
            string MIME_TYPE = string.Empty;
            
            if(Session.ProtectedFiles.Contains(filename) == true && Session.isAuthenticated() == true)
            {
                MIME_TYPE = Helpers.GetMimeTypeFromFile(processor.http_url);
                MIME_TYPE = "image/*";
                Stream fs = File.Open(Session.AppPath+"\\"+filename, FileMode.Open);
                // NO SE NECESITA DEVOLVER EL RESPONSE CODE EN ESTA LINEA
                Helpers.CopyStream(fs, processor.outputStream.BaseStream);
                processor.outputStream.BaseStream.Flush();
                fs.Close();
                fs = null;
            }
            else if(Session.ProtectedFiles.Contains(filename) == true && Session.isAuthenticated() == false)
            {
                MIME_TYPE = "text/html";
                processor.Return_Status_Response(Enviroment.STATUS_RESPONSE.UNAUTHORIZED, MIME_TYPE);
                processor.outputStream.WriteLine("UNAUTHORIZED ACCESS");
            }
            else
            {
                MIME_TYPE = Helpers.GetMimeTypeFromFile(filename);
                Stream fs;
                if (string.IsNullOrEmpty(path) == false)
                {
                    fs = File.Open(path, FileMode.Open);
                }
                else
                {
                    fs = File.Open(Session.AppPath + "\\" + filename, FileMode.Open);
                }

                // NO SE NECESITA DEVOLVER EL RESPONSE CODE EN ESTA LINEA
                Helpers.CopyStream(fs, processor.outputStream.BaseStream);
                processor.outputStream.BaseStream.Flush();
                fs.Close();
                fs = null;
                return;
            }
        }
    }
}
