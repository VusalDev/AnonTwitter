using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using System.Net.Http;

using YETwitter.Identity.Web.Data;

namespace YETwitter.Identity.Web.Tests
{
    public class ApplicationFactory : WebApplicationFactory<Program>
    {
        protected ITestOutputHelper outputHelper;

        public ApplicationFactory(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseSerilog((ctx, cfg) =>
            {
                //logging.AddXunit(output);
                // Pass the ITestOutputHelper object to the TestOutput sink
                cfg.MinimumLevel.Verbose()
                    .WriteTo.TestOutput(outputHelper, Serilog.Events.LogEventLevel.Verbose);
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<ApplicationDbContext>));

                services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(this.GetType().FullName);
                });

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                db.Database.EnsureCreated();
            });
        }


        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
        }
    }
}
