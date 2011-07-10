using System;
using Lymph.Actors;

namespace Lymph.Core {
	public class Item {
		/// <summary>
		/// What item is it?
		/// </summary>
		public string Type { get; set; }
		/// <summary>
		/// Set this to -1 for a permanent item
		/// </summary>
		public int Charges { get; set; }
		/// <summary>
		/// Set this to -1 for a permanent item
		/// </summary>
		public int MaxCharges { get; set; }
		/// <summary>
		/// The inventory this item is in
		/// </summary>
		public Inventory Owner { get; set; }
		/// <summary>
		/// What does this item do when it is used?
		/// </summary>
		public Func<Thing> Effect { get; set; }
		/// <summary>
		/// How much is this item worth?
		/// </summary>
		public int Cost { get; set; }

		/// <summary>
		/// Constructor for an item with charges
		/// </summary>
		/// <param name="type"></param>
		/// <param name="charges"></param>
		/// <param name="maxCharges"></param>
		/// <param name="owner"></param>
		public Item(string type, int charges, int maxCharges, Inventory owner) {
			Type = type;
			Charges = charges;
			MaxCharges = maxCharges;
			Owner = owner;
		}

		/// <summary>
		/// Constructor for an item with no charges
		/// </summary>
		/// <param name="type"></param>
		/// <param name="owner"></param>
		public Item(string type, Inventory owner) : this(type, -1, -1, owner) {
		}

		/// <summary>
		/// Another contructor - this one is so you can do stuff like this:
		/// new Item {
		///		Type = "blah"
		///		Charges = 5
		///		MaxCharges = 10
		///		Inventory = foo
		///	}
		/// </summary>
		public Item() {
		}

		/// <summary>
		/// Is this an item with charges?
		/// </summary>
		/// <returns></returns>
		public bool HasCharges {
			get {
				return Charges > -1;
			}
		}

		/// <summary>
		/// Destroy this item
		/// </summary>
		public void Destroy() {
			if (Owner != null)
				Owner.RemoveItem(this);
		}

		/// <summary>
		/// Calls this item's effect function, if it has one
		/// </summary>
		public void Use() {
			if (Effect != null)
				Effect.Invoke();
		}
	}
}
