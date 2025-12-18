using System.Xml.Serialization;

namespace CbrDailyWorker.WebModels
{
    [XmlRoot(ElementName = "Valuta")]
    public class CurrencyListModel
    {
        [XmlElement(ElementName = "Item")]
        public required List<CurrencyModel> Currencies { get; set; }
    }
}