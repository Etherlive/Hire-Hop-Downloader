using System;
using Hire_Hop_Interface.Management;
using Hire_Hop_Interface.Requests;

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
            
        }
    }
}
