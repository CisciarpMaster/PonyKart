using System.Collections.Generic;

namespace LymphThing {
	public class ThingDatabase {
		IDictionary<string, ThingDefinition> Definitions;

		public ThingDatabase() {
			Definitions = new Dictionary<string, ThingDefinition>();
		}

		public ThingDefinition GetThingDefinition(string name) {
			ThingDefinition def;
			if (Definitions.TryGetValue(name, out def))
				return def;
			else {
				def = new ThingImporter().Parse(name);
				Definitions.Add(name, def);
				return def;
			}
		}

		public void ClearDatabase() {
			Definitions.Clear();
		}
	}
}
