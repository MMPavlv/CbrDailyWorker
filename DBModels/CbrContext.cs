using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CbrDailyWorker.DBModels
{
    public class CbrContext : DbContext
    {
        public DbSet<BackEndInfo> BackEndInfo { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<RecentExchangeRate> RecentExchangeRates { get; set; }
        public DbSet<HistoricalExchangeRate> HistoricalExchangeRates { get;set; }

        public CbrContext(DbContextOptions<CbrContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }

    public static class DbHelper
    {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            dbSet.RemoveRange(dbSet);
        }
    }
}
