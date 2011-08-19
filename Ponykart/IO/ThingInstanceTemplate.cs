using Mogre;

namespace Ponykart.IO {
	/// <summary>
	/// "Template" for a Thing
	/// </summary>
	public class ThingInstanceTemplate : TokenHolder {
		public string Type { get; private set; }
		public int ID { get; private set; }

		/// <summary>
		/// Constructor without required tokens - you need to add them manually.
		/// Generates an ID number too.
		/// </summary>
		/// <param name="type">Essentially a class name</param>
		public ThingInstanceTemplate(string type) {
			Type = type;
			ID = IDs.New;
			SetUpDictionaries();
		}

		public override void Finish() { }

		/// <summary>
		/// Constructor with required tokens - add more as necessary.
		/// Generates an ID number too.
		/// </summary>
		/// <param name="type">Essentially a class name</param>
		/// <param name="name">The name of this thing</param>
		/// <param name="spawnPosition">Where should this thing spawn?</param>
		public ThingInstanceTemplate(string type, string name, Vector3 spawnPosition) : this(type) {
			StringTokens["name"] = name;
			VectorTokens["position"] = spawnPosition;
		}

		public ThingInstanceTemplate(string type, Vector3 spawnPosition) : this(type, type, spawnPosition) { }

		public string Name {
			get { return StringTokens["name"]; }
		}
	}
}
