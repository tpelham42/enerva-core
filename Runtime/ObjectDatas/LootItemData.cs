using System.Xml.Serialization;
namespace EnervaCore {
    public class LootItemData : ECObjectData {
        
        [XmlAttribute("item_id")]
        public string ItemID { get; set; }

        [XmlAttribute("quantity")]
        public int Quantity { get; set; } = 1; // Default quantity to drop, can be overridden by MinQuantity and MaxQuantity

        [XmlAttribute("min_quantity")]
        public int MinQuantity { get; set; } = 0; // Zero means no minimum, any quantity is allowed

        [XmlAttribute("max_quantity")]
        public int MaxQuantity { get; set; } = 0; // Zero means no maximum, any quantity is allowed

        [XmlAttribute("weight")]
        public int DropWeight { get; set; } = 1; // Weight for random selection, higher means more likely to drop        

        public bool IsUnique { get; set; } = false; // If true, only one instance of this item can exist in the game world at a time

        public LootItemData() {
            if (string.IsNullOrEmpty(SpawnType)) {
                SpawnType = "EnervaCore.Objects.LootItem"; 
            }
        }
    }
}