using Mogre;
using Mogre.PhysX;
using System;

namespace Ponykart.Phys {
	/// <summary>
	/// Standard controller hit reporter. Push around 
	/// </summary>
	public class StandardControllerHitReport : IUserControllerHitReport {

		/// <summary>
		/// Push around things with the CollidablePushableID, don't push anything else
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Do we automatically apply forces to the touched shape?</returns>
		public ControllerActions OnShapeHit(ControllerShapeHit value) {
			Console.WriteLine("Controller " + value.Controller.Actor.Name + " collided with shape " + value.Shape.Actor.Name + " with ID " + value.Shape.Group);
			if (value != null) {
				uint id = value.Shape.Group;
				// check the group
				if (id == Groups.CollidablePushableID) {
					Actor actor = value.Shape.Actor;
					// of course we can only push dynamic things
					if (actor.IsDynamic && !actor.BodyFlags.Kinematic) {
						// We only allow horizontal pushes. Vertical pushes when we stand on dynamic objects creates
						// useless stress on the solver. It would be possible to enable/disable vertical pushes on
						// particular objects, if the gameplay requires it.
						if (value.Direction.y == 0) {
							float coeff = actor.Mass * value.Length * 10;
							actor.AddForceAtLocalPos(value.Direction * coeff, Vector3.ZERO, ForceModes.Impulse);
						}
					}
				}
			}
			return ControllerActions.None;
		}

		/// <summary>
		/// Don't push other controllers
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Do we automatically apply forces to the touched controller?</returns>
		public ControllerActions OnControllerHit(ControllersHit value) {
			Console.WriteLine("Controller " + value.Controller.Actor.Name + " collided with controller " + value.Other.Actor.Name);
			return ControllerActions.None;
		}
	}
}
