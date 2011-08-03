using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Mogre;

namespace Ponykart.Actors {
	public class WheelFactory {

		// lat = sideways grip, long = forwards grip
		//										1.0f				 0.02f						2.0f					   0.01f						1000000f
		public static float LatExtremumSlip = 1.0f, LatExtremumValue = 0.05f, LatAsymptoteSlip = 5.0f, LatAsymptoteValue = 0.002f, LatStiffnessFactor = 1000000f,
							LongExtremumSlip = 1.0f, LongExtremumValue = 0.05f, LongAsymptoteSlip = 2.0f, LongAsymptoteValue = 0.01f, LongStiffnessFactor = 1000000f;

		// our list of wheels
		IDictionary<string, IDictionary<string, float>> wheels;

		// this helps us parse things because of how some countries use . as a decimal point while others use ,
		CultureInfo culture = CultureInfo.InvariantCulture;

		/// <summary>
		/// Constructor.
		/// </summary>
		public WheelFactory() {
			wheels = new Dictionary<string, IDictionary<string, float>>();

			ReadWheelsFromFiles();
		}

		/// <summary>
		/// Go through our /media/wheels/ directory and find all of the wheel definitions we have, then make dictionaries out of them
		/// and add them to our one big dictionary.		
		/// </summary>
		public void ReadWheelsFromFiles() {
			// since we can run this whenever (like when we're tweaking files), we want to clear our dictionary first
			wheels.Clear();

			// get all of the filenames of the files in media/wheels/
			IEnumerable<string> files = Directory.EnumerateFiles(Settings.Default.WheelFileLocation);

			foreach (string filename in files) {
				// our wheelname is the filename minus the file extension
				string wheelname = filename.Remove(filename.IndexOf(".wheel"));
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
		public Wheel CreateWheel(string wheelName, Kart owner, Vector3 position) {
			IDictionary<string, float> dict = wheels[wheelName];
			Wheel wheel = new Wheel(owner, position) {
				Radius = dict["Radius"],
				Suspension = dict["Suspension"],
				SpringRestitution = dict["SpringRestitution"],
				SpringDamping = dict["SpringDamping"],
				SpringBias = dict["SpringBias"],

				BrakeForce = dict["BrakeForce"],
				MotorForce = dict["MotorForce"],
				TurnAngle = Math.DegreesToRadians(dict["TurnAngle"]),
				// the maximum axle speed a kart with these wheels reaches is only about 87ish anyway (40 linear vel)
				MaxSpeed = dict["MaxSpeed"],

				LatExtremumSlip = dict["LatExtremumSlip"],
				LatExtremumValue = dict["LatExtremumValue"],
				LatAsymptoteSlip = dict["LatAsymptoteSlip"],
				LatAsymptoteValue = dict["LatAsymptoteValue"],
				LatStiffnessFactor = dict["LatStiffnessFactor"],

				LongExtremumSlip = dict["LongExtremumSlip"],
				LongExtremumValue = dict["LongExtremumValue"],
				LongAsymptoteSlip = dict["LongAsymptoteSlip"],
				LongAsymptoteValue = dict["LongAsymptoteValue"],
				LongStiffnessFactor = dict["LongStiffnessFactor"],
			};
			wheel.CreateWheelShape(position);
			return wheel;
		}
	}
}
