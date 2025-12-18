using System.Xml.Serialization;

namespace CbrDailyWorker.WebModels
{
    [XmlRoot(ElementName = "ValCurs")]
    public class ExchangeResponseModel
    {
        [XmlElement(ElementName = "Valute")]
        public required List<CurrencyExchangeModel> ExchangeRates { get; set; }

        [XmlAttribute(AttributeName = "Date")]
        public required string Date { get; set; }
    }
}
