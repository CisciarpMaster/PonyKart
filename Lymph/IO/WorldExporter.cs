using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Lymph.Actors;
using Lymph.Levels;
using Mogre;

namespace Lymph.IO {
	/// <summary>
	/// This class exports the current world state to a file so we can load it later.
	/// At the moment it just saves templates, which is kinda stupid, so we'll need to change it
	/// so it saves Things from the world instead.
	/// </summary>
	public class WorldExporter {
		CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

		/// <summary>
		/// Exports the current level's world state to a file using the world grammar.
		/// </summary>
		/// <param name="level">The level you want to save</param>
		public void Export(Level level) {

			// media/worlds/LevelName.sav
			string filePath = Settings.Default.SaveFileLocation + level.Name + Settings.Default.SaveFileExtension;

			Launch.Log("[World Exporter] Exporting level to file: " + filePath);

			using (StreamWriter sw = File.CreateText(filePath)) {
				// save our flags
				sw.WriteLine("Flags:");
				foreach (KeyValuePair<string, bool> kvp in level.Flags) {
					sw.WriteLine("\t" + kvp.Key + " = " + kvp.Value.ToString().ToLower());
				}

				// save our numbers
				sw.WriteLine("Numbers:");
				foreach (KeyValuePair<string, float> kvp in level.Numbers) {
					sw.WriteLine("\t" + kvp.Key + " = " + kvp.Value.ToString(culture));
				}

				// save entities
				sw.WriteLine("Entities:");
				foreach (KeyValuePair<string, Thing> kvp in level.Things)
				{
					// don't want to save the player! That goes somewhere else~
					if (kvp.Value.GetType() == typeof(Player))
						continue;

					// normally the type gives us "Lymph.Actors.Thing", for example, and we only want the last bit
					string type = kvp.Value.GetType().ToString();
					type = type.Substring(type.LastIndexOf(".") + 1);
					sw.WriteLine("\tActor (" + type + ") {");

					var flagTokens   = kvp.Value.GetOptionalFlags();
					var numberTokens = kvp.Value.GetOptionalNumbers();
					var stringTokens = kvp.Value.GetOptionalStrings();
					var vectorTokens = kvp.Value.GetOptionalVectors();

					// required tokens
					sw.WriteLine("\t\tName = \"" + kvp.Value.Name + "\"");

					Vector3 pos = kvp.Value.Node.Position;
					sw.WriteLine("\t\tPosition = " + pos.x.ToString(culture) + ", " + pos.y.ToString(culture) + ", " + pos.z.ToString(culture));


					// here we get all of the overridden tokens.
					// if nothing has been overridden, overridesBlock will still be "" at the end of these four
					// foreach loops. If that is the case then we don't need to write an overrides block in
					// our save file.
					string overridesBlock = "";
					foreach (KeyValuePair<string, bool> bools in flagTokens) {
						overridesBlock += "\t\t\t" + bools.Key + " = " + bools.Value + "\r\n";
					}
					foreach (KeyValuePair<string, float> floats in numberTokens) {
						overridesBlock += "\t\t\t" + floats.Key + " = " + floats.Value.ToString(culture) + "\r\n";
					}
					foreach (KeyValuePair<string, string> strings in stringTokens) {
						overridesBlock += "\t\t\t" + strings.Key + " = \"" + strings.Value + "\"\r\n";
					}
					foreach (KeyValuePair<string, Vector3> vectors in vectorTokens) {
						overridesBlock += "\t\t\t" + vectors.Key + " = " + vectors.Value.x.ToString(culture) +
							", " + vectors.Value.y.ToString(culture) + ", " + vectors.Value.z.ToString(culture) + "\r\n";
					}

					// do we have any optional tokens?
					if (overridesBlock != "")
					{
						sw.WriteLine("\t\tOverrides");
						sw.WriteLine("\t\t{");

						sw.Write(overridesBlock);

						// close the optional tokens block
						sw.WriteLine("\t\t}");
					}
					// close the entities block
					sw.WriteLine("\t}");
				}
				sw.Flush();
				sw.Close();
			}
		}
	}
}
