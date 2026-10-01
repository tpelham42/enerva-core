using System.Collections.Generic;
using System.Xml.Serialization;

namespace EnervaCore {
    /// <summary>
    /// The LootTableData class represents a loot table that defines how loot items are rolled and generated.
    /// It inherits from ECObjectData and contains properties for the number of rolls to perform and a list of loot items. 
    /// The Result method can be used to generate a dictionary of loot results based on the specified roll count.
    /// </summary>
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