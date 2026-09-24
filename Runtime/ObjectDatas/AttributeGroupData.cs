using System.Xml.Serialization;

namespace EnervaCore {
    public class AttributeGroupData : ECObjectData {
        [XmlElement("Attribute")]
        public AttributeGroupDataRow[] Attributes;

        public AttributeGroupData() {
            if (string.IsNullOrEmpty(SpawnType)) {
                SpawnType = "EnervaCore.Objects.AttributeGroup"; // choose the default you need
            }
        }
    }

    
    public class AttributeGroupDataRow {
        [XmlAttribute]
        public string id { get; set; }
    }
}