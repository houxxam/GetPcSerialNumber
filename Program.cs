using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace GetPcInformation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string hostName = Environment.MachineName;

            string _serialNumber = RemoveExtraWhitespace(ExecuteWMICCommand("bios get serialnumber").Replace("SerialNumber","").Replace("\r\r\n",""));
            
            string pcModel = RemoveExtraWhitespace(ExecuteWMICCommand("csproduct get name").Replace("Name", "").Replace("\r\r\n", ""));
            string _service = "";
            string _group = "";

            string[] parts = hostName.Split('-');

            if (parts[0] == "HIT")
            {
                _service = parts[1];
                if (parts.Length > 3)
                {
                    _group = parts[3];
                }
                else
                {
                    _group = parts[2];
                }
                
                
            }
            else
            {
                Console.WriteLine("Invalid host name format.");
                return; // Exit the program
            }

            Console.WriteLine("Host Name: " + hostName);
            Console.WriteLine("Service Name: " + _service);
            Console.WriteLine("Group Name: " + _group);
            Console.WriteLine("PC Model: " + pcModel);
            Console.WriteLine("PC Serial Number: " + _serialNumber);

            // Change API Url
            
            //api / MaterielsApi ? materielName = &serialNumber = &categorie = &service = &group =
            string apiUrl = "http://172.16.11.7/api/MaterielsApi" +
                            "?MaterielName=" + Uri.EscapeDataString(pcModel) +
                            "&SerialNumber=" + Uri.EscapeDataString(_serialNumber) +
                            "&Categorie=UC" +
                            "&Service=" + Uri.EscapeDataString(_service) +
                            "&Group=" + Uri.EscapeDataString(_group);


            try
            {
                // Create HTTP request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
                request.Method = "GET";

                // Get HTTP response
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseContent = reader.ReadToEnd();
                    Console.WriteLine("Response: " + responseContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }


            Console.ReadKey();
        }

        static string ExecuteWMICCommand(string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = "wmic";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output.Trim();
        }
        static string RemoveExtraWhitespace(string input)
        {
            // Replace multiple consecutive whitespace characters with a single space
            return Regex.Replace(input, @"\s+", " ");
        }
    }
}
