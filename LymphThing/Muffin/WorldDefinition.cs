using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mogre;

namespace PonykartParsers {
	public class WorldDefinition : TokenHolder {
		public string Name { get; private set; }
		public LevelType Type { get; private set; }
		public ICollection<ThingBlock> ThingBlocks { get; protected set; }

		public WorldDefinition(string name) {
			Name = name;
			SetUpDictionaries();
		}

		public override void SetUpDictionaries() {
			base.SetUpDictionaries();
			ThingBlocks = new Collection<ThingBlock>();
		}

		public override void Finish() {
			foreach (ThingBlock tb in ThingBlocks)
				tb.Finish();
		}

		public override void Dispose() {
			foreach (ThingBlock tb in ThingBlocks)
				tb.Dispose();
			base.Dispose();
		}
	}

	public class ThingBlock : TokenHolder {
		/// <summary>
		/// The name of the .thing file this corresponds with
		/// </summary>
		public string ThingName { get; private set; }
		public WorldDefinition Owner { get; private set; }

		public ThingBlock(string thingName, WorldDefinition owner) {
			ThingName = thingName;
			Owner = owner;
			SetUpDictionaries();
		}

		public ThingBlock(string thingName, Vector3 position) {
			ThingName = thingName;
			SetUpDictionaries();
			VectorTokens["position"] = position;
		}

		public override void Finish() { }
	}
}
