using Hire_Hop_Interface.Management;
using Hire_Hop_Interface.Objects;
using Hire_Hop_Interface.Requests;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Hire_Hop_Downloader
{
    internal class Program
    {
        #region Fields

        private static ClientConnection myHHConn = new ClientConnection();

        #endregion Fields

        #region Methods

        private static async void App()
        {
            GetLogin(out string username, out string password, out string companyCode, out JObject login);

            Console.WriteLine("Performing log in");
            bool loggedin = await Authentication.Login(myHHConn, username, password, companyCode);

            if (loggedin)
            {
                File.WriteAllText("./login.json", login.ToString());
                Console.WriteLine("Logged in");

                await LabourData.Load(myHHConn);

                var results = await Search.GetAllResults(myHHConn, new Search.SearchParams() { _closed = false, _open = false, _search = false, _depot = -1, _status = "" });

                //BulkAdditionalData.LoadExtraDetail(ref results, myHHConn);

                var jobs = BulkAdditionalData.SearchToJob(results);

                //BulkAdditionalData.CalculateCosts(ref jobs, myHHConn);

                BulkAdditionalData.CalculateBilling(ref jobs, myHHConn);

                Console.WriteLine($"Finished Collecting {results.Length} Results");
                Console.WriteLine("Writing Results To data.csv");

                JSON_To_CSV.Converter.WriteConversion("../data.csv", JArray.FromObject(jobs));

                Console.WriteLine("Wrote Results to CSV");
            }
            else
            {
                Console.WriteLine("Log In Failed!\n\n");
                App();
            }
        }

        private static void GetLogin(out string username, out string password, out string companyCode, out JObject login)
        {
            if (File.Exists("./login.json"))
            {
                string content = File.ReadAllText("./login.json");
                login = JObject.Parse(content);

                username = login["username"].ToString();
                password = login["password"].ToString();
                companyCode = login["companyCode"].ToString();

                Console.WriteLine("Loaded details from file");
            }
            else
            {
                Console.WriteLine("Enter Hire Hop Company Code: ");
                companyCode = Console.ReadLine();
                Console.WriteLine("Enter Hire Hop Username: ");
                username = Console.ReadLine();
                Console.WriteLine("Enter Hire Hop Password: ");
                password = Console.ReadLine();

                login = new JObject();
                login["username"] = username;
                login["password"] = password;
                login["companyCode"] = companyCode;
            }
        }

        private static void Main(string[] args)
        {
            App();
            while (true)
            {
                System.Threading.Thread.Sleep(5000);
            }
        }

        #endregion Methods
    }
}