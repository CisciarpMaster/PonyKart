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
		private Timer animTimer;
		private const float BLEND_TIME = 1f;
		// milliseconds
		private const int ANIMATION_TIMESPAN_MINIMUM = 5000, ANIMATION_TIMESPAN_MAXIMUM = 8000;
		private Random random;
		private Quaternion look_at = Quaternion.IDENTITY;
		private Bone headbone;
		private Kart followKart;

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

			Skeleton skeleton = bodyComponent.Entity.Skeleton;
			// set up all of the animation states to not use the head bone
			headbone = skeleton.GetBone("Head");
			headbone.SetManuallyControlled(true);
			foreach (var state in bodyComponent.Entity.AllAnimationStates.GetAnimationStateIterator()) {
				// don't add a blend mask to the blink state because we'll make a different one for it
				if (state.AnimationName == "Blink2")
					continue;

				state.CreateBlendMask(skeleton.NumBones);
				state.SetBlendMaskEntry(headbone.Handle, 0);
			}
			headbone.InheritOrientation = false;

			// set up the blink animation state with some stuff
			blinkState = bodyComponent.Entity.GetAnimationState("Blink2");
			blinkState.Enabled = true;
			blinkState.Loop = true;
			blinkState.Weight = 1;
			blinkState.AddTime(ID);

			// set up a blend mask so only the eyebrow bones have any effect on the blink animation
			blinkState.CreateBlendMask(skeleton.NumBones, 0);
			ushort handle = skeleton.GetBone("EyeBrowTop.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1);
			handle = skeleton.GetBone("EyeBrowBottom.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1);
			handle = skeleton.GetBone("EyeBrowTop.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1);
			handle = skeleton.GetBone("EyeBrowBottom.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1);

			// add the blink state to the animation manager so it has time added to it
			LKernel.GetG<AnimationManager>().Add(blinkState);

			// set up some timers to handle animation changing
			random = new Random(IDs.Random);
			animTimer = new Timer(new TimerCallback(AnimTimer), null, random.Next(ANIMATION_TIMESPAN_MINIMUM, ANIMATION_TIMESPAN_MAXIMUM), Timeout.Infinite);

			// add a bit of time to things so the animations aren't all synced at the beginning
			AddTimeToBodyManeAndTail();

			followKart = LKernel.GetG<Players.PlayerManager>().MainPlayer.Kart;
			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		
		bool FrameStarted(FrameEvent evt) {
			if (!Pauser.IsPaused) {
				Vector3 thisDerivedPos = RootNode._getDerivedPosition();

				Vector3 cam_pos = followKart.RootNode._getDerivedPosition( );
				Vector3 node_pos = thisDerivedPos + headbone._getDerivedPosition( );
				Vector3 diff = cam_pos - node_pos;
				diff.Normalise( );

				// Check we can turn here		
				Vector3 forward = thisDerivedPos * new Vector3( 0, 0, 1 );
				Quaternion desired = Quaternion.IDENTITY;
				if ( forward.DotProduct( diff ) > 0.1f )
				{
					desired = forward.GetRotationTo( diff );
				}
				look_at = Quaternion.Slerp( 0.95f, desired, look_at );
				headbone.Orientation = ( look_at * headbone.InitialOrientation );
			}

			return true;
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
				wingsComponent.AnimationBlender.AddTime(ID);
			}
		}

		/// <summary>
		/// Play a random standing animation and change our pose to Standing
		/// </summary>
		public void Stand() {
			string anim = "Stand" + random.Next(1, 4);
			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);
			if (wingsComponent != null)
				wingsComponent.AnimationBlender.Blend("WingsRest", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

			AnimPose = Pose.Standing;
		}

		/// <summary>
		/// Play a random sitting animation and change our pose to Sitting
		/// </summary>
		public void Sit() {
			string anim = "Sit" + random.Next(1, 4);
			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);
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

				AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);

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

			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);

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
		/// method for the animation timer to run
		/// </summary>
		void AnimTimer(object o) {
			if (Pauser.IsPaused) {
				// keep trying again until we're unpaused
				animTimer.Change(500, 500);
			}
			else {
				PlayNext();
				animTimer.Change(random.Next(ANIMATION_TIMESPAN_MINIMUM, ANIMATION_TIMESPAN_MAXIMUM), Timeout.Infinite);
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
			if (animTimer != null)
				animTimer.Dispose();

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;

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
