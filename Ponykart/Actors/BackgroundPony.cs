using Mogre;
using Ponykart.Core;
using PonykartParsers;

namespace Ponykart.Actors {
	public class BackgroundPony : LThing {
		// don't care about eyes, hair, horn, or folded wings since they aren't animated
		private ModelComponent bodyComponent, maneComponent, tailComponent, wingsComponent;
		public Pose AnimPose { get; private set; }
		public Type PonyType { get; private set; }
		private bool cheering = false;
		private AnimationState blinkState;
		private const float _blendTime = 1f;

		public BackgroundPony(ThingBlock block, ThingDefinition def) : base(block, def) {
			AnimPose = Pose.Standing;
			PonyType = Type.Earth;

			foreach (ModelComponent mc in ModelComponents) {
				if (mc.Name.EndsWith("Body"))
					bodyComponent = mc;
				else if (mc.Name.EndsWith("Mane"))
					maneComponent = mc;
				else if (mc.Name.EndsWith("Tail"))
					tailComponent = mc;
				else if (mc.Name.EndsWith("Wings")) {
					wingsComponent = mc;
					PonyType = Type.Pegasus;
				}
				else if (mc.Name.EndsWith("Horn")) {
					PonyType = Type.Unicorn;
				}
			}

			// make sure our animations add their weights and don't just average out. The AnimationBlender already handles averaging between two anims.
			bodyComponent.Entity.Skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_CUMULATIVE;

			// set up the animation state with some stuff
			blinkState = bodyComponent.Entity.GetAnimationState("Blink");
			blinkState.Enabled = false;
			blinkState.Loop = false;
			blinkState.Weight = 1;
			blinkState.TimePosition = blinkState.Length;

			// set up a blend mask so only the head and eyebrow bones have any effect
			Skeleton skeleton = bodyComponent.Entity.Skeleton;
			blinkState.CreateBlendMask(skeleton.NumBones, 0);
			ushort handle = skeleton.GetBone("EyeBrowTop.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1);
			handle = skeleton.GetBone("EyeBrowBottom.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1);
			handle = skeleton.GetBone("EyeBrowTop.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1);
			handle = skeleton.GetBone("EyeBrowBottom.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1);
			//handle = skeleton.GetBone("Head").Handle;
			//blinkState.SetBlendMaskEntry(handle, 1);

			LKernel.GetG<AnimationManager>().Add(blinkState);
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
		public void ChangeWingAnimation(string animationName) {
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

			bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);
			maneComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);
			tailComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);
			if (wingsComponent != null)
				wingsComponent.AnimationBlender.Blend("WingsRest", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

			AnimPose = Pose.Standing;
		}

		/// <summary>
		/// Play a random sitting animation and change our pose to Sitting
		/// </summary>
		public void Sit() {
			int rand = (IDs.Random % 3) + 1;
			string anim = "Sit" + rand;

			bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);
			maneComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);
			tailComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);
			if (wingsComponent != null)
				wingsComponent.AnimationBlender.Blend("WingsRest", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

			AnimPose = Pose.Sitting;
		}

		/// <summary>
		/// Play a random flying animation (if this is a pegasus) and change our pose to Flying
		/// </summary>
		public void Fly() {
			if (wingsComponent != null) {
				int rand = (IDs.Random % 4) + 1;
				string anim = "Fly" + rand;

				bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);
				maneComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);
				tailComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, _blendTime, true);

				rand = (IDs.Random % 3) + 1;
				anim = "Flap" + rand;
				wingsComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

				AnimPose = Pose.Flying;
			}
		}

		/// <summary>
		/// Blink once
		/// </summary>
		public void Blink() {
			blinkState.Enabled = true;
			blinkState.TimePosition = 0;
		}

		/// <summary>
		/// Play a random cheering animation according to our pose
		/// </summary>
		public void Cheer() {
			string anim = "";

			switch (AnimPose) {
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
					// TODO
					break;
			}

			bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, _blendTime, true);
			maneComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, _blendTime, true);
			tailComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, _blendTime, true);

			cheering = true;
		}

		public void StopCheering() {
			switch (AnimPose) {
				case Pose.Standing:
					Stand();
					break;
				case Pose.Sitting:
					Sit();
					break;
				case Pose.Flying:
					Fly();
					break;
			}

			cheering = false;
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				LKernel.GetG<AnimationManager>().Remove(blinkState);
			}

			base.Dispose(disposing);
		}

		public enum Pose {
			Sitting,
			Standing,
			Flying
		}

		public enum Type {
			Earth,
			Pegasus,
			Unicorn
		}
	}
}
