using System;
using System.Threading;
using Mogre;
using Ponykart.Core;
using PonykartParsers;
using Timer = System.Threading.Timer;

namespace Ponykart.Actors {
	public class BackgroundPony : LThing {
		// don't care about eyes, hair, horn, or folded wings since they aren't animated
		private ModelComponent bodyComponent, maneComponent, tailComponent, wingsComponent;
		public Pose AnimPose { get; private set; }
		public Type PonyType { get; private set; }
		private bool cheering = false;
		private AnimationState blinkState;
		private Timer blinkTimer, animTimer;
		private const float _blendTime = 1f;
		// milliseconds
		private const int _blinkTimeSpanMin = 1500, _blinkTimeSpanMax = 5000,
						  _animTimeSpanMin = 5000, _animTimeSpanMax = 8000;
		private Random random;

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
				else if (mc.Name.EndsWith("WingsFolded")) {
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

			// set up some timers to handle animation changing
			random = new Random(IDs.Random);
			blinkTimer = new Timer(new TimerCallback(BlinkTimer), null, random.Next(_blinkTimeSpanMin, _blinkTimeSpanMax), Timeout.Infinite);
			animTimer = new Timer(new TimerCallback(AnimTimer), null, random.Next(_animTimeSpanMin, _animTimeSpanMax), Timeout.Infinite);

			AddTimeToBodyManeAndTail();
		}

		/// <summary>
		/// helper method to add animation blending to the three main animated parts of the bg ponies
		/// </summary>
		private void AnimateBodyManeAndTail(string animationName, AnimationBlendingTransition transition, float duration, bool looping) {
			bodyComponent.AnimationBlender.Blend(animationName, transition, duration, looping);
			maneComponent.AnimationBlender.Blend(animationName, transition, duration, looping);
			tailComponent.AnimationBlender.Blend(animationName, transition, duration, looping);
		}

		/// <summary>
		/// helper method to add some initial time to all of the three main bg ponies so they aren't all in sync
		/// </summary>
		public void AddTimeToBodyManeAndTail() {
			float rand = (float) random.NextDouble();
			bodyComponent.AnimationBlender.AddTime(rand);
			maneComponent.AnimationBlender.AddTime(rand);
			tailComponent.AnimationBlender.AddTime(rand);
		}

		/// <summary>
		/// Play an animation instantly
		/// </summary>
		public override void ChangeAnimation(string animationName) {
			AnimateBodyManeAndTail(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
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
			string anim = "Stand" + random.Next(1, 4);
			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, _blendTime, true);
			if (wingsComponent != null)
				wingsComponent.AnimationBlender.Blend("WingsRest", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

			AnimPose = Pose.Standing;
		}

		/// <summary>
		/// Play a random sitting animation and change our pose to Sitting
		/// </summary>
		public void Sit() {
			string anim = "Sit" + random.Next(1, 4);
			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, _blendTime, true);
			if (wingsComponent != null)
				wingsComponent.AnimationBlender.Blend("WingsRest", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

			AnimPose = Pose.Sitting;
		}

		/// <summary>
		/// Play a random flying animation (if this is a pegasus) and change our pose to Flying
		/// </summary>
		public void Fly() {
			if (wingsComponent != null) {
				string anim = "Fly" + random.Next(1, 5);

				AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, _blendTime, true);

				anim = "Flap" + random.Next(1, 4);
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
					int rand = random.Next(0, 2);
					anim = "Cheer" + rand;
					if (rand == 0)
						anim = "Clap";
					break;
				case Pose.Sitting:
					rand = random.Next(1, 1);
					anim = "SitCheer" + rand;
					break;
				case Pose.Flying:
					// TODO
					break;
			}

			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, _blendTime, true);

			cheering = true;
		}

		/// <summary>
		/// Stops playing a cheering animation and plays a different appropriate one instead
		/// </summary>
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

		/// <summary>
		/// Plays a different, similar animation.
		/// </summary>
		public void PlayNext() {
			if (cheering)
				Cheer();
			else if (AnimPose == Pose.Standing)
				Stand();
			else if (AnimPose == Pose.Sitting)
				Sit();
			else if (AnimPose == Pose.Flying)
				Fly();
		}

		/// <summary>
		/// method for the blink timer to run
		/// </summary>
		void BlinkTimer(object o) {
			if (Pauser.IsPaused) {
				// keep trying again until we're unpaused
				blinkTimer.Change(500, 500);
			}
			else {
				Blink();
				blinkTimer.Change(random.Next(_blinkTimeSpanMin, _blinkTimeSpanMax), Timeout.Infinite);
			}
		}

		/// <summary>
		/// method for the animation timer to run
		/// </summary>
		void AnimTimer(object o) {
			if (Pauser.IsPaused) {
				// keep trying again until we're unpaused
				blinkTimer.Change(500, 500);
			}
			else {
				PlayNext();
				animTimer.Change(random.Next(_animTimeSpanMin, _animTimeSpanMax), Timeout.Infinite);
			}
		}

		/// <summary>
		/// Clean up
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				LKernel.GetG<AnimationManager>().Remove(blinkState);
			}

			blinkTimer.Dispose();
			animTimer.Dispose();

			base.Dispose(disposing);
		}

		//////////////////////////////////////////////

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
