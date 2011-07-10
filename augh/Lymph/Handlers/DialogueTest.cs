using Lymph.Phys;
using Lymph.UI;
using Mogre.PhysX;

namespace Lymph.Handlers {
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
				d.CreateDialogue("media/gui/epic.png", oshape.Actor.Name, "I have entered " + tshape.Actor.Name);
			else
				d.DestroyDialogue();
		}
	}
}
