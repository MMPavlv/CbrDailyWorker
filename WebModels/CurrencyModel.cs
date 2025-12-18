using System.Xml.Serialization;

namespace CbrDailyWorker.WebModels
{
    [XmlRoot(ElementName = "Item")]
    public record CurrencyModel
    {
        [XmlAttribute(AttributeName = "ID")]
        public required string Id { get; set; }

        [XmlElement(ElementName = "Name")]
        public required string Name { get; set; }

        [XmlElement(ElementName = "EngName")]
        public required string EngName { get; set; }

        [XmlElement(ElementName = "Nominal")]
        public required uint Nominal { get; set; }

        [XmlElement(ElementName = "ParentCode")]
        public required string ParentCode { get; set; }
    }
}
