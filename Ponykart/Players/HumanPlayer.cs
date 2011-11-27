using Ponykart.Actors;
using Ponykart.Core;
using PonykartParsers;

namespace Ponykart.Players {
	public class HumanPlayer : Player {
		KeyBindingManager bindings;


		public HumanPlayer(MuffinDefinition def, int id)
			: base(def, id) {

			// hook up to input events
			bindings = LKernel.Get<KeyBindingManager>();

			bindings.PressEventsDict[LKey.Accelerate] += OnPressAccelerate;
			bindings.ReleaseEventsDict[LKey.Accelerate] += OnReleaseAccelerate;
			bindings.PressEventsDict[LKey.Drift] += OnPressDrift;
			bindings.ReleaseEventsDict[LKey.Drift] += OnReleaseDrift;
			bindings.PressEventsDict[LKey.Reverse] += OnPressReverse;
			bindings.ReleaseEventsDict[LKey.Reverse] += OnReleaseReverse;
			bindings.PressEventsDict[LKey.TurnLeft] += OnPressTurnLeft;
			bindings.ReleaseEventsDict[LKey.TurnLeft] += OnReleaseTurnLeft;
			bindings.PressEventsDict[LKey.TurnRight] += OnPressTurnRight;
			bindings.ReleaseEventsDict[LKey.TurnRight] += OnReleaseTurnRight;
		}

		public override bool IsControlEnabled {
			get {
				return base.IsControlEnabled;
			}
			// we need to do this so we can immediately start moving when we regain control without having to repress keys
			set {
				base.IsControlEnabled = value;

				if (value) {
					// gain control
					if (bindings.IsKeyPressed(LKey.Accelerate))
						OnPressAccelerate(LKey.Accelerate);
					if (bindings.IsKeyPressed(LKey.Drift))
						OnPressDrift(LKey.Drift);
					if (bindings.IsKeyPressed(LKey.Reverse))
						OnPressReverse(LKey.Reverse);
					if (bindings.IsKeyPressed(LKey.TurnLeft))
						OnPressTurnLeft(LKey.TurnLeft);
					if (bindings.IsKeyPressed(LKey.TurnRight))
						OnPressTurnRight(LKey.TurnRight);
				}
				else {
					// lose control
					if (bindings.IsKeyPressed(LKey.Accelerate))
						OnReleaseAccelerate(LKey.Accelerate);
					if (bindings.IsKeyPressed(LKey.Drift))
						OnReleaseDrift(LKey.Drift);
					if (bindings.IsKeyPressed(LKey.Reverse))
						OnReleaseReverse(LKey.Reverse);
					if (bindings.IsKeyPressed(LKey.TurnLeft))
						OnReleaseTurnLeft(LKey.TurnLeft);
					if (bindings.IsKeyPressed(LKey.TurnRight))
						OnReleaseTurnRight(LKey.TurnRight);
				}
			}
		}


