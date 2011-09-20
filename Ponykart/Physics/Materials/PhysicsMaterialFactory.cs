using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Ponykart.Properties;

namespace Ponykart.Physics {
	public class PhysicsMaterialFactory {
		IDictionary<string, PhysicsMaterial> materials;

		CultureInfo culture = CultureInfo.InvariantCulture;

		public PhysicsMaterialFactory() {
			materials = new Dictionary<string, PhysicsMaterial>();

			ReadMaterialsFromFiles();
		}

		/// <summary>
		/// Go through our media/physicsmaterials/ directory and find all of the material definitions we have, then make objects out
		/// of them and add them to our dictionary.
		/// </summary>
		public void ReadMaterialsFromFiles() {
			// since we can run this whenever (like when we're tweaking files), we want to clear this first
			materials.Clear();

			// get all of the filenames of the files in media/physicsmaterials
			IEnumerable<string> files = Directory.EnumerateFiles(Settings.Default.PhysicsMaterialFileLocation, "*" + Settings.Default.PhysicsMaterialFileExtension);

			foreach (string filename in files) {
				// our matname is the filename minus the file extension
				string matname = filename.Remove(filename.IndexOf(Settings.Default.PhysicsMaterialFileExtension));
				// this gets rid of the "media/physicsmaterials/" bit
				matname = matname.Replace(Settings.Default.PhysicsMaterialFileLocation, string.Empty);

				string matcontents = string.Empty;

				// open up the file and read everything from it
				using (var stream = File.Open(filename, FileMode.Open)) {
					using (var reader = new StreamReader(stream)) {
						while (!reader.EndOfStream) {
							matcontents += reader.ReadLine() + "\n";
						}
						reader.Close();
					}
				}

				materials[matname] = ParseMaterial(matname, matcontents);
			}
		}

		/// <summary>
		/// Take the contents of a .physicsmaterial file and puts it in an object
		/// </summary>
		private PhysicsMaterial ParseMaterial(string matname, string matcontents) {
			PhysicsMaterial mat = new PhysicsMaterial();

			// get rid of whitespace
			matcontents.Replace(" ", string.Empty);
			matcontents.Replace("\t", string.Empty);

			string[] splits = matcontents.Split('\n');

			foreach (string line in splits) {
				// ignore newlines and comments
				if (line.Length == 0 || line.StartsWith("//"))
					continue;

				string[] splitline = line.Split('=');
				string prop = splitline[0].TrimEnd().ToLower(culture);

				if (prop == "friction")
					mat.Friction = float.Parse(splitline[1], culture);
				else if (prop == "bounciness")
					mat.Bounciness = float.Parse(splitline[1], culture);
				else if (prop == "angulardamping")
					mat.AngularDamping = float.Parse(splitline[1], culture);
				else if (prop == "lineardamping")
					mat.LinearDamping = float.Parse(splitline[1], culture);
			}

			return mat;
		}

		/// <summary>
		/// Gets a material from the dictionary.
		/// </summary>
		/// <returns>If the material with that name was not found, this just returns the default material.</returns>
		public PhysicsMaterial GetMaterial(string materialName) {
			PhysicsMaterial mat;
			if (materials.TryGetValue(materialName, out mat))
				return mat;
			else {
				Launch.Log("[PhysicsMaterialFactory] That material did not exist! Applying default...");
				return materials["Default"];
			}
		}
	}
}
