using System;
using System.Collections.Generic;
using Mogre;
using Mogre.PhysX;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Phys;
using Ponykart.Players;

namespace Ponykart.Handlers {
	/// <summary>
	/// This'll be a class that stops karts from rolling over and spinning around when they're in the air,
	/// but it isn't really working right now.
	/// At the moment it raycasts downwards and if the distance to the nearest thing is over than something, then we stop it from spinning in the air
	/// </summary>
	public class StopKartsFromRollingOverHandler : IDisposable {
		public IDictionary<Kart, SelfRightingHandler> SRHs { get; private set; }

		public StopKartsFromRollingOverHandler() {
			SRHs = new Dictionary<Kart, SelfRightingHandler>();
			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		float elapsed;
		bool FrameStarted(FrameEvent evt) {
			if (elapsed > 0.2f) {
				elapsed = 0;

				foreach (Player p in LKernel.Get<PlayerManager>().Players) {
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
					Shape closestShape = LKernel.Get<PhysXMain>().Scene.RaycastClosestShape(ray, ShapesTypes.All, out hit);

					// if the ray either didn't collide with anything or if the closest thing is >2 away, then make the kart upright
					if (closestShape == null || hit.Distance > 2f) {
						SRHs.Add(kart, new SelfRightingHandler(kart));
					}
				}
			}
			elapsed += evt.timeSinceLastFrame;

			return true;
		}

		public void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
			foreach (SelfRightingHandler h in SRHs.Values) {
				h.Dispose();
			}
			SRHs.Clear();
		}
	}
}
