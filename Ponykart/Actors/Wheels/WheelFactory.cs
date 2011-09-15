using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Mogre;
using Ponykart.Properties;

namespace Ponykart.Actors {
	/// <summary>
	/// A class to read .wheel files and stick them into a wheel object
	/// </summary>
	public class WheelFactory {
		// our list of wheels
		private IDictionary<string, IDictionary<string, float>> wheels;

		// this helps us parse things because of how some countries use . as a decimal point while others use ,
		private CultureInfo culture = CultureInfo.InvariantCulture;

		/// <summary>
		/// Constructor.
		/// </summary>
		public WheelFactory() {
			wheels = new Dictionary<string, IDictionary<string, float>>();

			ReadWheelsFromFiles();
		}

		/// <summary>
		/// Go through our media/wheels/ directory and find all of the wheel definitions we have, then make dictionaries out of them
		/// and add them to our one big dictionary.		
		/// </summary>
		public void ReadWheelsFromFiles() {
			// since we can run this whenever (like when we're tweaking files), we want to clear our dictionary first
			wheels.Clear();

			// get all of the filenames of the files in media/wheels/
			IEnumerable<string> files = Directory.EnumerateFiles(Settings.Default.WheelFileLocation, "*" + Settings.Default.WheelFileExtension);

			foreach (string filename in files) {
				// our wheelname is the filename minus the file extension
				string wheelname = filename.Remove(filename.IndexOf(Settings.Default.WheelFileExtension));
				// this gets rid of the "media/wheels/" bit
				wheelname = wheelname.Replace(Settings.Default.WheelFileLocation, "");

				string wheelcontents = "";

				// open up the file and read everything from it
				using (var stream = File.Open(filename, FileMode.Open)) {
					using (var reader = new StreamReader(stream)) {
						while (!reader.EndOfStream) {
							wheelcontents += reader.ReadLine() + "\n";
						}
						reader.Close();
					}
				}

				wheels[wheelname] = ParseWheel(wheelname, wheelcontents);
			}
		}

		/// <summary>
		/// Takes the contents of a .wheel file and puts it in a dictionary
		/// </summary>
		private IDictionary<string, float> ParseWheel(string wheelname, string wheelcontents) {
			IDictionary<string, float> wheeldict = new Dictionary<string, float>();

			// get rid of whitespace
			wheelcontents.Replace(" ", "");
			wheelcontents.Replace("\t", "");

			string[] splits = wheelcontents.Split('\n');

			foreach (string line in splits) {
				// ignore newlines and comments
				if (line.Length == 0 || line.StartsWith("//"))
					continue;

				string[] splitline = line.Split('=');
				wheeldict.Add(splitline[0].TrimEnd(), float.Parse(splitline[1], culture));
			}

			return wheeldict;
		}

		/// <summary>
		/// Creates a wheel.
		/// </summary>
		/// <param name="wheelName">
		/// The name of the wheel type you want to create. Should be the same as the filename, minus the extension. Case sensitive!
		/// </param>
		public Wheel CreateWheel(string wheelName, WheelID ID, Kart owner, Vector3 position, bool isFrontWheel) {
			IDictionary<string, float> dict = wheels[wheelName];
			Wheel wheel = new Wheel(owner, position, ID) {
				Radius = dict["Radius"],
				Width = dict["Width"],
				SuspensionRestLength = dict["SuspensionRestLength"],

				SpringStiffness = dict["SpringStiffness"],
				SpringCompression = dict["SpringCompression"],
				SpringDamping = dict["SpringDamping"],
				FrictionSlip = dict["FrictionSlip"],
				RollInfluence = dict["RollInfluence"],

				BrakeForce = dict["BrakeForce"],
				MotorForce = dict["MotorForce"],
				TurnAngle = Math.DegreesToRadians(dict["TurnAngle"]),
			};
			wheel.CreateWheel(position, isFrontWheel);
			return wheel;
		}
	}
}
