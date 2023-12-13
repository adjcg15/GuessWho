using GuessWhoDataAccess;
using GuessWhoServices;
using System;
using System.ServiceModel;

namespace GuessWhoHost
{
    public static class Program
    {
        static void Main(string[] args)
        {
            ServerLogger.ConfigureLogger();

            using (ServiceHost host = new ServiceHost(typeof(GuessWhoService)))
            {
                host.Open();
                Console.WriteLine("Server is running");
                Console.ReadLine();
            }
        }
    }
}
