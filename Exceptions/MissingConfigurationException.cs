using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbrDailyWorker.Exceptions
{
    public class MissingConfigurationException : Exception
    {
        public MissingConfigurationException(string configName) : base($"{configName} is missing from Configuration") { }
    }
}
