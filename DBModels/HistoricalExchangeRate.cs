using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbrDailyWorker.DBModels
{
    [PrimaryKey(nameof(Date), nameof(CurrencyId))]
    public record HistoricalExchangeRate
    {
        public required DateOnly Date { get; set; }

        [ForeignKey(nameof(Currency))]
        public required string CurrencyId { get; set; }
        public required uint NumCode { get; set; }
        public required string CharCode { get; set; }
        public required string Value { get; set; }
        public required string VunitRate { get; set; }
    }
}
