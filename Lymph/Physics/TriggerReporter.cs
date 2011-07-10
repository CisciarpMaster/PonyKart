using System;
using System.Collections.Generic;
using System.Linq;
using Lymph.Actors;
using Lymph.Levels;
using Mogre.PhysX;

namespace Lymph.Phys {

	public delegate void TriggerReportHandler(Shape triggerShape, Shape otherShape, TriggerFlags flags);

	/// <summary>
	/// The main thing of this you use is AddEvent. Stick in the name of the trigger region actor and the method you want
	/// to run when something enters/leaves it, and you're good to go. The method you give it can check stuff like which actors
	/// were involved and whether it was an entry or leave event.
	/// (You can use the static methods IsLeaveFlag and IsEnterFlag to help with this)
	/// 
	/// If you're using a handler class thingy, don't forget to add RemoveEvent in its Dispose method.
	/// </summary>
	public class TriggerReporter : IUserTriggerReport {
		private IDictionary<string, TriggerReportHandler> perShapeReports;

		private event TriggerReportHandler OnTriggerEnter;
		private event TriggerReportHandler OnTriggerLeave;

		// to make something a trigger area, use the shape desc's ShapeFlags -> TriggerEnable
		// trigger areas should have no body

		public TriggerReporter() {
			perShapeReports = new Dictionary<string, TriggerReportHandler>();

			LKernel.Get<LevelManager>().OnLevelUnload += (ea) => perShapeReports.Clear();
		}

		/// <summary>
		/// This is the method that physX runs for us
		/// </summary>
		public void OnTrigger(Shape triggerShape, Shape otherShape, TriggerFlags flags) {
			// if it's not important, don't bother firing events
			if (!IsShapeImportant(otherShape))
				return;

			if (IsEnterFlag(flags)) {
				// something entered the trigger area
				if (OnTriggerEnter != null)
					OnTriggerEnter(triggerShape, otherShape, flags);

				FireEvent(triggerShape, otherShape, flags);
			}
			if (IsLeaveFlag(flags)) {
				// something left the trigger area
				if (OnTriggerLeave != null)
					OnTriggerLeave(triggerShape, otherShape, flags);

				FireEvent(triggerShape, otherShape, flags);
			}
		}

		// can extend this to do more stuff later. Maybe have a flag or something?
		// basically instead of firing events willy nilly, check to see if the thing that entered here is even worth worrying about
		// for example we don't give a crap about projectiles
		private bool IsShapeImportant(Shape shape) {
			// the player is important
			if (shape.Actor == LKernel.Get<Player>().Actor)
				return true;

			// cells are important
			Kart kart = shape.Actor.UserData as Kart;
			if (kart != null)
				return true;
			// the shape is not important
			return false;
		}

		/// <summary>
		/// Add an event to the trigger reporter.
		/// </summary>
		/// <param name="triggerShape">The actor name of your trigger region</param>
		/// <param name="handler">Your method to execute whenever something important enters the trigger area</param>
		public void AddEvent(string triggerShape, TriggerReportHandler handler) {
			// is the trigger shape already in the dictionary?
			if (perShapeReports.ContainsKey(triggerShape))
				perShapeReports[triggerShape] += handler;

			else {
				perShapeReports.Add(triggerShape, null);
				perShapeReports[triggerShape] += handler;
			}
		}

		/// <summary>
		/// Remove an event from the trigger reporter
		/// </summary>
		/// <param name="triggerShape">The actor name of your trigger region</param>
		/// <param name="handler">Your method you want to remove</param>
		public void RemoveEvent(string triggerShape, TriggerReportHandler handler) {
			if (perShapeReports.ContainsKey(triggerShape)) {
				perShapeReports[triggerShape] -= handler;
				if (perShapeReports[triggerShape] == null)
					perShapeReports.Remove(triggerShape);
			} else
				throw new KeyNotFoundException("Tried to remove handle for shape \"" + triggerShape + "\" but this shape was never added!");
		}

		/// <summary>
		/// invoke an event.
		/// </summary>
		private void FireEvent(Shape triggerShape, Shape otherShape, TriggerFlags flags) {
			string name = triggerShape.Actor.Name;
			if (perShapeReports.ContainsKey(name)) {
				try {
					perShapeReports[name].Invoke(triggerShape, otherShape, flags);
				} catch {
					// if you restart lua in the middle of a level and then enter a region that triggers an event, you'll probably get an exception here
					// but we really don't care about it so we'll just ignore it here and stop it from crashing our program
				}
			}
		}

		/// <summary>
		/// Searches the PhysX scene for an actor with the given name. We don't really name shapes so we search for actors instead.
		/// </summary>
		/// <param name="nameOfShape">The name of the shape you want to search for</param>
		/// <returns></returns>
		public Shape GetShapeFromName(string nameOfShape) {

			// wheee linq
			Actor act = LKernel.Get<PhysXMain>().Scene.Actors.First<Actor>(x => x.Name == nameOfShape);

			if (act != null) {
				Shape s = act.Shapes[0];
				return s;
			} else
				throw new ArgumentException("No shape exists with that name!", "nameOfShape");
		}

		/// <summary>
		/// Shortcut method to see if the given TriggerFlags contain TriggerOnEnter.
		/// </summary>
		/// <param name="flags">The flags you want to check</param>
		/// <returns>True if TriggerFlags.TriggerOnEnter = 1, false otherwise</returns>
		public static bool IsEnterFlag(TriggerFlags flags) {
			return ((flags & TriggerFlags.TriggerOnEnter) == TriggerFlags.TriggerOnEnter);
		}

		/// <summary>
		/// Shortcut method to see if the given TriggerFlags contain TriggerOnLeave.
		/// </summary>
		/// <param name="flags">The flags you want to check</param>
		/// <returns>True if TriggerFlags.TriggerOnLeave = 1, false otherwise</returns>
		public static bool IsLeaveFlag(TriggerFlags flags) {
			return ((flags & TriggerFlags.TriggerOnLeave) == TriggerFlags.TriggerOnLeave);
		}
	}
}
