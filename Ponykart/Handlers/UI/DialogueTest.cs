using BulletSharp;
using Ponykart.Physics;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// a little test for the dialogue system
	/// </summary>
	public class DialogueTest : System.IDisposable {
		public DialogueTest() {
			Launch.Log("[Loading] Creating DialogueTest");
			LKernel.Get<TriggerReporter>().AddEvent("test trigger area", Test);
		}

		void Test(TriggerRegion region, RigidBody body, bool isEntering) {
			var d = LKernel.Get<DialogueManager>();

			if (isEntering)
				d.CreateDialogue("media/gui/lyra.jpg", body.GetName(), "I have entered " + region.Name);
			else 
				d.DestroyDialogue();
		}

		public void Dispose() {
			LKernel.Get<TriggerReporter>().RemoveEvent("test trigger area", Test);
		}
	}
}
