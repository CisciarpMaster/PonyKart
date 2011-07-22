using Mogre.PhysX;
using Ponykart.Phys;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// a little test for the dialogue system
	/// </summary>
	public class DialogueTest {
		public DialogueTest() {
			Launch.Log("[Loading] Creating DialogueTest");
			LKernel.Get<TriggerReporter>().AddEvent("test trigger area", Test);
		}

		void Test(Shape tshape, Shape oshape, TriggerFlags tf) {
			var d = LKernel.Get<DialogueManager>();

			if (Phys.TriggerReporter.IsEnterFlag(tf))
				d.CreateDialogue("media/gui/lyra.jpg", oshape.Actor.Name, "I have entered " + tshape.Actor.Name);
			else
				d.DestroyDialogue();
		}
	}
}
