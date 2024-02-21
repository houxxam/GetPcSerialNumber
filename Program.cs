using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Threading.Tasks;


namespace GetPcInformation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string hostName = Environment.MachineName;
            hostName = "HIT-RAD-SEC-MED";
            string pcModel = GetPCModel();
            string _serialNumber = GetSystemSerialNumber();
            string _service = "";
            string _group = "";
            string monitorName;
            string monitorSerialNumber;

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
            }

            Console.WriteLine("Host Name: " + hostName);
            Console.WriteLine("Service Name: " + _service);
            Console.WriteLine("Group Name: " + _group);
            Console.WriteLine("PC Model: " + pcModel);
            Console.WriteLine("PC Serial Number: " + _serialNumber);
            Console.WriteLine("###############");



            //change API Url
            string _url = "http://localhost:5268/api/MaterielsApi";
            // Push Pc Information to api 
            pushToApi(_url, pcModel, _serialNumber, "UC", _service, _group);

            string exePath = "DumpEDID.exe";
            string output = RunExternalExe(exePath);
            var m = ParseMonitors(output);

            foreach (var item in m)
            {
                Console.WriteLine(item.ModelName);
                Console.WriteLine(item.SerialNumber);

                // Push Pc Information to api 
                pushToApi(_url, item.ModelName, item.SerialNumber, "ECRAN", _service, _group);
            }






            Console.ReadLine();
        }
        static string GetPCModel()
        {
            string pcModel = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");

            foreach (ManagementObject obj in searcher.Get())
            {
                pcModel = obj["Model"].ToString();
                break;
            }

            return pcModel;
        }

        static string GetSystemSerialNumber()
        {
            string serialNumber = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");

            foreach (ManagementObject obj in searcher.Get())
            {
                serialNumber = obj["SerialNumber"].ToString();
                break;
            }

            return serialNumber;
        }

        static string RunExternalExe(string exePath, string arguments = "")
        {
            try
            {
                // Create process start info
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Create and start the process
                using (Process process = Process.Start(startInfo))
                {
                    // Read the output (if any)
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(); // Wait for the process to exit
                    return output;
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        static List<MonitorInfo> ParseMonitors(string output)
        {
            List<MonitorInfo> monitors = new List<MonitorInfo>();

            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            MonitorInfo monitor = null;

            foreach (string line in lines)
            {
                if (line.StartsWith("Monitor Name"))
                {
                    if (monitor != null)
                    {
                        monitors.Add(monitor);
                    }
                    monitor = new MonitorInfo();
                    monitor.ModelName = line.Replace("Monitor Name             : ", "").Trim();
                }

                if (monitor != null && line.Trim().StartsWith("Serial Number") && monitor.SerialNumber == null)
                {
                    string[] parts = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        monitor.SerialNumber = parts[1].Trim();
                    }
                }
            }

            // Add the last monitor
            if (monitor != null)
            {
                monitors.Add(monitor);
            }

            return monitors;
        }

        static void pushToApi(string url, string MaterielName, string SerialNumber, string Categorie, string Service, string Group)
        {
            string apiUrl = url +
                            "?MaterielName=" + MaterielName +
                            "&SerialNumber=" + SerialNumber +
                            "&Categorie=" + Categorie +
                            "&Service=" + Service +
                            "&Group=" + Group;
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
        }
    }

    class MonitorInfo
    {
        public string SerialNumber { get; set; }
        public string ModelName { get; set; }
    }

}
