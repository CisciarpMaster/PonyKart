using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lymph.Core {
	public class Inventory {
		/// <summary>
		/// The items in this inventory
		/// </summary>
		private ICollection<Item> Items;
		/// <summary>
		/// Maximum size of this inventory - puts a constraint on splitting items and such
		/// </summary>
		public int MaxSize { get; set; }

		/// <summary>
		/// Constructor for an empty inventory
		/// </summary>
		public Inventory() {
			Items = new Collection<Item>();
		}

		/// <summary>
		/// Constructor for an inventory with the given items
		/// </summary>
		/// <param name="items">The items that are initially in the inventory</param>
		public Inventory(params Item[] items) {
			// need to tell all the items they're in this inventory now
			for (int a = 0; a < items.Length; a++)
				items[a].Owner = this;

			Items = new Collection<Item>(items);
		}

		/// <summary>
		/// Adds an item to this inventory. The item's owner is set to this inventory if it is successful.
		/// If the item has charges, it will 
		/// </summary>
		/// <param name="item">The item you want to add</param>
		/// <returns>Whether adding the item was successful or not</returns>
		public bool AddItem(Item item) {
			// don't want to add the item if it's already in the inventory
			if (Items.Contains(item)) {
				Launch.Log("This inventory already contains that item!");
				return false;
			}
			// check to see if there are any items we can try merging this with
			if (item.HasCharges) {
				// get the items that match this one's type that have charges
				IList<Item> list = Items.Where((_item, _index) => _item.Type == item.Type && _item.Charges == _item.MaxCharges).ToList();
				// check that it isn't empty
				if (list.Count > 0) {
					// okay now we have some different scenarios
					// 1) We merge this item into the existing one and that is that
					// 2) We merge this item into the existing one, but there are still charges left and there are no other existing ones to merge with
					// 3) We merge this item into the existing one, but there are still charges left and there are other existing ones to merge with
					// 4) We merge this item into the existing one, but there are still charges left, no other stacks, and the inventory is full

					foreach (Item existingItem in list) {
						Merge(existingItem, item);

						// case 1
						if (item.Charges == 0)
							return true;
						// case 4
						if (IsFull) {
							Launch.Log("Item added to the inventory, but some charges could not be added");
							return true;
						}
						// case 3 - continue
					}
				}
				// case 2, also happens if no mergeable stacks were found
				if (!IsFull) {
					Items.Add(item);
					item.Owner = this;
					return true;
				}
				return false;
			}
			// can't add any more items if it's full. But we also want to try merging an item into the inventory if it is possible
			if (IsFull) {
				Launch.Log("Could not add item to this inventory because it is full!");
				return false;
			}
			// it's an item without charges and the inventory is not full, so add it!
			Items.Add(item);
			item.Owner = this;
			return true;
		}

		/// <summary>
		/// Remove an item from the inventory but does not destroy it.
		/// </summary>
		/// <param name="item">The item you want to remove</param>
		/// <returns>Whether removing the item was successful or not</returns>
		public bool RemoveItem(Item item) {
			if (Items.Contains(item)) {
				Items.Remove(item);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets an item from the inventory with the specified type.
		/// </summary>
		/// <param name="type">The type to search for. It's case sensitive!</param>
		/// <returns>The item, or null if one wasn't found</returns>
		public Item GetItem(string type) {
			return Items.First((i) => i.Type == type);
		}

		/// <summary>
		/// Gets all of the items from the inventory with the specified type.
		/// </summary>
		/// <param name="type">The type to search for. It's case sensitive!</param>
		/// <returns>The items, or null if one wasn't found</returns>
		public IEnumerable<Item> GetItems(string type) {
			return Items.Where((i) => i.Type == type);
		}

		/// <summary>
		/// Gets all of the items from the inventory that match the specified condition.
		/// </summary>
		/// <param name="condition">The condition to use for filtering items</param>
		/// <returns>The items, or null if one wasn't found</returns>
		public IEnumerable<Item> GetItems(Func<Item, bool> condition) {
			return Items.Where(condition);
		}

		/// <summary>
		/// Does this inventory have an item of the specified type?
		/// </summary>
		/// <param name="type">The type to search for. It's case sensitive!</param>
		/// <returns>Whether the inventory contains an item of that type</returns>
		public bool HasItem(string type) {
			return Items.Any((i) => i.Type == type);
		}

		/// <summary>
		/// Merges two stacks of items.
		/// Conditions:
		/// - The two items are in the inventory
		/// - The two items have charges
		/// - The two items are of the same type
		/// </summary>
		/// <param name="destination">If the conditions pass, the two items are merged into this one.</param>
		/// <param name="source">If the conditions pass, the contents of this item will be merged into the destination item.</param>
		/// <returns>Whether the merging was successful or not</returns>
		public bool Merge(Item destination, Item source) {
			// check if they're in the inventory
			if (!Items.Contains(destination) || !Items.Contains(source)) {
				Launch.Log("Item merging failed - the two items are not in the inventory!");
				return false;
			}
			// check if they both have charges
			if (destination.Charges == -1 || source.Charges == -1) {
				Launch.Log("Item merging failed - one of the two items does not have charges!");
				return false;
			}
			// check if they're the same type
			if (destination.Type != source.Type) {
				Launch.Log("Item merging failed - the two items are not of the same type!");
				return false;
			}

			int totalCharges = destination.Charges + source.Charges;

			// if they can merge into one stack
			if (totalCharges <= destination.MaxCharges) {
				destination.Charges = totalCharges;
				// get rid of the source since it has 0 charges
				source.Charges = 0;
				source.Destroy();
			}
			// if they can't merge into one stack, fill up the destination with as many charges as it can get from the source item
			else {
				// dest: 7 and source: 6
				// max is 10
				// then we can transfer 3
				int maxToTransfer = destination.MaxCharges - destination.Charges;
				destination.Charges = destination.MaxCharges;
				source.Charges -= maxToTransfer;
			}
			return true;
		}

		public bool IsFull {
			get { return Items.Count == MaxSize; }
		}
	}
}
