using System;
using Hire_Hop_Interface.Management;
using Hire_Hop_Interface.Requests;
using Newtonsoft.Json.Linq;

namespace Hire_Hop_Downloader
{
    class Program
    {
        static ClientConnection myHHConn = new ClientConnection();

        static void Main(string[] args)
        {
            App();
            while (true)
            {
                System.Threading.Thread.Sleep(5000);
            }
        }

        static async void App()
        {
            

            bool loggedin = await Authentication.Login(myHHConn, "odavies@etherlive.co.uk", "");

            if (loggedin)
            {
                JObject job = await Jobs.GetJobData(myHHConn, "1131");
            }
        }
    }
}
