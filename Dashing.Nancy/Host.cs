using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
using Nancy.Bootstrapper;
using Nancy;
using Nancy.TinyIoc;
using Nancy.ViewEngines.Razor;
using Cassette;
using Cassette.Nancy;

namespace Dashing
{
    public class Host
    {
        public static void Main(params string[] args)
        {
            Settings.DefaultDashboard = "sample";
            Settings.Views = "dashboards";
            var nancyHost = new NancyStreamHost(new Bootstrapper(),new Uri("http://localhost:1234"));
            
            nancyHost.Start();
            Console.ReadKey();
            nancyHost.Stop();
        }

    }
}
