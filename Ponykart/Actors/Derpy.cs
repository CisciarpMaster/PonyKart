using Mogre;
using Ponykart.Core;
using PonykartParsers;

namespace Ponykart.Actors {
	public class Derpy : LThing {
		private AnimationState blinkState;
		private Kart followKart;

		private SceneNode interpNode;
		private Euler facing;

		private ModelComponent bodyComponent;
		private ModelComponent flagComponent;
		private ModelComponent startLightComponent;

		private DerpyAnimation anim;

		public Derpy(ThingBlock block, ThingDefinition def) : base(block, def) {
			bodyComponent = ModelComponents[0];
			flagComponent = ModelComponents[1];
			startLightComponent = ModelComponents[2];

			facing = new Euler(0, 0, 0);

			bodyComponent.Entity.Skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_CUMULATIVE;

			blinkState = bodyComponent.Entity.GetAnimationState("Blink2");
			blinkState.Enabled = true;
			blinkState.Loop = true;
			blinkState.Weight = 1f;
			blinkState.AddTime(ID);

			Skeleton skeleton = bodyComponent.Entity.Skeleton;
			blinkState.CreateBlendMask(skeleton.NumBones, 0f);
			ushort handle = skeleton.GetBone("EyeBrowTop.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowTop.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);

			LKernel.GetG<AnimationManager>().Add(blinkState);

			interpNode = LKernel.GetG<SceneManager>().RootSceneNode.CreateChildSceneNode("DerpyInterpNode" + ID, Vector3.ZERO);

			anim = DerpyAnimation.Hover1;
		}

		void EveryTenth(object state) {
			if (followKart == null)
				return;

			float speed = followKart.VehicleSpeed;
			if ((speed < 30f && speed > -15f) && anim == DerpyAnimation.Forward1) {
				bodyComponent.AnimationBlender.Blend("Hover1", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);
				anim = DerpyAnimation.Hover1;
			}
			else if ((speed > 30f || speed < -15f) && anim == DerpyAnimation.Hover1) {
				bodyComponent.AnimationBlender.Blend("Forward1", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);
				anim = DerpyAnimation.Forward1;
			}
		}

		//private readonly Radian PITCH_LIMIT = new Degree(25f);

		bool FrameStarted(FrameEvent evt) {
			if (Pauser.IsPaused)
				return true;

			Vector3 derivedInterp = interpNode._getDerivedPosition();
			Vector3 derivedDerpy = this.RootNode._getDerivedPosition();

			Vector3 displacement = derivedInterp - derivedDerpy;
			this.RootNode.Translate(displacement * 6 * evt.timeSinceLastFrame);


			Vector3 lookat = followKart.ActualPosition - this.RootNode.Position;
			Euler temp = facing.GetRotationTo(-lookat, true, false, true);
			Radian tempTime = new Radian(evt.timeSinceLastFrame * 3f);
			temp.LimitYaw(tempTime);

			facing = facing + temp;
			this.RootNode.Orientation = facing;

			return true;
		}

		/// <summary>
		/// Specifies a kart to follow around
		/// </summary>
		public void AttachToKart(Vector3 offset, Kart kart) {
			this.followKart = kart;

			// attach this to the kart's node
			interpNode.Parent.RemoveChild(interpNode);
			kart.RootNode.AddChild(interpNode);

			//this.RootNode.InheritOrientation = false;
			//this.RootNode.SetAutoTracking(true, kart.RootNode, Vector3.UNIT_Z, new Vector3(0, 0, 3f));
			//this.RootNode.SetFixedYawAxis(true, Vector3.UNIT_Y);
			this.RootNode.Position = Vector3.ZERO;

			interpNode.Position = offset;

			LKernel.GetG<Root>().FrameStarted += FrameStarted;
			Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
		}

		/// <summary>
		/// Detach from the kart
		/// </summary>
		public void DetachFromKart() {
			if (followKart == null)
				return;

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;
			Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;

			this.RootNode.Position = Vector3.ZERO;

			// reattach it to the scene's root scene node so she still gets rendered
			followKart.RootNode.RemoveChild(interpNode);
			interpNode.Creator.RootSceneNode.AddChild(interpNode);

			//this.RootNode.InheritOrientation = true;
			//this.RootNode.SetAutoTracking(false);
			//this.RootNode.SetFixedYawAxis(false);

			followKart = null;
		}



		public override void ChangeAnimation(string animationName) {
			if (bodyComponent.Entity.AllAnimationStates.HasAnimationState(animationName)) {
				bodyComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);

				if (animationName == "FlagWave1" || animationName == "FlagWave2") {
					startLightComponent.Entity.Visible = false;
					flagComponent.Entity.Visible = true;

					flagComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
					startLightComponent.AnimationBlender.Blend("Basis", AnimationBlendingTransition.BlendSwitch, 0, true);
				}
				else if (animationName == "HoldStartLight1") {
					flagComponent.Entity.Visible = false;
					startLightComponent.Entity.Visible = true;

					startLightComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
					flagComponent.AnimationBlender.Blend("Basis", AnimationBlendingTransition.BlendSwitch, 0, true);
				}
				else {
					flagComponent.Entity.Visible = false;
					startLightComponent.Entity.Visible = false;

					startLightComponent.AnimationBlender.Blend("Basis", AnimationBlendingTransition.BlendSwitch, 0, true);
					flagComponent.AnimationBlender.Blend("Basis", AnimationBlendingTransition.BlendSwitch, 0, true);
				}
			}
		}

		/// <summary>
		/// Blink once
		/// </summary>
		public virtual void Blink() {
			blinkState.Enabled = true;
			blinkState.TimePosition = 0;
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;
			Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;

			if (disposing) {
				LKernel.GetG<AnimationManager>().Remove(blinkState);
				//LKernel.GetG<SceneManager>().DestroySceneNode(interpNode);
			}
			//interpNode.Dispose();

			base.Dispose(disposing);
		}

		/*
			Body animaitons:
			Stand, Hover1, Forward1, Lift1, Blink, Blink2 (2 blinks spread out), Flap1, Flap2 (Same as Flap 1 but 12 fames shorter), WingsRest

			Body/Flag animations:
			FlagWave1, FlagWave2

			Body/StartLight animations:
			HoldStartLight1
		 */
		enum DerpyAnimation {
			Hover1,
			Forward1,
			Lift1,
			FlagWave1,
			FlagWave2,
			HoldStartLight1,
		}
	}
}
