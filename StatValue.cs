using System.Xml.Serialization;

namespace CSMud
{
    public class StatValue
    {
        [XmlElement]
        public string Stat { get; set; }
        [XmlElement]
        public int Value { get; set; }

        public void Deconstruct (out string stat, out int value)
        {
            stat = Stat;
            value = Value;
        }
    }
}
