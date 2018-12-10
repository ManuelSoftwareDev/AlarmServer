using System.Xml.Serialization;

namespace AlarmServer.Core
{
    public class AlarmClient
    {
        [XmlAttribute("id")]
        public string ClientIdentifier { get; set; }
        [XmlAttribute("label")]
        public string ClientLabel { get; set; }
    }
}
