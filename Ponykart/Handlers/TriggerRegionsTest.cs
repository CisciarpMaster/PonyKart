using System;
using BulletSharp;
using Mogre;
using Ponykart.Physics;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// just a little test for the trigger regions
	/// </summary>
	[Handler(HandlerScope.Level)]
	public class TriggerRegionsTest : ILevelHandler {
		TriggerRegion tr;

		public TriggerRegionsTest() {
			tr = new TriggerRegion("test trigger area", new Vector3(5, 0, 5), new BoxShape(1, 1, 1));

#if DEBUG
			//new TriggerRegion("test trigger area 2", new Vector3(-5, 0, 5), new Vector3(45, 45, 45), new BoxShapeDesc(new Vector3(2, 1, 1)));

			//new TriggerRegion("test trigger area 3", new Vector3(-10, 0, 5), new Vector3(45, 0, 0), new CapsuleShapeDesc(1, 1));
#endif

			// attach the handler
			tr.OnTrigger += doSomething;
		}

		void doSomething(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags) {
			var d = LKernel.Get<DialogueManager>();

			if (flags.IsEnterFlag()) {
				Console.WriteLine(otherBody.GetName() + " has entered trigger area \"" + region.Name + "\"");
				region.SetBalloonGlowColor(BalloonGlowColor.cyan);

				d.CreateDialogue("media/gui/lyra.jpg", otherBody.GetName(), "I have entered " + region.Name);
			}
			else {
				Console.WriteLine(otherBody.GetName() + " has left trigger area \"" + region.Name + "\"");
				region.SetBalloonGlowColor(BalloonGlowColor.orange);

				d.DestroyDialogue();
			}
		}

		public void Dispose() {
			tr.OnTrigger -= doSomething;
		}
	}
}
