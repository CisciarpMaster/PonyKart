namespace Lymph {
	/// <summary>
	/// Handles getting ID numbers
	/// </summary>
	public class IDs {
		private static int Counter = 0;

		/// <summary>
		/// Just get this property and it'll give you a new ID number.
		/// Note that it doesn't reset to 0 when we load a new level. (Should we fix this?)
		/// </summary>
		/// <example>
		/// something.IDNumber = IDs.New;
		/// </example>
		public static int New {
			get {
				return Counter++;
			}
		}
	}
}
