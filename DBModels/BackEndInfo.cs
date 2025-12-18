using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbrDailyWorker.DBModels
{
    public record BackEndInfo
    {
        [DefaultValue("1")]
        [Key]
        public int Id { get; set; }
        public required DateTimeOffset LastUpdateDate { get; set; }
        public required string Version { get; set; }
    }
}
