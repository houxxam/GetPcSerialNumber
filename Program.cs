using Newtonsoft.Json;
using System;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GetPcInformation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string hostName = Environment.MachineName;

            string pcModel = GetPCModel();
            string _serialNumber = GetSystemSerialNumber();
            string _service = "";
            string _group = "";

            string[] parts = hostName.Split('-');

            if (parts.Length == 3 && parts[0] == "HIT")
            {
                _service = parts[1];
                _group = parts[2];
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

            //change API Url
            string apiUrl = "http://localhost:5268/api/MaterielsApi";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var data = new { MaterielName = pcModel, SerialNumber = _serialNumber, Categorie = "UC", Service = _service, Group = _group };

                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response: " + responseContent);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
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
    }

}
