using System;
using Mogre.PhysX;
using Ponykart.Phys;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// a little test for the dialogue system
	/// </summary>
	public class DialogueTest : IDisposable {
		public DialogueTest() {
			Launch.Log("[Loading] Creating DialogueTest");
			Launch.Log(LKernel.Get<TriggerReporter>().AddEvent("test trigger area", Test) + "");
		}

		void Test(TriggerRegion region, Shape oshape, TriggerFlags tf) {
			var d = LKernel.Get<DialogueManager>();

			if (tf.IsEnterFlag())
				d.CreateDialogue("media/gui/lyra.jpg", oshape.Actor.Name, "I have entered " + region.Name);
			else if (tf.IsLeaveFlag())
				d.DestroyDialogue();
		}

		public void Dispose() {
			Launch.Log(LKernel.Get<TriggerReporter>().RemoveEvent("test trigger area", Test) + "");
		}
	}
}