		#region key events
		protected void OnPressAccelerate(LKey k) {
			if (IsControlEnabled) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Reverse))
					Kart.Acceleration = 0;
				// otherwise go forwards as normal
				else
					Kart.Acceleration = 1;
			}
		}
		protected void OnReleaseAccelerate(LKey k) {
			if (IsControlEnabled) {
				// if reverse is still held down, then we start reversing
				if (bindings.IsKeyPressed(LKey.Reverse))
					Kart.Acceleration = -1;
				// otherwise we just stop accelerating
				else
					Kart.Acceleration = 0;
			}
		}


		protected void OnPressDrift(LKey k) {
			if (IsControlEnabled) {
				// if left is pressed and right isn't, start drifting left
				if (bindings.IsKeyPressed(LKey.TurnLeft) && !bindings.IsKeyPressed(LKey.TurnRight)) {
					Kart.StartDrifting(KartDriftState.StartRight);
				}
				// otherwise if right is pressed and left isn't, start drifting right
				else if (bindings.IsKeyPressed(LKey.TurnRight) && !bindings.IsKeyPressed(LKey.TurnLeft)) {
					Kart.StartDrifting(KartDriftState.StartLeft);
				}
				// otherwise it wants to drift but we don't have a direction yet
				else if (Kart.VehicleSpeed > 100) {
					Kart.DriftState = KartDriftState.WantsDriftingButNotTurning;
				}
			}
		}
		/// <summary>
		/// cancel the drift
		/// </summary>
		protected void OnReleaseDrift(LKey k) {
			if (IsControlEnabled) {
				// if we were drifting left
				if (Kart.DriftState == KartDriftState.FullLeft || Kart.DriftState == KartDriftState.StartLeft) {
					Kart.StopDrifting();
				}
				// if we were drifting right
				else if (Kart.DriftState == KartDriftState.FullRight || Kart.DriftState == KartDriftState.StartRight) {
					Kart.StopDrifting();
				}
				// if we had the drift button down but weren't actually drifting
				else if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning) {
					Kart.DriftState = KartDriftState.None;
				}
			}
		}


		protected void OnPressReverse(LKey k) {
			if (IsControlEnabled) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Accelerate))
					Kart.Acceleration = 0;
				// otherwise go forwards as normal
				else
					Kart.Acceleration = -1;
			}
		}
		protected void OnReleaseReverse(LKey k) {
			if (IsControlEnabled) {
				// if forward is still held down, then we start going forwards
				if (bindings.IsKeyPressed(LKey.Accelerate))
					Kart.Acceleration = 1;
				// otherwise we just stop accelerating
				else
					Kart.Acceleration = 0;
			}
		}


		protected void OnPressTurnLeft(LKey k) {
			if (IsControlEnabled) {
				// if we're waiting to drift
				if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning) {
					Kart.StartDrifting(KartDriftState.StartRight);
				}
				// normal steering
				else {
					// if both turns are pressed, we go straight
					if (bindings.IsKeyPressed(LKey.TurnRight))
						Kart.TurnMultiplier = 0;
					// otherwise go left
					else
						Kart.TurnMultiplier = 1;
				}
			}
		}
		protected void OnReleaseTurnLeft(LKey k) {
			if (IsControlEnabled) {
				// if right is still pressed, turn right
				if (bindings.IsKeyPressed(LKey.TurnRight))
					Kart.TurnMultiplier = -1;
				// otherwise go straight
				else
					Kart.TurnMultiplier = 0;
			}
		}


		protected void OnPressTurnRight(LKey k) {
			if (IsControlEnabled) {
				if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning) {
					Kart.StartDrifting(KartDriftState.StartLeft);
				}
				// normal steering
				else {
					// if both turns are pressed, we go straight
					if (bindings.IsKeyPressed(LKey.TurnLeft))
						Kart.TurnMultiplier = 0;
					// otherwise go right
					else
						Kart.TurnMultiplier = -1;
				}
			}
		}
		protected void OnReleaseTurnRight(LKey k) {
			if (IsControlEnabled) {
				// if left is still pressed, turn left
				if (bindings.IsKeyPressed(LKey.TurnLeft))
					Kart.TurnMultiplier = 1;
				// otherwise go straight
				else
					Kart.TurnMultiplier = 0;
			}
		}
		#endregion


		protected override void UseItem() {
			throw new System.NotImplementedException();
		}


		public override void Detach() {
			bindings.PressEventsDict[LKey.Accelerate] -= OnPressAccelerate;
			bindings.ReleaseEventsDict[LKey.Accelerate] -= OnReleaseAccelerate;
			bindings.PressEventsDict[LKey.Drift] -= OnPressDrift;
			bindings.ReleaseEventsDict[LKey.Drift] -= OnReleaseDrift;
			bindings.PressEventsDict[LKey.Reverse] -= OnPressReverse;
			bindings.ReleaseEventsDict[LKey.Reverse] -= OnReleaseReverse;
			bindings.PressEventsDict[LKey.TurnLeft] -= OnPressTurnLeft;
			bindings.ReleaseEventsDict[LKey.TurnLeft] -= OnReleaseTurnLeft;
			bindings.PressEventsDict[LKey.TurnRight] -= OnPressTurnRight;
			bindings.ReleaseEventsDict[LKey.TurnRight] -= OnReleaseTurnRight;

			base.Detach();
		}
	}
}
