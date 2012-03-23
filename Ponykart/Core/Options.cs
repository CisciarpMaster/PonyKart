using System;
using System.Collections.Generic;
using System.IO;
using Mogre;

namespace Ponykart.Core {
	public static class Options {
		private static IDictionary<string, string> dict;
		private static IDictionary<string, string> defaults;
		public static ModelDetailOption ModelDetail;
		public static ShadowDetailOption ShadowDetail;

		/// <summary>
		/// Creates the folder and file if they don't exist, and either prints some data to it (if it doesn't exist) or reads from it (if it does)
		/// </summary>
		public static void Initialise() {
			SetupDictionaries();
#if DEBUG
			string pkPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ponykart";

			// if a folder doesn't exist there, create it
			if (!Directory.Exists(pkPath))
				Directory.CreateDirectory(pkPath);

			string optionsPath = pkPath + "\\options.ini";
#else
			string optionsPath = "options.ini";
#endif

			// create it if the file doesn't exist, and write out some defaults
			if (!File.Exists(optionsPath)) {
				using (FileStream stream = File.Create(optionsPath)) {
					using (StreamWriter writer = new StreamWriter(stream)) {
						foreach (KeyValuePair<string, string> pair in defaults) {
							writer.WriteLine(pair.Key + "=" + pair.Value);
						}
						writer.Flush();
						writer.Close();
					}
					stream.Flush();
					stream.Close();
				}
				ModelDetail = ModelDetailOption.Medium;
			}
			// otherwise we just read from it
			else {
				ConfigFile cfile = new ConfigFile();
				cfile.Load(optionsPath, "=", true);

				ConfigFile.SectionIterator sectionIterator = cfile.GetSectionIterator();
				sectionIterator.MoveNext();
				foreach (KeyValuePair<string, string> pair in sectionIterator.Current) {
					dict[pair.Key] = pair.Value;
				}
				ModelDetail = (ModelDetailOption) Enum.Parse(typeof(ModelDetailOption), dict["ModelDetail"], true);
				ShadowDetail = (ShadowDetailOption) Enum.Parse(typeof(ShadowDetailOption), dict["ShadowDetail"], true);

				cfile.Dispose();
				sectionIterator.Dispose();
			}

#if DEBUG
			// since we sometimes add new options, we want to make sure the .ini file has all of them
			Save();
#endif
		}

		private static void SetupDictionaries() {
			// set up our dictionary with some default stuff in it
			defaults = new Dictionary<string, string>();
			// 0, 2, 4, or 8
			defaults["FSAA"] = "0";
			// Fastest or Accurate
			defaults["Floating-point mode"] = "Fastest";
			// Yes or No
			defaults["Full Screen"] = "No";
			// Yes or No
			defaults["VSync"] = "Yes";
			// 1, 2, 3, or 4
			defaults["VSync Interval"] = "1";
			defaults["Video Mode"] = "1280 x 800 @ 32-bit colour";
			// Yes or No
			defaults["sRGB Gamma Conversion"] = "No";
			// Yes or No
			defaults["Music"] = "No";
			defaults["Sounds"] = "No";
			defaults["Ribbons"] = "No";
			// Low or High
			defaults["ModelDetail"] = "Medium";
			defaults["ShadowDetail"] = "Some";
			defaults["ShadowDistance"] = "40";
			defaults["Twh"] = "No";
			// copy it into the regular dictionary
			dict = new Dictionary<string, string>(defaults);
		}

		/// <summary>
		/// Writes out the current settings to our options file
		/// </summary>
		public static void Save() {
#if DEBUG
			string optionsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ponykart\\options.ini";
#else
			string optionsPath = "options.ini";
#endif
			dict["ModelDetail"] = ModelDetail.ToString();
			dict["ShadowDetail"] = ShadowDetail.ToString();

			using (FileStream stream = File.Create(optionsPath)) {
				using (StreamWriter writer = new StreamWriter(stream)) {
					foreach (KeyValuePair<string, string> pair in dict) {
						writer.WriteLine(pair.Key + "=" + pair.Value);
					}
				}
			}
		}

		/// <summary>
		/// Gets an option.
		/// </summary>
		public static string Get(string keyName) {
			if (string.Equals(keyName, "ModelDetail", StringComparison.InvariantCultureIgnoreCase))
				throw new ArgumentException("Use the Options.ModelDetail enum instead of this method!", "keyName");
			else if (string.Equals(keyName, "ShadowDetail", StringComparison.InvariantCultureIgnoreCase))
				throw new ArgumentException("Use the Options.ShadowDetail enum instead of this method!", "keyName");

			return dict[keyName];
		}

		/// <summary>
		/// Gets an option as a boolean.
		/// </summary>
		public static bool GetBool(string keyName) {
			string value = dict[keyName];
			if (value == "Yes")
				return true;
			else if (value == "No")
				return false;
			else
				throw new ArgumentException("That key does not represent a boolean option!", "keyName");
		}

		/// <summary>
		/// Gets the dimensions of the render window
		/// </summary>
		/// <param name="height">The height of the window</param>
		/// <param name="width">The width of the window</param>
		public static void GetWindowDimensions(out uint width, out uint height) {
			string videoMode = dict["Video Mode"];
			string[] split = videoMode.Split(' ');

			width = uint.Parse(split[0]);
			height = uint.Parse(split[2]);
		}
	}
}
