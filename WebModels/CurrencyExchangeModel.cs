using System.Xml.Serialization;

namespace CbrDailyWorker.WebModels
{
    [XmlRoot(ElementName = "Valute")]
    public record CurrencyExchangeModel
    {
        [XmlAttribute(AttributeName = "ID")]
        public required string Id { get; set; }

        [XmlElement(ElementName = "NumCode")]
        public required uint NumCode { get; set; }

        [XmlElement(ElementName = "CharCode")]
        public required string CharCode { get; set; }

        [XmlElement(ElementName = "Nominal")]
        public required uint Nominal { get; set; }

        [XmlElement(ElementName = "Name")]
        public required string Name { get; set; }

        [XmlElement(ElementName = "Value")]
        public required string Value { get; set; }

        [XmlElement(ElementName = "VunitRate")]
        public required string VunitRate { get; set; }
    }
}
