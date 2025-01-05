using System;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Threading.Tasks;

namespace BookStore
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8574");

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "API Default",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            try
            {
                using (var server = new HttpSelfHostServer(config))
                {
                    Task serverTask = server.OpenAsync();
                    serverTask.Wait();
                    Console.WriteLine("Server is running at http://localhost:8574");
                }
            }
            catch (AggregateException ex)
            {
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine("Error starting server: " + innerEx.Message);
                    Console.WriteLine(innerEx.StackTrace);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting server: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
