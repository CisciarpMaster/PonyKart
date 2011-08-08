using System;
using BulletSharp;
using Mogre;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	/// <summary>
	/// just a little test for the trigger regions
	/// </summary>
	public class TriggerRegionsTest : System.IDisposable {
		TriggerRegion tr;

		public TriggerRegionsTest() {
			Launch.Log("[Loading] Creating TriggerRegionsTest");
			tr = new TriggerRegion("test trigger area", new Vector3(5, 0, 5), new SphereShape(1));

#if DEBUG
			//new TriggerRegion("test trigger area 2", new Vector3(-5, 0, 5), new Vector3(45, 45, 45), new BoxShapeDesc(new Vector3(2, 1, 1)));

			//new TriggerRegion("test trigger area 3", new Vector3(-10, 0, 5), new Vector3(45, 0, 0), new CapsuleShapeDesc(1, 1));
#endif

			// attach the handler
			tr.OnTrigger += doSomething;
		}

		void doSomething(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags) {
			if (flags.IsEnterFlag()) {
				Console.WriteLine(otherBody.GetName() + " has entered trigger area \"" + region.Name + "\"");
				region.SetBalloonGlowColor(BalloonGlowColor.cyan);
			}
			else {
				Console.WriteLine(otherBody.GetName() + " has left trigger area \"" + region.Name + "\"");
				region.SetBalloonGlowColor(BalloonGlowColor.orange);
			}
		}

		public void Dispose() {
			tr.OnTrigger -= doSomething;
		}
	}
}
