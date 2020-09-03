using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace todo_app
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {                    
                    builder.AddSystemsManager((source) =>
                    {
                        source.Path = "/Database";
                    });
                })
            .UseStartup<Startup>()
            .UseUrls("http://*:8080");
    }
}
