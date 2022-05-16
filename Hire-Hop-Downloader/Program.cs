using Hire_Hop_Interface.Interface.Connections;
using Newtonsoft.Json.Linq;
using System;
using Hire_Hop_Interface.Interface;
using Hire_Hop_Interface.Objects;
using Hire_Hop_Interface.Objects.JobProject;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hire_Hop_Downloader
{
    internal class Program
    {
        #region Fields

        private static CookieConnection myHHConn = new CookieConnection();

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

                var costs = await DefaultCost.Search(myHHConn);

                var results = await SearchResult.SearchForAll(new SearchResult.SearchOptions() { closed = false, open = false, search = false, depot = -1, status = "" }, myHHConn);

                //BulkAdditionalData.LoadExtraDetail(ref results, myHHConn);

                var t_jobs = results.results.Select(x => x.GetJob(myHHConn)).ToArray();
                Task.WaitAll(t_jobs);
                var jobs = t_jobs.Select(x=>new JobWithMisc(x.Result)).ToArray();

                //BulkAdditionalData.CalculateCosts(ref jobs, myHHConn);

                var t_bill = jobs.Select(x => x.LoadMisc(myHHConn)).ToArray();
                Task.WaitAll(t_bill);

                Console.WriteLine($"Finished Collecting {jobs.Length} Results");
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