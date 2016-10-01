using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace WebServer.Core
{
    public static class Helpers
    {
        public static string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        public static AuthCredentials GetCredentialsFromAuth(string Base64String)
        {
            byte[] data;
            string decodedString;
            string[] data_splitted;
            string code_to_process;

            try
            {
                code_to_process = Base64String.Split(' ')[1];
                data = Convert.FromBase64String(code_to_process);
                decodedString = Encoding.UTF8.GetString(data);
                data_splitted = decodedString.Split(':');

                if (data_splitted.Length < 2)
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            return new AuthCredentials { username = data_splitted[0], password = data_splitted[1] };
        }

        public static string GetMimeTypeFromFile(string path)
        {

            return "";
        }

        public static long CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];

            long totalBytes = 0;
            int bytesRead = 0;

            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {
                target.Write(buf, 0, bytesRead);
                totalBytes += bytesRead;
            }
            return totalBytes;
        }

        public static IEnumerable<RequestParam> ParseRequestParams(string raw_params)
        {
            string[] param_list = raw_params.Split('&');
            string[] pair_aux;
            List<RequestParam> to_return = new List<RequestParam>();
            foreach(var pair in param_list)
            {
                pair_aux = pair.Split('=');
                to_return.Add(new RequestParam { key = pair_aux[0], value = pair_aux[1] });
            }

            return to_return;
        }
    }
}
