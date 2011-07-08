using System.Collections.Generic;
using Mogre;

namespace Lymph {
	/// <summary>
	/// "Template" for a Thing
	/// </summary>
	public class ThingTemplate {
		public string Type { get; private set; }
		public int ID { get; private set; }

		public IDictionary<string, string> StringTokens { get; private set; }
		public IDictionary<string, float> FloatTokens { get; private set; }
		public IDictionary<string, bool> BoolTokens { get; private set; }
		public IDictionary<string, Vector3> VectorTokens { get; private set; }

		/// <summary>
		/// Constructor without required tokens - you need to add them manually.
		/// Generates an ID number too.
		/// </summary>
		/// <param name="type">Essentially a class name</param>
		public ThingTemplate(string type) {
			Type = type;
			StringTokens = new Dictionary<string, string>();
			FloatTokens = new Dictionary<string, float>();
			BoolTokens = new Dictionary<string, bool>();
			VectorTokens = new Dictionary<string, Vector3>();
			ID = IDs.New;
		}

		/// <summary>
		/// Constructor with required tokens - add more as necessary.
		/// Generates an ID number too.
		/// </summary>
		/// <param name="type">Essentially a class name</param>
		/// <param name="name">The name of this thing</param>
		/// <param name="spawnPosition">Where should this thing spawn?</param>
		public ThingTemplate(string type, string name, Vector3 spawnPosition) : this(type) {
			StringTokens["Name"] = name;
			VectorTokens["Position"] = spawnPosition;
		}

		public string Name {
			get { return StringTokens["Name"]; }
		}
	}
}
