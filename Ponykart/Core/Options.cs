using System;
using System.Collections.Generic;
using System.IO;
using Mogre;

namespace Ponykart.Core {
	public static class Options {
		private static IDictionary<string, string> dict;

		/// <summary>
		/// Creates the folder and file if they don't exist, and either prints some data to it (if it doesn't exist) or reads from it (if it does)
		/// </summary>
		public static void Initialise() {
			dict = new Dictionary<string, string>();

			string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			string pkPath = appdataPath + "\\Ponykart";

			// if a folder doesn't exist there, create it
			if (!Directory.Exists(pkPath))
				Directory.CreateDirectory(pkPath);

			string optionsPath = pkPath + "\\options.ini";

			// create it if the file doesn't exist, and write out some defaults
			if (!File.Exists(optionsPath)) {
				using (FileStream stream = File.Create(optionsPath)) {
					using (StreamWriter writer = new StreamWriter(stream)) {
						writer.Write(
@"FSAA=0
Floating-point mode=Fastest
Full Screen=No
VSync=Yes
VSync Interval=1
Video Mode=1280 x 800 @ 32-bit colour
sRGB Gamma Conversion=No
Music=No
Sounds=Yes
Ribbons=No");
					}
				}

				dict["FSAA"] = "0";
				dict["Floating-point mode"] = "Fastest";
				dict["Full Screen"] = "No";
				dict["VSync"] = "Yes";
				dict["VSync Interval"] = "1";
				dict["Video Mode"] = "1280 x 800 @ 32-bit colour";
				dict["sRGB Gamma Conversion"] = "No";
				dict["Music"] = "No";
				dict["Sounds"] = "Yes";
				dict["Ribbons"] = "No";
			}
			// otherwise we just read from it
			else {
				ConfigFile cfile = new ConfigFile();
				cfile.Load(optionsPath, "=", true);

				ConfigFile.SectionIterator sectionIterator = cfile.GetSectionIterator();
				sectionIterator.MoveNext();
				foreach (KeyValuePair<string, string> pair in sectionIterator.Current) {
					dict.Add(pair.Key, pair.Value);
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
