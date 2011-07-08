using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lymph.Actors;
using Lymph.Core;
using Lymph.Levels;
using Lymph.Phys;
using Lymph.Stuff;
using Mogre;
using Math = System.Math;

namespace Lymph.Handlers {
	/// <summary>
	/// This class handles movement of all NPCs. Currently it's kinda crude but it works for now.
	/// </summary>
	public class MovementHandler : IDisposable {

		ICollection<Thing> thingsToMove;
		float time = 0;
		readonly float delay = 0;//0.3f;

		public MovementHandler() {
			Launch.Log("[Loading] Creating MovementHandler");

			thingsToMove = new Collection<Thing>();

			LKernel.Get<Spawner>().OnThingCreation += AddActor;
			LKernel.Get<Root>().FrameStarted += FrameStarted;

			// for things that were created via the save file
			if (LKernel.Get<LevelManager>().CurrentLevel != null) {
				foreach (Thing t in LKernel.Get<LevelManager>().CurrentLevel.Things.Values) {
					AddActor(t);
				}
			}
		}

		public void Dispose() {
			Launch.Log("[Loading] Disposing MovementHandler");

			LKernel.Get<Spawner>().OnThingCreation -= AddActor;
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
			thingsToMove.Clear();
		}

		bool FrameStarted(FrameEvent evt) {
			if (Pauser.Paused || !LKernel.Get<LevelManager>().IsValidLevel)
				return true;

			time += evt.timeSinceLastFrame;
			if (time > delay) {
				MoveThings();
				time = 0;
			}

			return true;
		}

		void MoveThings() {
			// first of all, check to make sure this level is valid
			if (!LKernel.Get<LevelManager>().IsValidLevel)
				return;

			Player player = LKernel.Get<Player>();
			PhysXMain physics = LKernel.Get<PhysXMain>();
			// since thingsToMove holds two types of Things, we have to loop over them all and then cast them to see which they are
			foreach (Thing thing in thingsToMove)
			{
				ControllerThing ct = thing as ControllerThing;
				if (ct != null) {
					if (ct.Controller == null) {
						thingsToMove.Remove(ct);
						Launch.Log("[WARNING]: Found a null ControllerThing in the MovementHandler collection - make sure you're removing things from this!");
						continue;
					}
					switch (ct.MoveBehaviour) {
						case MoveBehaviour.TOWARDS_PLAYER:
							Vector3 vec = player.Node.Position - ct.Node.Position;
							// don't move inside the player
							Vector3 vec2 = vec * 2;
							if (Math.Abs(vec2.x) <= player.Radius && Math.Abs(vec2.z) <= player.Radius)
								continue;
							vec.Normalise();
							ct.Move(vec * ct.MoveSpeed);
							break;
					}
					continue;
				}

				KinematicThing kt = thing as KinematicThing;
				if (kt != null) {
					if (kt.Actor == null) {
						thingsToMove.Remove(kt);
						Launch.Log("[WARNING]: Found a null KinematicThing in the MovementHandler collection - make sure you're removing things from this!");
						continue;
					}
					switch (kt.MoveBehaviour)
					{
						case MoveBehaviour.TOWARDS_PLAYER:
							Vector3 vec = player.Node.Position - kt.Node.Position;
							// don't move inside the player
							Vector3 vec2 = vec * 2;
							if (Math.Abs(vec2.x) <= player.Radius && Math.Abs(vec2.z) <= player.Radius)
								continue;
							vec.Normalise();
							vec *= kt.MoveSpeed;
							kt.Actor.MoveGlobalPosition(vec + kt.Node.Position); //MoveGlobal___() methods are made just for kinematic actors!
							break;
					}
				}
			}
		}

		/// <summary>
		/// Adds a thing to move around.
		/// </summary>
		/// <param name="thing"></param>
		public void AddActor(ControllerThing thing) {
			// If you try to add an thing that should be ignored, then it doesn't get added
			if (thing.MoveBehaviour != MoveBehaviour.IGNORE)
				thingsToMove.Add(thing);
		}
		
		/// <summary>
		/// Adds a thing to move around.
		/// </summary>
		/// <param name="thing"></param>
		public void AddActor(KinematicThing thing) {
			// If you try to add an thing that should be ignored, then it doesn't get added
			if (thing.MoveBehaviour != MoveBehaviour.IGNORE)
				thingsToMove.Add(thing);
		}

		/// <summary>
		/// Adds a thing to move around if it is a subclass of ControllerThing or KinematicThing.
		/// </summary>
		/// <param name="thing"></param>
		public void AddActor(Thing thing) {
			// try casting it to a ControllerThing
			ControllerThing ct = thing as ControllerThing;
			if (ct != null)
				AddActor(ct);

			// try casting it to a KinematicThing
			KinematicThing kt = thing as KinematicThing;
			if (kt != null)
				AddActor(kt);
		}

		/// <summary>
		/// Removes a thing from the movement handler
		/// </summary>
		/// <param name="thing"></param>
		public void RemoveActor(ControllerThing thing) {
			if (thingsToMove.Contains(thing))
				thingsToMove.Remove(thing);
			else
				throw new ArgumentException("That thing could not be removed from the MovementHandler because it was never added!", "thing");
		}


	}
}
