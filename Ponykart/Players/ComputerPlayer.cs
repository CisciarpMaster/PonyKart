using System.Collections.Generic;
using System.IO;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;

namespace Ponykart.Players {
	public class ComputerPlayer : Player {
		const float DecelThreshold = 12100f; // 110 ^ 2
		const float WaypointThreshold = 10000f; // 100 ^ 2
		private int loop = 0;
		private Player Human;
		private int currWaypoint = 0;
		private List<Vector3> Waypoints = new List<Vector3>();

		private StreamWriter outfile = new StreamWriter("waypoints.txt");

		public ComputerPlayer(LevelChangedEventArgs eventArgs, int id) : base(eventArgs, id) {
			Human = LKernel.GetG<PlayerManager>().MainPlayer;
			
			//Waypoints.Add(new Vector3(200, 0, 200));
			//Waypoints.Add(new Vector3(200, 0, -200));
			//Waypoints.Add(new Vector3(-200, 0, -200));
			//Waypoints.Add(new Vector3(-200, 0, 200));

			//Dear David. THIS NEXT LINE IS BAD AND YOU SHOULD FEEL BAD. Sincerely, David.
			StreamReader infile = new StreamReader("media/saa_r1 (2).waypoint");

			string line;
			string[] tempStr;
			while ((line = infile.ReadLine()) != null) {
				tempStr = line.Split(' ');
				Waypoints.Add(new Vector3(float.Parse(tempStr[0]), float.Parse(tempStr[1]), float.Parse(tempStr[2])));
			}
			infile.Close();

			LKernel.GetG<Root>().FrameEnded += FrameEnded;
		}

		float elapsed;
		bool FrameEnded(FrameEvent evt) {
			if (!Pauser.IsPaused) {

				if (elapsed > 0.1f) {
					// use LKernel.GetG<LevelManager>().CurrentLevel.Definition.Get__Property() to retrieve your waypoints
					/*loop++;
					if (loop % 10 == 0) {
						string tmp;
						tmp = Human.NodePosition.x + " " + Human.NodePosition.y + " " + Human.NodePosition.z;
						outfile.WriteLine(tmp);
					}*/
					Vector3 target = Waypoints[currWaypoint];
					Vector3 vecToTar = target - Kart.RootNode.Position;
					// it's better to use the squared length because this way we avoid having to so a square root operation, which are pretty expensive
					float distToTar = vecToTar.SquaredLength;
					// I changed this so we only have to do the subtraction once
					Kart.TurnMultiplier = SteerTowards(target, vecToTar);

					if (distToTar > DecelThreshold)
						Kart.Acceleration = 0.5f;
					else
						Kart.Acceleration = 0.1f;

					if (distToTar < WaypointThreshold) {
						currWaypoint++;
						currWaypoint = currWaypoint % Waypoints.Count;
					}

					elapsed -= 0.1f;
				}

				elapsed += evt.timeSinceLastFrame;
			}

			return true;
		}

		private float SteerTowards(Vector3 target, Vector3 vecToTar) {
			Vector3 xaxis = Kart.Body.Orientation.XAxis;

			xaxis.Normalise();
			vecToTar.Normalise();

			float result = xaxis.DotProduct(vecToTar);

			if (result < -1)
				return -1;
			else if (result > 1)
				return 1;
			else
				return result;
		}

		public override void Detach() {
			LKernel.GetG<Root>().FrameEnded -= FrameEnded;
			outfile.Flush();
			outfile.Close();
			base.Detach();
		}

		protected override void UseItem() {
			throw new System.NotImplementedException();
		}
	}
}
