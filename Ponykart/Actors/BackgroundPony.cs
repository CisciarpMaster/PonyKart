using Mogre;
using PonykartParsers;

namespace Ponykart.Actors {
	public class BackgroundPony : LThing {
		// don't care about eyes, hair, horn, or folded wings since they aren't animated
		private ModelComponent bodyComponent, maneComponent, tailComponent, wingsComponent;
		private Pose pose = Pose.Standing;
		private bool cheering = false;
		private AnimationState blinkState;

		public BackgroundPony(ThingBlock block, ThingDefinition def) : base(block, def) {
			foreach (ModelComponent mc in ModelComponents) {
				if (mc.Name.EndsWith("Body"))
					bodyComponent = mc;
				else if (mc.Name.EndsWith("Mane"))
					maneComponent = mc;
				else if (mc.Name.EndsWith("Tail"))
					tailComponent = mc;
				else if (mc.Name.EndsWith("Wings"))
					wingsComponent = mc;
			}

			blinkState = bodyComponent.Entity.GetAnimationState("Blink");
			blinkState.Enabled = false;
			blinkState.Loop = false;
			blinkState.Weight = 1;
			blinkState.TimePosition = blinkState.Length;
			// is this automatically updated from the AnimationManager?
		}

		/// <summary>
		/// Play an animation instantly
		/// </summary>
		public override void ChangeAnimation(string animationName) {
			bodyComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
			bodyComponent.AnimationBlender.AddTime((int) ID);
			maneComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
			maneComponent.AnimationBlender.AddTime((int) ID);
			tailComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
			tailComponent.AnimationBlender.AddTime((int) ID);
		}

		/// <summary>
		/// Make the wings (if we have some) change their animation instantly
		/// </summary>
		public void ChangeWingsAnimation(string animationName) {
			if (wingsComponent != null) {
				wingsComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
				wingsComponent.AnimationBlender.AddTime((int) ID);
			}
		}

		/// <summary>
		/// Play a random standing animation and change our pose to Standing
		/// </summary>
		public void Stand() {
			int rand = (IDs.Random % 3) + 1;
			string anim = "Stand" + rand;

			bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
			maneComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
			tailComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
			if (wingsComponent != null)
				wingsComponent.AnimationBlender.Blend("WingsRest", AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);

			pose = Pose.Standing;
		}

		/// <summary>
		/// Play a random sitting animation and change our pose to Sitting
		/// </summary>
		public void Sit() {
			int rand = (IDs.Random % 3) + 1;
			string anim = "Sit" + rand;

			bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
			maneComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
			tailComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
			if (wingsComponent != null)
				wingsComponent.AnimationBlender.Blend("WingsRest", AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);

			pose = Pose.Sitting;
		}

		/// <summary>
		/// Blink once
		/// </summary>
		public void Blink() {
			//blinkState.Enabled = true;
			blinkState.TimePosition = 0;
		}

		/// <summary>
		/// Play a random cheering animation according to our pose
		/// </summary>
		public void Cheer() {
			string anim = "";

			switch (pose) {
				case Pose.Standing:
					int rand = IDs.Random % 3;
					anim = "Cheer" + rand;
					if (rand == 0)
						anim = "Clap";
					break;
				case Pose.Sitting:
					anim = "SitCheer1";
					break;
				case Pose.Flying:
					break;
			}

			bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
			maneComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
			tailComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, 0.3f, true);
		}

		public enum Pose {
			Sitting,
			Standing,
			Flying
		}
	}
}
