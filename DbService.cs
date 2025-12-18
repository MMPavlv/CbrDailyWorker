using CbrDailyWorker.DBModels;
using CbrDailyWorker.WebModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CbrDailyWorker
{
    public class DbService
    {
        private readonly ILogger<DbService> _logger;
        private readonly IDbContextFactory<CbrContext> _dbFactory;

        public DbService(ILogger<DbService> logger, IDbContextFactory<CbrContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
        }

        public async Task<(string Version, DateTimeOffset LastUpdateDate)?> GetBackEndInfo()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            BackEndInfo? result = await context.BackEndInfo.FirstOrDefaultAsync();

            if (result is null)
            {
                return null;
            }
            else
            {
                return new ValueTuple<string, DateTimeOffset>(result.Version, result.LastUpdateDate);
            }
        }

        public async Task SetBackEndInfo(string version, DateTimeOffset date)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            context.BackEndInfo.Clear();

            await context.BackEndInfo.AddAsync(new BackEndInfo{Version = version, LastUpdateDate = date});

            await context.SaveChangesAsync();
        }

        public async Task UpdateBackEndInfo(DateTimeOffset date)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var info = await context.BackEndInfo.FirstOrDefaultAsync();

            info!.LastUpdateDate = date;

            context.Update(info);

            await context.SaveChangesAsync();
        }

        public async Task LoadCurrencies(CurrencyListModel currencyListModel)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (await context.Currencies.AnyAsync())
            {
                context.Currencies.Clear();
            }

            var newCurrencies = currencyListModel.Currencies.Select(currency =>
                new Currency
                {
                    Id = currency.Id,
                    Name = currency.Name,
                    EngName = currency.EngName,
                    Nominal = currency.Nominal,
                    ParentCode = currency.ParentCode
                }).ToList();

            await context.Currencies.AddRangeAsync(newCurrencies);

            await context.SaveChangesAsync();
        }

        public async Task LoadRecentExchangeRates(ExchangeResponseModel exchangeRates)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (await context.RecentExchangeRates.AnyAsync())
            {
                context.RecentExchangeRates.Clear();
            }

            var recentExchangeRates = exchangeRates.ExchangeRates.Select(rate =>
                new RecentExchangeRate
                {
                    CurrencyId = rate.Id,
                    NumCode = rate.NumCode,
                    CharCode = rate.CharCode,
                    Value = rate.Value,
                    VunitRate = rate.VunitRate
                }).ToList();

            await context.RecentExchangeRates.AddRangeAsync(recentExchangeRates);

            await context.SaveChangesAsync();
        }


        public async Task UpdateHistoricalExchangeRates(List<ExchangeResponseModel> historicalExchanges)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            const string format = "dd.MM.yyyy";

            foreach (var historicalExchange in historicalExchanges)
            {
                if (DateOnly.TryParseExact(historicalExchange.Date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateValue))
                {
                    foreach (var exchange in historicalExchange.ExchangeRates)
                    {
                        if (await context.HistoricalExchangeRates.FindAsync(dateValue, exchange.Id) is null)
                        {
                            context.HistoricalExchangeRates.Add(new HistoricalExchangeRate
                            {
                                Date = dateValue,
                                CurrencyId = exchange.Id,
                                NumCode = exchange.NumCode,
                                CharCode = exchange.CharCode,
                                Value = exchange.Value,
                                VunitRate = exchange.VunitRate
                            });
                        }
                    }
                }
                else
                {
                    _logger.LogError("Failed to parse '{date}' into a valid date using format '{text}'.", historicalExchange.Date, format);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
