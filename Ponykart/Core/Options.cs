using System;
using System.Collections.Generic;
using System.IO;
using Mogre;

namespace Ponykart.Core {
	public static class Options {
		private static IDictionary<string, string> dict;
		private static IDictionary<string, string> defaults;

		/// <summary>
		/// Creates the folder and file if they don't exist, and either prints some data to it (if it doesn't exist) or reads from it (if it does)
		/// </summary>
		public static void Initialise() {
			// set up our dictionary with some default stuff in it
			defaults = new Dictionary<string, string>();
			#region defaults
			defaults["FSAA"] = "0";
			defaults["Floating-point mode"] = "Fastest";
			defaults["Full Screen"] = "No";
			defaults["VSync"] = "Yes";
			defaults["VSync Interval"] = "1";
			defaults["Video Mode"] = "1280 x 800 @ 32-bit colour";
			defaults["sRGB Gamma Conversion"] = "No";
			defaults["Music"] = "No";
			defaults["Sounds"] = "Yes";
			defaults["Ribbons"] = "No";
			#endregion
			// copy it into the regular dictionary
			dict = new Dictionary<string, string>(defaults);


			string pkPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ponykart";
			// if a folder doesn't exist there, create it
			if (!Directory.Exists(pkPath))
				Directory.CreateDirectory(pkPath);

			string optionsPath = pkPath + "\\options.ini";

			// create it if the file doesn't exist, and write out some defaults
			if (!File.Exists(optionsPath)) {
				using (FileStream stream = File.Create(optionsPath)) {
					using (StreamWriter writer = new StreamWriter(stream)) {
						foreach (KeyValuePair<string, string> pair in defaults) {
							writer.WriteLine(pair.Key + "=" + pair.Value);
						}
					}
				}
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

				cfile.Dispose();
				sectionIterator.Dispose();
			}
		}

		/// <summary>
		/// Writes out the current settings to our options file
		/// </summary>
		public static void Save() {
			string optionsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ponykart\\options.ini";

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
