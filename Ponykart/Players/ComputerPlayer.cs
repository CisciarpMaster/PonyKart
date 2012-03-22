using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Physics;

namespace Ponykart.Players {
	public class ComputerPlayer : Player {
		private Vector3 nextWaypoint;
		/// <summary> The trigger region we are driving towards </summary>
		public TriggerRegion CurrentRegion { get; private set; }
		/// <summary> The trigger region we came from </summary>
		public TriggerRegion PreviousRegion { get; private set; }
		private LThing axis;

		public ComputerPlayer(LevelChangedEventArgs eventArgs, int id) : base(eventArgs, id, true) {
			axis = LKernel.GetG<Core.Spawner>().Spawn("Axis", Kart.RootNode.Position);
			axis.ModelComponents[0].Node.SetScale(0.1f, 0.1f, 0.1f);

			Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
		}

		void EveryTenth(object o) {
			Vector3 vecToTar = nextWaypoint - Kart.RootNode.Position;
			// not using Y so set it to 0
			vecToTar.y = 0;
					
			float steerFactor = SteerTowards(vecToTar);

			Kart.TurnMultiplier = steerFactor;
			Kart.Acceleration = 1.0f - (System.Math.Abs(steerFactor) / 3f);
		}

		/// <summary>
		/// Calculates the multiplier for the kart to steer towards the next waypoint
		/// </summary>
		/// <param name="vecToTar">the next waypoint position - the current kart position</param>
		/// <returns>A float between -1 and 1, inclusive</returns>
		private float SteerTowards(Vector3 vecToTar) {
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

		/// <summary>
		/// Calculates the next waypoint we should drive to
		/// </summary>
		/// <param name="enteredRegion">The trigger region that fired the collision event</param>
		/// <param name="nextRegion">The next trigger region we should drive to</param>
		/// <param name="info">
		/// Contains some information about when we drove into the previous trigger region. 
		/// If this is null, then it's the initial waypoint we got when we loaded the level.
		/// </param>
		public void CalculateNewWaypoint(TriggerRegion enteredRegion, TriggerRegion nextRegion, CollisionReportInfo info) {
			// have to check for this because trigger regions like sending duplicate events
			// also we check against the previous region in situations where the kart is in two regions at once
			if ((nextRegion != CurrentRegion && nextRegion != PreviousRegion && enteredRegion != PreviousRegion) || info == null) {

				// do we need to do all of the transform stuff? it's accurate, but do we need to be particularly accurate?
				// forward is -X
				float offset;
				if (info == null || info.Position == null) {
					// when we're starting, we'll just use our kart's position for things
					Vector3 relativePos = nextRegion.Body.WorldTransform.InverseAffine() * Kart.RootNode.Position;
					offset = MakeOffset(relativePos, nextRegion);
				}
				else {
					// otherwise calculate using the region we just entered
					Vector3 relativePos = enteredRegion.Body.WorldTransform.InverseAffine() * info.Position.Value;
					offset = MakeOffset(relativePos, enteredRegion);
				}
				nextWaypoint = nextRegion.Body.WorldTransform * new Vector3(0, 0, offset * nextRegion.Width);
				nextWaypoint.y = Kart.RootNode.Position.y;

				// update the region pointers
				PreviousRegion = enteredRegion;
				CurrentRegion = nextRegion;

				// update this axis' position
				axis.RootNode.Position = nextWaypoint;
				axis.RootNode.Orientation = nextRegion.Body.Orientation;
				nextRegion.CycleToNextColor();
			}
		}

		private float MakeOffset(Vector3 relativePos, TriggerRegion region) {
			
			float offset = relativePos.z / (region.Width * 2);
			//System.Console.WriteLine(relativePos.z + " | " + region.Width + " | " + offset);

			/*if (offset < -0.75f)
				return -0.75f;
			else if (offset > 0.75f)
				return 0.75f;
			else*/
				return offset;
		}

		public override void Detach() {
			Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;
			base.Detach();
		}

		protected override void UseItem() {
			throw new System.NotImplementedException();
		}
	}
}
