﻿using Hire_Hop_Interface.Management;
using Hire_Hop_Interface.Objects;
using Hire_Hop_Interface.Requests;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

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

            Console.WriteLine("Performing log in");
            bool loggedin = await Authentication.Login(myHHConn, username, password);

            if (loggedin)
            {
                Console.WriteLine("Logged in");

                await LabourData.Load(myHHConn);

                var results = await Search.GetAllResults(myHHConn, new Search.SearchParams() { _closed=false, _open=false, _money_owed=false }, true);
                var jobs = results.Select(x => new Hire_Hop_Interface.Objects.Jobs(x.Value));

                var loadTasks = jobs.Select(x => x.CalculateCosts(myHHConn)).ToArray();

                Task.WaitAll(loadTasks);

                int i = 0;
                foreach (Hire_Hop_Interface.Objects.Jobs j in jobs)
                {
                    Console.WriteLine($"{j.id}-- Total Cost: £{loadTasks[i].Result.totalCost}");
                    i++;
                }

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