using System;

using Microsoft.Owin.Hosting;

namespace CrmConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string endpoint = "http://localhost:8080";

            using (WebApp.Start<Startup>(endpoint))
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
