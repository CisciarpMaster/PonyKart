using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Phys;
using Ponykart.Stuff;
using Mogre;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class handles movement of all NPCs. Currently it's kinda crude but it works for now.
	/// </summary>
	public class MovementHandler : IDisposable {

		ICollection<Thing> thingsToMove;
		float time = 0;
		readonly float delay = 0.3f;

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
							break;
					}
				}
			}
		}
		
		/// <summary>
		/// Adds a thing to move around.
		/// </summary>
		/// <param name="thing"></param>
		public void AddActor(Thing thing) {
			// If you try to add an thing that should be ignored, then it doesn't get added
			if (thing.MoveBehaviour != MoveBehaviour.IGNORE)
				thingsToMove.Add(thing);
		}


	}
}
