using Mogre;
using Ponykart.Core;
using Ponykart.Players;
using PonykartParsers;

namespace Ponykart.Actors {
	public class Derpy : LThing {

		private AnimationState blinkState;
		private const float BLEND_TIME = 1f;
		private Kart followKart;

		private ModelComponent bodyComponent;
		private ModelComponent flagComponent;
		private ModelComponent startLightComponent;

		public Derpy(ThingBlock block, ThingDefinition def) : base(block, def) {
			bodyComponent = ModelComponents[0];
			flagComponent = ModelComponents[1];
			startLightComponent = ModelComponents[2];

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

			followKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
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

			if (disposing) {
				LKernel.GetG<AnimationManager>().Remove(blinkState);
			}

			base.Dispose(disposing);
		}
	}
}
