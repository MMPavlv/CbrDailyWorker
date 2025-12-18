using CbrDailyWorker.Exceptions;
using CbrDailyWorker.WebModels;
using System.Configuration;
using System.Text;
using System.Xml.Serialization;

namespace CbrDailyWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DbService _dbService;
        private readonly IConfiguration _configuration;

        private readonly int _timeout = 60;

        public Worker(ILogger<Worker> logger, DbService dbService, IConfiguration configuration)
        {
            _logger = logger;
            _dbService = dbService;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            //Настраиваем кодировку, чтобы не падать при запросе страниц в кодировке Win 1251
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //Настраиваем таймаут для проверки необходимости действий
            if (!int.TryParse(_configuration.GetSection("Settings")["Timeout"], out int timeout))
            {
                timeout = _timeout;
            }

            //Если информация о бэкенде не инициализрована,
            //то инициализируем, включая список валют
            if (await _dbService.GetBackEndInfo() is null)
            {
                var version = _configuration.GetSection("Settings")["Version"];

                if (version == null)
                {
                    throw new MissingConfigurationException("Version");
                }

                await _dbService.SetBackEndInfo(version, DateTimeOffset.MinValue);

                var currencyList = await GetCurrencyList();

                if (currencyList is not null)
                {
                    await _dbService.LoadCurrencies(currencyList);

                    _logger.LogInformation("Loaded {0} currencies", currencyList.Currencies.Count);
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var today = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime);
                DateOnly lastExecutionDate =
                    DateOnly.FromDateTime((await _dbService.GetBackEndInfo())!.Value.LastUpdateDate.DateTime);

                //Проверяем, запускали мы сегодня или нет задачу. Если запускали, то ждем таймаут
                if (today <= lastExecutionDate)
                {
                    await Task.Delay(TimeSpan.FromMinutes(timeout), stoppingToken);
                }

                //Запрашиваем данные по курсам валют на сегодня и перезаписываем таблицу с этой информацией
                var exchangeRawData = await GetExchangeData(today);

                if (exchangeRawData is not null)
                {
                    await _dbService.LoadRecentExchangeRates(exchangeRawData);

                    _logger.LogInformation("Loaded {0} exchange rates", exchangeRawData.ExchangeRates.Count);
                }

                //Дни с первого числа по текущее число
                var days = Enumerable.Range(1, today.Day - 1).Select(day => new DateOnly(today.Year, today.Month, day)).ToList();

                //Все дни предыдущего месяца
                //var yesterMonth = today.AddMonths(-1);
                //var days = Enumerable.Range(1, DateTime.DaysInMonth(yesterMonth.Year, yesterMonth.Month))
                //    .Select(day => new DateOnly(yesterMonth.Year, yesterMonth.Month, day)).ToList();

                //Предыдущие 30 дней, исключая текущий
                //var startDate = today.AddDays(-30);
                //var days = Enumerable.Range(0, 30).Select(offset => startDate.AddDays(offset)).ToList();

                //Для полученного списка дней мы запрашиваем исторические данные о курсах
                //И добавляем отсутствующие записи в соответствующую таблицу
                if (days.Any())
                {
                    List<ExchangeResponseModel> historicalExchanges = new();

                    foreach (var day in days)
                    {
                        var oldExchangeRawData = await GetExchangeData(day);

                        if (oldExchangeRawData is not null)
                        {
                            historicalExchanges.Add(oldExchangeRawData);
                        }
                    }

                    await _dbService.UpdateHistoricalExchangeRates(historicalExchanges);


                    _logger.LogInformation("Loaded information for {0} Days", days.Count);
                }

                //Обновляем дату последней работы в бэкенде
                await _dbService.UpdateBackEndInfo(DateTimeOffset.Now);
                _logger.LogInformation("Daily routine ended at {time}", DateTimeOffset.Now);
            }
        }


        #region Currencies

        protected async Task<CurrencyListModel?> GetCurrencyList()
        {
            string currencyRawData = await RequestCurrencyData();
            return ParseCurrencyData(currencyRawData);
        }

        protected async Task<string> RequestCurrencyData()
        {
            string url = "https://cbr.ru/scripts/XML_val.asp?d=0";

            using HttpClient client = new HttpClient();

            return await client.GetStringAsync(url);
        }

        protected CurrencyListModel? ParseCurrencyData(string data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CurrencyListModel));
            using TextReader reader = new StringReader(data);
            return serializer.Deserialize(reader) as CurrencyListModel;
        }

        #endregion

        #region Exchange Rates

        protected async Task<ExchangeResponseModel?> GetExchangeData(DateOnly date)
        {
            string exchangeRawData = await RequestExchangeData(date);
            return ParseExchangeData(exchangeRawData);
        }

        protected async Task<string> RequestExchangeData(DateOnly date)
        {
            string url = $"http://www.cbr.ru/scripts/XML_daily.asp?date_req={date.Day:D2}/{date.Month:D2}/{date.Year}";

            using HttpClient client = new HttpClient();

            return await client.GetStringAsync(url);
        }

        protected ExchangeResponseModel? ParseExchangeData(string data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ExchangeResponseModel));

            using TextReader reader = new StringReader(data);

            return serializer.Deserialize(reader) as ExchangeResponseModel;
        }

        #endregion
    }
}
