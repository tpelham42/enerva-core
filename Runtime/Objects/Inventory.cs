using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnervaCore {
    public class Inventory : ECObject {
        public Inventory() { }

        public Dictionary<string, int> Items { get; set; } = new Dictionary<string, int>();

        public Action<string, int> OnItemQuantityChanged;
        public Action OnItemsCleared;


        public void ClearInventory() {
            Items.Clear();

            OnItemsCleared?.Invoke();
        }

        public void AddItem(string itemID, int quantity) {
            if (Items.ContainsKey(itemID)) {
                Items[itemID] += quantity;
            }
            else {
                Items[itemID] = quantity;
            }

            OnItemQuantityChanged?.Invoke(itemID, GetItemQuantity(itemID));
        }

        public void AddItems(Dictionary<string, int> ItemsToAdd) {
            foreach(var item in ItemsToAdd) {
                AddItem(item.Key, item.Value);
            }
        }

        public void RemoveItem(string itemID, int Quantity=1) {
            if (Items.ContainsKey(itemID)) {
                Items[itemID] -= Quantity;

                if (Items[itemID] <= 0) {
                    Items.Remove(itemID);
                }

                OnItemQuantityChanged?.Invoke(itemID, GetItemQuantity(itemID));
            }            
        }

        public void RemoveItems(Dictionary<string, int> ItemsToRemove) {
            foreach (var item in ItemsToRemove) {
                RemoveItem(item.Key, item.Value);
            }
        }

        public int GetItemQuantity(string itemID) {
            if (Items.ContainsKey(itemID)) {
                return Items[itemID];
            }
            else {
                return 0;
            }
        }
    }
}