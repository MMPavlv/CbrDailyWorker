using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbrDailyWorker.DBModels
{
    public record Currency
    {
        [Key]
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string EngName { get; set; }
        public required uint Nominal { get; set; }
        public required string ParentCode { get; set; }
    }
}
