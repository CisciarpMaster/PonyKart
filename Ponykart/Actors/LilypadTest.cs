#if DEBUG
using Mogre;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Just a class to test animated models
	/// </summary>
	public class LilypadTest : LThing {
		AnimationState animState;

		public LilypadTest(ThingBlock block, ThingDefinition def) : base(block, def) {
			// hook into the "on every frame" event
			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		protected override void PostInitialiseComponents(ThingBlock template, ThingDefinition def) {
			// then make it play a looping stand animation
			animState = ModelComponents[0].Entity.GetAnimationState("float");
			animState.Loop = true;
			animState.Enabled = true;
		}

		// this runs every frame
		bool FrameStarted(FrameEvent evt) {
			if (!Core.Pauser.IsPaused)
				animState.AddTime(evt.timeSinceLastFrame);
			return true;
		}
	}
}
#endif