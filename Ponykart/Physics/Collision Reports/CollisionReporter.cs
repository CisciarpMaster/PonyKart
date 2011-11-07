using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.Physics {

	/// <summary>
	/// Our delegate for collision reports
	/// </summary>
	public delegate void CollisionReportEvent(CollisionReportInfo info);

	/// <summary>
	/// Our class for handling all collision reports, firing events when physics objects collide.
	/// </summary>
	public class CollisionReporter {
		/// <summary>
		/// our 2D array of contact report delegates
		/// </summary>
		private CollisionReportEvent[,] reporters;
		/// <summary>
		/// Our dictionary of collision objects, with a set containing the objects it collided with last frame.
		/// Why a hash set? They prevent having multiple identical objects, but also don't throw an error if they already contain it.
		/// They just silently ignore it (though .Add does return whether the adding was successful or not)
		/// </summary>
		private IDictionary<CollisionObject, HashSet<CollisionObject>> CurrentlyCollidingWith;


		#region -------------------- HEY THIS IS IMPORTANT ---------------------
		/// <summary>
		/// remember to update this!
		/// </summary>
		static readonly int HIGHEST_BIT_IN_COLLISION_GROUPS = 64;

		#endregion //-----------------------------------------------------------


		/// <summary>
		/// Constructor and stuff
		/// </summary>
		public CollisionReporter() {
			reporters = new CollisionReportEvent[HIGHEST_BIT_IN_COLLISION_GROUPS + 1, HIGHEST_BIT_IN_COLLISION_GROUPS + 1];
			CurrentlyCollidingWith = new Dictionary<CollisionObject, HashSet<CollisionObject>>();

			PhysicsMain.PostSimulate += PostSimulate;
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		Dictionary<CollisionObject, HashSet<CollisionObject>> newCollidingWith = new Dictionary<CollisionObject, HashSet<CollisionObject>>();

		/// <summary>
		/// Loop through all of the contacts and does the following:
		/// - Finds new collision pairs and fires events for them
		/// - Finds collision pairs that don't exist any more and fires events for them
		/// - Keeps an updated list of every contact pair currently in the world
		/// </summary>
		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {

			// we start with an empty dict and then gradually build it up. After the frame, we replace the old dict with this one,
			// and then find which pairs did not exist in the new one and fire stoppedtouching events for them
			newCollidingWith.Clear();

			// go through all of the contacts and find the ones we're interested in

			//var stopwatch = new System.Diagnostics.Stopwatch();
			//stopwatch.Start();

			for (int a = 0; a < world.Dispatcher.NumManifolds; a++) {
				PersistentManifold manifold = world.Dispatcher.GetManifoldByIndexInternal(a);

				// I'm not quite sure why it's giving manifolds for pairs that aren't even colliding, but oh well
				if (manifold.NumContacts <= 0)
					continue;

				// here are our two objects
				CollisionObject objectA = manifold.Body0 as CollisionObject;
				CollisionObject objectB = manifold.Body1 as CollisionObject;

				// if one of the two objects is deactivated, we don't care
				if (!objectA.IsActive && !objectB.IsActive)
					continue;

				// do we even care about collision groups for this pair?
				// Note: Both objects have to care in order for this to pass
				if (!objectA.CareAboutCollisionEvents() && !objectB.CareAboutCollisionEvents())
					continue;

				int objectACollisionGroup = (int) objectA.GetCollisionGroup();
				int objectBCollisionGroup = (int) objectB.GetCollisionGroup();

				// do we have any events that care about these groups? if not, then skip this collision pair
				if (reporters[objectACollisionGroup, objectBCollisionGroup] == null
						&& reporters[objectBCollisionGroup, objectACollisionGroup] == null)
					continue;


				// get the lists
				HashSet<CollisionObject> objectAList = GetCollisionListForObject(objectA, CurrentlyCollidingWith),
										 newObjectAList = GetCollisionListForObject(objectA, newCollidingWith),
										 objectBList = GetCollisionListForObject(objectB, CurrentlyCollidingWith),
										 newObjectBList = GetCollisionListForObject(objectB, newCollidingWith);

				// sanity check
				if (manifold.NumContacts > 0) {
					ManifoldPoint point = manifold.GetContactPoint(0);

					// when the actual bodies are touching and not just their AABB's
					if (point.Distance <= 0.05) {
						// see if the other object is in there
						if (!objectAList.Contains(objectB) || !objectBList.Contains(objectA)) {
							/*
							 * if it isn't, add it! this means we have a new collision and need to fire off an event!
							 * okay now we need to get the point where it contacted!
							 * Limitation with this system: if we're already colliding with B and then collide with it in a different place without
							 * leaving the original place, we won't get another event. Why? Well because what if something's sliding along?
							 * Don't need loads of events for that
							 */
							// make sure we add it to our collections! The hashmap means we don't have to worry about duplicates
							objectAList.Add(objectB);
							objectBList.Add(objectA);
							newObjectAList.Add(objectB);
							newObjectBList.Add(objectA);

							// update the dictionaries (is this necessary?)
							CurrentlyCollidingWith[objectA] = objectAList;
							CurrentlyCollidingWith[objectB] = objectBList;
							newCollidingWith[objectA] = newObjectAList;
							newCollidingWith[objectB] = newObjectBList;


							Vector3 pos = point.PositionWorldOnA.MidPoint(point.PositionWorldOnB);
							Vector3 normal = point.NormalWorldOnB;

							// woop woop they started touching, so we fire off an event!
							SetupAndFireEvent(objectA, objectB, pos, normal, ObjectTouchingFlags.StartedTouching);
						}
						else {
							// already in the dictionary, no new collisions. Add it to the new dictionary anyway though, because if we don't then it thinks
							// they stopped colliding. Which we don't want!
							newObjectAList.Add(objectB);
							newObjectBList.Add(objectA);

							newCollidingWith[objectA] = newObjectAList;
							newCollidingWith[objectB] = newObjectBList;
						}
					}
					// This means they're still inside each other's AABB's, but they aren't actually touching
					//else {
						
					//}
				}
			}
			//stopwatch.Stop();
			//System.Diagnostics.Debug.WriteLine("Physics pt 1: " + stopwatch.Elapsed);

			//stopwatch.Reset();
			CurrentlyCollidingWith.AsParallel().ForAll(pair => {
				// does the new dict have the old key?
				if (newCollidingWith.ContainsKey(pair.Key)) {
					// find all of the objects that were in the old list but weren't in the new one
					IEnumerable<CollisionObject> oldSet = pair.Value.Except(newCollidingWith[pair.Key]);

					foreach (CollisionObject obj in oldSet) {
						// fire events for each of them
						// this stops us from firing events twice
						if (pair.Key.GetCollisionGroup() < obj.GetCollisionGroup())
							SetupAndFireEvent(pair.Key, obj, null, null, ObjectTouchingFlags.StoppedTouching);
					}
				}
				// if it doesn't, that means two things stopped touching, and the new dict only had one object for that key.
				else if (pair.Value.Count > 0) {
					// this stops us from firing events twice
					CollisionObject toStopObjectA = pair.Key;
					CollisionObject toStopObjectB = pair.Value.First();

					if (toStopObjectA.GetCollisionGroup() < toStopObjectB.GetCollisionGroup())
						SetupAndFireEvent(toStopObjectA, toStopObjectB, null, null, ObjectTouchingFlags.StoppedTouching);
				}
			});
			//stopwatch.Stop();
			//System.Diagnostics.Debug.WriteLine("Physics pt 2: " + stopwatch.Elapsed);


			// then we replace the old dictionary with the new one
			CurrentlyCollidingWith = newCollidingWith;
		}

		/// <summary>
		/// Sets up a contact event and then invokes it.
		/// </summary>
		private void SetupAndFireEvent(CollisionObject objectA, CollisionObject objectB, Vector3? position, Vector3? normal, ObjectTouchingFlags flags) {
			PonykartCollisionGroups groupA = objectA.GetCollisionGroup();
			PonykartCollisionGroups groupB = objectB.GetCollisionGroup();
			int groupIDA = (int) groupA;
			int groupIDB = (int) groupB;

			CollisionReportInfo info = new CollisionReportInfo {
				FirstGroup = groupA,
				SecondGroup = groupB,
				FirstObject = objectA,
				SecondObject = objectB,
				Position = position,
				Normal = normal,
				Flags = flags
			};

			FireEvent(info);
		}

		/// <summary>
		/// Gets the collision set associated with this CollisionObject in the CurrentlyCollidingWith dictionary.
		/// If the object doesn't have a set in the dictionary, this method makes one and adds it to the dictionary.
		/// It does not add anything to the new set, but it initialises it.
		/// </summary>
		/// <param name="colObj">The CollisionObject to use as the "key" in the dictionary.</param>
		/// <param name="dict">The dictionary to check against</param>
		/// <returns>The collision set associated with the given CollisionObject.</returns>
		private HashSet<CollisionObject> GetCollisionListForObject(CollisionObject colObj, IDictionary<CollisionObject, HashSet<CollisionObject>> dict) {
			HashSet<CollisionObject> colSet;
			if (!dict.TryGetValue(colObj, out colSet)) {
				colSet = new HashSet<CollisionObject>();
				dict.Add(colObj, colSet);
			}
			return colSet;
		}

		/// <summary>
		/// Hooks up an event handler to a collision event.
		/// 
		/// The order of the two groups do not matter, as it will add them to both [a,b] and [b,a].
		/// 
		/// For example, to listen for the player colliding with the wall, you want to use Groups.PlayerID and Groups.WallID.
		/// </summary>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void AddEvent(PonykartCollisionGroups firstType, PonykartCollisionGroups secondType, CollisionReportEvent handler) {
			AddEvent((int) firstType, (int) secondType, handler);
		}

		/// <summary>
		/// Hooks up an event handler to a collision event.
		/// 
		/// The order of the two groups do not matter, as it will add them to both [a,b] and [b,a].
		/// 
		/// For example, to listen for the player colliding with the wall, you want to use Groups.PlayerID and Groups.WallID.
		/// </summary>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void AddEvent(int firstType, int secondType, CollisionReportEvent handler) {
			reporters[firstType, secondType] += handler;
			reporters[secondType, firstType] += handler;
		}

		/// <summary>
		/// Removes a handler from a collision event. 
		/// 
		/// The order of the two group IDs does not matter, as it will remove from both [a,b] and [b,a].
		/// </summary>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void RemoveEvent(PonykartCollisionGroups firstType, PonykartCollisionGroups secondType, CollisionReportEvent handler) {
			RemoveEvent((int) firstType, (int) secondType, handler);
		}

		/// <summary>
		/// Removes a handler from a collision event. 
		/// 
		/// The order of the two group IDs does not matter, as it will remove from both [a,b] and [b,a].
		/// </summary>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void RemoveEvent(int firstType, int secondType, CollisionReportEvent handler) {
			reporters[firstType, secondType] -= handler;
			reporters[secondType, firstType] -= handler;
		}

		/// <summary>
		/// invoke an event.
		/// 
		/// note that it only invokes [a,b] and not [b,a] - if it invokes both, errors will happen
		/// </summary>
		private void FireEvent(CollisionReportInfo info) {
			var e = reporters[(int) info.FirstGroup, (int) info.SecondGroup];
			if (e != null) {
				e(info);
			}
		}

		/// <summary>
		/// Clear the dictionary whenever we unload a level
		/// </summary>
		private void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			CurrentlyCollidingWith.Clear();
		}
	}
}
