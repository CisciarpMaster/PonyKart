using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Phys;

namespace Ponykart.Handlers {
	/// <summary>
	/// just a little test for the trigger regions
	/// </summary>
	public class TriggerRegionsTest : IDisposable {
		TriggerRegion tr;

		public TriggerRegionsTest() {
			Launch.Log("[Loading] Creating TriggerRegionsTest");
			tr = new TriggerRegion("test trigger area", new Vector3(5, 0, 5), new SphereShapeDesc(1));

			new TriggerRegion("test trigger area 2", new Vector3(-5, 0, 5), new Vector3(45, 45, 45), new BoxShapeDesc(new Vector3(1, 1, 1)));

			// attach the handler
			tr.OnTrigger += doSomething;
		}

		void doSomething(TriggerRegion region, Shape otherShape, TriggerFlags flags) {
			if (flags.IsEnterFlag()) {
				Console.WriteLine(otherShape.Actor.Name + " has entered trigger area \"" + region.Name + "\"");
				region.SetBalloonGlowColor(BalloonGlowColor.cyan);
			}
			else if (flags.IsLeaveFlag()) {
				Console.WriteLine(otherShape.Actor.Name + " has left trigger area \"" + region.Name + "\"");
				region.SetBalloonGlowColor(BalloonGlowColor.orange);
			}
		}

		public void Dispose() {
			tr.OnTrigger -= doSomething;
		}
	}
}
