using Ponykart.Core;
using PonykartParsers;

namespace Ponykart.Players {
	public class HumanPlayer : Player {
		KeyBindingManager bindings;

		public HumanPlayer(MuffinDefinition def, int id) : base(def, id) {

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

		#region key events
		protected void OnPressAccelerate(LKey k) {
			// do nothing if the brake is pressed
			//if (!bindings.IsKeyPressed(LKey.Drift)) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Reverse))
					Kart.Accelerate(0);
					// otherwise go forwards as normal
				else
					Kart.Accelerate(1);
			//}
		}
		protected void OnReleaseAccelerate(LKey k) {
			// if reverse is still held down, then we start reversing
			if (bindings.IsKeyPressed(LKey.Reverse))
				Kart.Accelerate(-1);
			// otherwise we just stop accelerating
			else
				Kart.Accelerate(0);
		}


		protected void OnPressDrift(LKey k) {

		}
		protected void OnReleaseDrift(LKey k) {

		}


		protected void OnPressReverse(LKey k) {
			// do nothing if the brake is pressed
			//if (!bindings.IsKeyPressed(LKey.Drift)) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Accelerate))
					Kart.Accelerate(0);
				// otherwise go forwards as normal
				else
					Kart.Accelerate(-1);
				
			//}
		}
		protected void OnReleaseReverse(LKey k) {
			// if forward is still held down, then we start going forwards
			if (bindings.IsKeyPressed(LKey.Accelerate))
				Kart.Accelerate(1);
			// otherwise we just stop accelerating
			else
				Kart.Accelerate(0);
		}


		protected void OnPressTurnLeft(LKey k) {
			// if both turns are pressed, we go straight
			if (bindings.IsKeyPressed(LKey.TurnRight))
				Kart.Turn(0);
			// otherwise go left
			else
				Kart.Turn(1);
		}
		protected void OnReleaseTurnLeft(LKey k) {
			// if right is still pressed, turn right
			if (bindings.IsKeyPressed(LKey.TurnRight))
				Kart.Turn(-1);
			// otherwise go straight
			else
				Kart.Turn(0);
		}


		protected void OnPressTurnRight(LKey k) {
			// if both turns are pressed, we go straight
			if (bindings.IsKeyPressed(LKey.TurnLeft))
				Kart.Turn(0);
			// otherwise go right
			else
				Kart.Turn(-1);
		}
		protected void OnReleaseTurnRight(LKey k) {
			// if left is still pressed, turn left
			if (bindings.IsKeyPressed(LKey.TurnLeft))
				Kart.Turn(1);
			// otherwise go straight
			else
				Kart.Turn(0);
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
