using Mogre;
using Ponykart.Core;
using PonykartParsers;

namespace Ponykart.Actors {
	public class DashJavelin : Kart {
		private AnimationState jetMax;
		private AnimationState jetMin;
		private float topSpeedKmHour;

		public DashJavelin(ThingBlock block, ThingDefinition def) : base(block, def) {
			ModelComponent chassis = ModelComponents[0];
			// first get rid of the existing animation blender it creates automatically
			LKernel.GetG<AnimationManager>().Remove(chassis.AnimationBlender);
			chassis.AnimationBlender = null;


			Entity chassisEnt = chassis.Entity;
			// get our two animation states
			jetMax = chassisEnt.GetAnimationState("JetMax");
			jetMax.Enabled = true;
			jetMax.Weight = 0f;
			jetMin = chassisEnt.GetAnimationState("JetMin");
			jetMin.Enabled = true;
			jetMin.Weight = 1f;

			// we want the two animations to blend together, not add to each other
			chassisEnt.Skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_AVERAGE;

			// convert from linear velocity to KPH
			topSpeedKmHour = DefaultMaxSpeed * 3.6f;

			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		/// <summary>
		/// Change the width of the jet engine based on our current speed
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			// crop it to be between 0 and 1
			float whatIfPoneWasAnts = _vehicle.CurrentSpeedKmHour / topSpeedKmHour;
			if (whatIfPoneWasAnts > 1) {
				jetMax.Weight = 1f;
				jetMin.Weight = 0f;
			}
			else if (whatIfPoneWasAnts < 0) {
				jetMax.Weight = 0f;
				jetMin.Weight = 1f;
			}
			else {
				jetMax.Weight = whatIfPoneWasAnts;
				jetMin.Weight = 1f - whatIfPoneWasAnts;
			}

			return true;
		}

		/// <summary>
		/// Unhook from the event
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;

			base.Dispose(disposing);
		}
	}
}
