
namespace PonykartParsers {
	/// <summary>
	/// Represents a Ribbon { } block in a .thing file
	/// </summary>
	public class BillboardBlock : TokenHolder {
		public ThingDefinition Owner { get; protected set; }

		public BillboardBlock(ThingDefinition owner) {
			Owner = owner;
			SetUpDictionaries();
		}

		public override void Finish() { }
	}
}
