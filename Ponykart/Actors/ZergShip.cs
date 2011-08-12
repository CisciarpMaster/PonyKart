using Mogre;

namespace Ponykart.Actors {
	/// <summary>
	/// Just a class to test animated models
	/// </summary>
	public class ZergShip : Thing {

		protected override string DefaultModel {
			get { return "zerg ship.mesh"; }
		}
		protected override string DefaultMaterial {
			get { return "zerg"; }
		}

		AnimationState animState;

		public ZergShip(ThingTemplate tt) : base(tt) {
			// hook into the "on every frame" event
			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		protected override void CreateMoreMogreStuff() {
			// rotate this to the right way around
			Node.Rotate(Vector3.UNIT_X, new Degree(90));
			// scale it down so it isn't so HUEG
			Node.SetScale(0.05f, 0.05f, 0.05f);
			// then make it play a looping stand animation
			animState = Entity.GetAnimationState("Stand");
			animState.Loop = true;
			animState.Enabled = true;

			base.CreateMoreMogreStuff();
		}

		protected override void SetUpPhysics() { }

		// this runs every frame
		bool FrameStarted(FrameEvent evt) {
			//if (LKernel.Get<Levels.LevelManager>().IsValidLevel && Entity != null)
				// advance our animation by the time since the last frame
				animState.AddTime(evt.timeSinceLastFrame);
			/*else {
				// this is not a valid level so destroy this
				Dispose();
				return true;
			}*/
			return true;
		}

		// cleanup
		public override void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
			base.Dispose();
		}
	}
}
