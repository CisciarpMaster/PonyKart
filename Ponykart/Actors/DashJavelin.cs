using IrrKlang;
using Mogre;
using Ponykart.Core;
using PonykartParsers;

namespace Ponykart.Actors {
	public class DashJavelin : Kart {
		private AnimationState jetMax;
		private AnimationState jetMin;
		private readonly float topSpeedKmHour;
		private float jetOpening = 0f;

		private RibbonTrail jetRibbon;

		private ISound idleSound, fullSound, revDownSound, revUpSound;
		private DashKartSoundState state;

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

			jetRibbon = RibbonComponents[0].Ribbon;

			idleSound = SoundComponents[0].Sound;
			fullSound = SoundComponents[1].Sound;
			revDownSound = SoundComponents[2].Sound;
			revUpSound = SoundComponents[3].Sound;

			// convert from linear velocity to KPH
			topSpeedKmHour = DefaultMaxSpeed * 3.6f;

			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		const float JET_FLAP_OPEN_AMOUNT = 0.05f;
		/// <summary>
		/// Change the width of the jet engine based on our current speed
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			// crop it to be between 0 and 1

			if (_acceleration <= 0f) {
				float relSpeed = (_vehicle.CurrentSpeedKmHour / topSpeedKmHour) * 0.5f;

				if (relSpeed > 0.5f) {
					relSpeed = 0.5f;
				}
				else if (relSpeed < 0f) {
					relSpeed = 0f;
				}

				if (relSpeed < jetOpening && jetOpening > 0f) {
					if (jetOpening - relSpeed < JET_FLAP_OPEN_AMOUNT)
						jetOpening = relSpeed;
					else
						jetOpening -= JET_FLAP_OPEN_AMOUNT;
				}
				else if (relSpeed > jetOpening && jetOpening < 1f) {
					if (relSpeed - jetOpening < JET_FLAP_OPEN_AMOUNT)
						jetOpening = relSpeed;
					else
						jetOpening += JET_FLAP_OPEN_AMOUNT;
				}

				jetRibbon.SetInitialWidth(0u, jetOpening * 0.2f);
				jetRibbon.SetColourChange(0u, 0f, 0f, 0f, 20f);
			}
			else {
				if (jetOpening < 1f)
					jetOpening += JET_FLAP_OPEN_AMOUNT;

				jetRibbon.SetInitialWidth(0u, jetOpening * 0.2f);
				jetRibbon.SetColourChange(0u, 0f, 0f, 0f, 3f);
			}

			jetMax.Weight = jetOpening;
			jetMin.Weight = 1f - jetOpening;

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

		enum DashKartSoundState {
			PostRevDown,
			RevUpRunning,
			PostRevUp,
			RevDownRunning
		}
	}
}
