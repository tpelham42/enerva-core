using System.Collections.Generic;
using System.Xml.Serialization;

namespace EnervaCore {
    public class LootTableData : ECObjectData {
        
        [XmlAttribute("roll_count")]
        public int RollCount { get; set; } = 1; // Number of times to roll for loot items. This is Defaulted to 1, but can be set to any positive integer. If set to 0, no items will be rolled.

        [XmlArray("LootList")]
        [XmlArrayItem("LootEntry")]
        public List<LootItemData> LootItems { get; set; } = new List<LootItemData>();

        public LootTableData() {
            if (string.IsNullOrEmpty(SpawnType)) {
                SpawnType = "EnervaCore.Objects.LootTable"; 
            }
        }

        public Dictionary<string, int> Result(int rollCount=0) {
            if(rollCount <= 0) {
                //Use default roll count if not specified or invalid
                rollCount = RollCount;
            }

            Dictionary<string, int> lootResult = new Dictionary<string, int>();

            return lootResult;
        }
    }

   
}