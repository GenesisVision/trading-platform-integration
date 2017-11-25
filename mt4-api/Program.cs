using DataModels;
using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mt4_api
{
    class Program
    {
        static void Main(string[] args)
        {
            HostConfiguration hostConf = new HostConfiguration();
            hostConf.RewriteLocalhost = true;
            using (var host = new NancyHost(hostConf, new Uri("http://localhost:1234")))
            {
                host.Start();
                Console.WriteLine("Running on http://localhost:1234");
                Console.ReadLine();
            }
  
        }
    }
}
