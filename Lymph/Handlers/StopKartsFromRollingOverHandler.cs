using System;
using System.Collections.Generic;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Physics;
using Ponykart.Players;

namespace Ponykart.Handlers {
	/// <summary>
	/// This handler finds karts that are flying in the air and turns them around so they are facing upwards.
	/// This stops them from bouncing all over the place when they land.
	/// At the moment it raycasts downwards and if the distance to the nearest thing is over than something, then we stop it from spinning in the air
	/// </summary>
	public class StopKartsFromRollingOverHandler : IDisposable {
		public IDictionary<Kart, SelfRightingHandler> SRHs { get; private set; }

		readonly float RAYCAST_TIME = 0.2f;
		readonly float IN_AIR_MIN_DISTANCE = 2f;

		public StopKartsFromRollingOverHandler() {
			SRHs = new Dictionary<Kart, SelfRightingHandler>();
			LKernel.Get<Root>().FrameEnded += FrameEnded;
		}

		float elapsed;
		bool FrameEnded(FrameEvent evt) {
			if (elapsed > RAYCAST_TIME) {
				elapsed = 0;

				foreach (Player p in LKernel.Get<PlayerManager>().Players) {
					if (p == null)
						continue;

					Kart kart = p.Kart;
					// don't bother raycasting for karts that aren't moving, or if we're paused
					if (kart == null || kart.Actor.IsDisposed || Pauser.IsPaused)
						continue;

					// check to make sure we aren't righting this kart already
					if (SRHs.ContainsKey(kart))
						continue;

					// get a ray pointing downwards from the kart (-Y axis)
					Ray ray = new Ray(kart.Node.Position, -kart.Node.GetLocalYAxis());

					RaycastHit hit;
					// TODO: check that the hit shape is either static or kinematic
					Shape closestShape = LKernel.Get<PhysicsMain>().Scene.RaycastClosestShape(ray, ShapesTypes.All, out hit);

					// if the ray either didn't collide with anything or if the closest thing is >2 away, then make the kart upright
					if (closestShape == null || hit.Distance > IN_AIR_MIN_DISTANCE) {
						SRHs.Add(kart, new SelfRightingHandler(kart));
					}
				}
			}
			elapsed += evt.timeSinceLastFrame;

			return true;
		}

		public void Dispose() {
			LKernel.Get<Root>().FrameEnded -= FrameEnded;
			// have to do this because if we change levels while SRHs is being modified, we get an exception
			SelfRightingHandler[] tempCollection = new SelfRightingHandler[SRHs.Count];
			SRHs.Values.CopyTo(tempCollection, 0);
			foreach (SelfRightingHandler h in tempCollection) {
				h.Dispose();
			}
			SRHs.Clear();
		}
	}
}
