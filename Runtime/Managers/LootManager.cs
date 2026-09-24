using System.Collections.Generic;
using System.Diagnostics;

namespace EnervaCore {
    public class LootManager : IManager, IManagerGameStart {
        private Dictionary<string, LootTableData> _lootTables = new Dictionary<string, LootTableData>();

        //Called after all other managers have been initialized.
        public void OnGameStart() {
            InitLootTables();
        }

        public void Initialize() {
            
        }

        public void OnDestroyed() {
            
        }

        void InitLootTables() {
            HashSet<ECObjectData> lootTables = ECM.DB.GetTemplatesByType<LootTableData>();
            if (lootTables == null) {
                UnityEngine.Debug.LogError(this + " :: OnGameManagerInitialized() :: No LootTableData templates found in the database.");
                return;
            }

            foreach (var lootTable in lootTables) {
                if (lootTable is LootTableData lootTableData) {
                    _lootTables[lootTableData.ID] = lootTableData;
                }
            }
        }

        public Dictionary<string, int> GetLootResult(string TableId, int rollCount=0, string seedContext="") {
            InitLootTables();

            if (_lootTables.ContainsKey(TableId) == false || _lootTables[TableId] == null) {
                UnityEngine.Debug.LogError(this + " :: GetLootResult() :: LootTableData with ID " + TableId + " does not exist.");
                return new Dictionary<string, int>();
            }            

            LootTableData lootTable = _lootTables[TableId];

            if (rollCount <= 0) {
                //Use default roll count if not specified or invalid
                rollCount = lootTable.RollCount;
            }

            int seed = GetSeedForTable(TableId, seedContext);
            var rng = new System.Random(seed);
            var items = lootTable.LootItems;
            Dictionary<string, int> result = new Dictionary<string, int>();

            // Precompute total weight
            int totalWeight() {
                int tw = 0;
                for (int i = 0; i < items.Count; i++) {
                    var w = items[i].DropWeight;
                    if (w > 0) tw += w;
                }
                return tw;
            }

            for (int roll = 0; roll < rollCount; roll++) {
                int tw = totalWeight();
                if (tw <= 0) break; // nothing to roll
                int r = rng.Next(tw); // [0, tw)
                int acc = 0;
                LootItemData chosen = null;
                for (int i = 0; i < items.Count; i++) {
                    acc += items[i].DropWeight;
                    if (r < acc) {
                        chosen = items[i];
                        // If unique, zero out weight so it won't be selected again
                        if (chosen.IsUnique) items[i].DropWeight = 0;
                        break;
                    }
                }
                if (chosen == null) continue;

                // determine quantity
                int qty;
                if (chosen.MinQuantity > 0 || chosen.MaxQuantity > 0) {
                    int minq = chosen.MinQuantity > 0 ? chosen.MinQuantity : chosen.Quantity;
                    int maxq = chosen.MaxQuantity > 0 ? chosen.MaxQuantity : chosen.Quantity;
                    if (maxq < minq) maxq = minq;
                    qty = (minq == maxq) ? minq : rng.Next(minq, maxq + 1);
                }
                else {
                    qty = chosen.Quantity;
                }

                if (result.ContainsKey(chosen.ItemID)) result[chosen.ItemID] += qty;
                else result[chosen.ItemID] = qty;

                UnityEngine.Debug.Log($"Roll {roll}: picked {chosen.ItemID} x{qty} (seed component {r})");
            }

            return lootTable.Result(rollCount);
        }

        int GetSeedForTable(string TableId, string seedContext) {
            if (_lootTables.ContainsKey(TableId) == false || _lootTables[TableId] == null) {
                UnityEngine.Debug.LogError(this + " :: GetSeedForTable() :: LootTableData with ID " + TableId + " does not exist.");
                return 0;
            }
            
            LootTableData lootTable = _lootTables[TableId];
            
            int seed;
            var bytes = System.Text.Encoding.UTF8.GetBytes(lootTable.ID + "|" + seedContext);
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                var hash = md5.ComputeHash(bytes);
                seed = System.BitConverter.ToInt32(hash, 0);
            }

            return seed;
        }

    }
}