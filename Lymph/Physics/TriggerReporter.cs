using System.Collections.Generic;
using Mogre.PhysX;
using Ponykart.Levels;

namespace Ponykart.Phys {
	/// <summary>
	/// The main thing of this you use is AddEvent. Stick in the name of the trigger region and the method you want
	/// to run when something enters/leaves it, and you're good to go. The method you give it can check stuff like which actors
	/// were involved and whether it was an entry or leave event.
	/// (You can use the extension methods IsLeaveFlag and IsEnterFlag on TriggerFlags to help with this)
	/// 
	/// If you're using a handler class thingy, don't forget to add RemoveEvent in its Dispose method.
	/// </summary>
	public class TriggerReporter : IUserTriggerReport {
		public IDictionary<string, TriggerRegion> Regions { get; private set; }

		// to make something a trigger area, use the shape desc's ShapeFlags -> TriggerEnable
		// trigger areas should have no body

		public TriggerReporter() {
			Launch.Log("[Loading] First Get<TriggerReporter>");
			Regions = new Dictionary<string, TriggerRegion>();

			LKernel.Get<LevelManager>().OnLevelUnload += OnLevelUnload;
		}

		/// <summary>
		/// Clean up all of the regions when we unload a level
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs args) {
			foreach (TriggerRegion tr in Regions.Values) {
				tr.Dispose();
			}
			Regions.Clear();
		}

		/// <summary>
		/// This is the method that physX runs for us
		/// </summary>
		public void OnTrigger(Shape triggerShape, Shape otherShape, TriggerFlags flags) {
			TriggerRegion tr = Regions[triggerShape.Name];
			tr.InvokeTrigger(otherShape, flags);
		}

		/// <summary>
		/// Hooks an event to a trigger region safely. If that region does not exist, you simply get a warning message instead of a crash.
		/// </summary>
		/// <returns>If the region with that name exists, this returns that region. If it doesn't, this returns null.</returns>
		public TriggerRegion AddEvent(string regionName, TriggerReportHandler handler) {
			TriggerRegion tr;

			if (Regions.TryGetValue(regionName, out tr)) {
				tr.OnTrigger += handler;
				return tr;
			}
			Launch.Log("** [WARNING]: A trigger region with that name does not exist! (" + regionName + ")");
			return null;
		}

		/// <summary>
		/// Removes an event from a trigger region safely. If that region does not exist, you simply get a warning message instead of a crash.
		/// </summary>
		/// <returns>True if that region exists, false otherwise.</returns>
		public bool RemoveEvent(string regionName, TriggerReportHandler handler) {
			TriggerRegion tr;

			if (Regions.TryGetValue(regionName, out tr)) {
				tr.OnTrigger -= handler;
				return true;
			}
			Launch.Log("** [WARNING]: A trigger region with that name does not exist! (" + regionName + ")");
			return false;
		}
	}
}
