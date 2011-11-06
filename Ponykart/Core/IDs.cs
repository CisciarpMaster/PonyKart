using System;
namespace Ponykart {
	/// <summary>
	/// Handles getting ID numbers
	/// </summary>
	public abstract class IDs {
		private static long Counter = 0;
		private static readonly Random _rand = new Random();

		/// <summary>
		/// Just get this property and it'll give you a new ID number.
		/// Note that it doesn't reset to 0 when we load a new level. (Should we fix this?)
		/// </summary>
		/// <example>
		/// something.IDNumber = IDs.New;
		/// </example>
		public static long Incremental {
			get {
				return Counter++;
			}
		}

		public static int Random {
			get { return _rand.Next(); }
		}
	}
}
