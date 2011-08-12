using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BulletSharp;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.Physics {
	/// <summary>
	/// The main thing of this you use is AddEvent. Stick in the name of the trigger region and the method you want
	/// to run when something enters/leaves it, and you're good to go. The method you give it can check stuff like which actors
	/// were involved and whether it was an entry or leave event.
	/// (You can use the extension methods IsLeaveFlag and IsEnterFlag on TriggerFlags to help with this)
	/// 
	/// If you're using a handler class thingy, don't forget to add RemoveEvent in its Dispose method.
	/// </summary>
	public class TriggerReporter {
		public IDictionary<string, TriggerRegion> Regions { get; private set; }

		// to make something a trigger area, use the shape desc's ShapeFlags -> TriggerEnable
		// trigger areas should have no body

		public TriggerReporter() {
			Launch.Log("[Loading] First Get<TriggerReporter>");
			Regions = new Dictionary<string, TriggerRegion>();

			LKernel.Get<LevelManager>().OnLevelUnload += OnLevelUnload;
			LKernel.Get<PhysicsMain>().PostSimulate += PostSimulate;
		}

		/// <summary>
		/// go through each region and find bodies that have entered or left it
		/// </summary>
		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			foreach (TriggerRegion region in Regions.Values) {
				// get our set of bodies that were inside the region from the previous frame
				Collection<RigidBody> previousBodies = region.CurrentlyCollidingWith;
				Collection<RigidBody> newBodies = new Collection<RigidBody>();

				// get all of the objects that are inside the region and add them to the new collection
				for (int a = 0; a < region.Ghost.NumOverlappingObjects; a++) {
					RigidBody body = region.Ghost.GetOverlappingObject(a) as RigidBody;
					// check to make sure this is actually a RigidBody - if it isn't, ignore it
					if (body != null) {
						newBodies.Add(body);
					}
				}

				// get the bodies that have been removed and added (yay linq)
				IEnumerable<RigidBody> added = newBodies.Except(previousBodies);
				IEnumerable<RigidBody> removed = previousBodies.Except(newBodies);

				// update the region's list of bodies that are inside it
				region.CurrentlyCollidingWith = newBodies;

				// then run our triggers
				foreach (RigidBody addedBody in added)
					region.InvokeTrigger(addedBody, TriggerReportFlags.Enter);
				foreach (RigidBody removedBody in removed)
					region.InvokeTrigger(removedBody, TriggerReportFlags.Leave);
			}
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
