using System;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Threading.Tasks;
using NLog;
using System.IO;

namespace BookStore
{
    class Program
    {
        private static readonly Logger RequestLogger = LogManager.GetLogger("request-logger");

        static void Main(string[] args)
        {
            LogManager.LoadConfiguration("nlog.config");
            var config = new HttpSelfHostConfiguration("http://localhost:8574");

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "API Default",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            try
            {
                using (var server = new HttpSelfHostServer(config))
                {
                    Task serverTask = server.OpenAsync();
                    serverTask.Wait();
                    Console.WriteLine("Server is running at http://localhost:8574");
                    //Console.ReadLine(); //delete before submittion
                }
            }
            catch (AggregateException ex)
            {
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine("Error starting server: " + innerEx.Message);
                    RequestLogger.Error(innerEx, "Error starting server: ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting server: " + ex.Message);
                RequestLogger.Error(ex, "Error starting server: ");
            }
        }

    }
}
