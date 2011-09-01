using System;
using System.Collections.Generic;
using Mogre;
using Ponykart.Properties;

namespace Ponykart.Stuff {
	/// <summary>
	/// Class to get starting positions for the karts. Mostly placeholder code for now - later we should get positions from a file or something.
	/// </summary>
	public class KartSpawnPositions : IDisposable {
		private IDictionary<int, Vector3> dict;

		public KartSpawnPositions() {
			dict = new Dictionary<int, Vector3>();
			dict[0] = new Vector3(0, 0.5f, 0);
			dict[1] = new Vector3(0, 0.5f, -4);
			dict[2] = new Vector3(0, 0.5f, -8);
			dict[3] = new Vector3(-4, 0.5f, 0);
			dict[4] = new Vector3(-4, 0.5f, -4);
			dict[5] = new Vector3(-4, 0.5f, -8);
			dict[6] = new Vector3(-8, 0.5f, 0);
			dict[7] = new Vector3(-8, 0.5f, -4);
		}

		/// <summary>
		/// Gets the spawn position of the specified kart.
		/// </summary>
		/// <param name="ID">
		/// Must be between 0..n-1 inclusive where n is the maximum number of players as specified in Constants.NUMBER_OF_PLAYERS
		/// </param>
		public Vector3 GetPosition(int ID) {
			if (ID < 0 || ID >= Settings.Default.NumberOfPlayers)
				throw new ArgumentOutOfRangeException("ID", "ID number specified for kart spawn position is not valid!");
			return dict[ID];
		}

		public void Dispose() {
			dict.Clear();
		}
	}
}
