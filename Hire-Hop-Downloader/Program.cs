using Hire_Hop_Interface.Management;
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
            GetLogin(out string username, out string password);

            bool loggedin = await Authentication.Login(myHHConn, username, password);

            if (loggedin)
            {
                var results = await Search.GetAllResults(myHHConn, new Search.SearchParams() { _money_owed = false }, false);

                Console.WriteLine($"Finished Collecting {results.Count} Results");
            }
        }

        private static void GetLogin(out string username, out string password)
        {
            if (File.Exists("./login.json"))
            {
                string content = File.ReadAllText("./login.json");
                JObject login = JObject.Parse(content);

                username = login["username"].ToString();
                password = login["password"].ToString();

                Console.WriteLine("Loaded details from file");
            }
            else
            {
                Console.WriteLine("Enter Username: ");
                username = Console.ReadLine();
                Console.WriteLine("Enter Password: ");
                password = Console.ReadLine();

                JObject login = new JObject();
                login["username"] = username;
                login["password"] = password;
                File.WriteAllText("./login.json", login.ToString());
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