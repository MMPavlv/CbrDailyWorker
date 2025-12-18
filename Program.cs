using CbrDailyWorker.DBModels;
using Microsoft.EntityFrameworkCore;

namespace CbrDailyWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContextFactory<CbrContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddSingleton<DbService>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}