using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnervaCore {
    public class Inventory : ECObject {

        public enum InventoryMatchMode { HasExact, HasAtleast }
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

        public void AddItems(Inventory otherInventory) {
            if (otherInventory == null)
                return;

            foreach (var item in otherInventory.Items) {
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

        public void RemoveItems(Inventory otherInventory) {
            if (otherInventory == null)
                return;

            foreach(var item in otherInventory.Items) {
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

        public bool HasItems(Inventory otherInventory, InventoryMatchMode MatchMode = InventoryMatchMode.HasAtleast) {
            foreach(var item in otherInventory.Items) {
                if (HasItem(item.Key, item.Value, MatchMode) == false)
                    return false;
            }

            return true;
        }

        public bool HasItems(Dictionary<string, int> Items, InventoryMatchMode MatchMode = InventoryMatchMode.HasAtleast) {
            foreach(var item in Items) {
                if (HasItem(item.Key, item.Value, MatchMode) == false)
                    return false;
            }

            return true;
        }

        public bool HasItem(string itemID, int Quantity, InventoryMatchMode MatchMode = InventoryMatchMode.HasAtleast) {
            switch (MatchMode) {
                case InventoryMatchMode.HasAtleast:
                    return GetItemQuantity(itemID) >= Quantity;                    

                case InventoryMatchMode.HasExact:
                    return GetItemQuantity(itemID) == Quantity;
            }

            return false;
        }

        public override string ToString() {
            string invString = "";
            foreach(var item in Items) {
                invString += $"\n{item.Key}: {item.Value}";
            }

            return invString;
        }
    }

    
}