using System.Collections.Generic;
using BulletSharp;
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
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class StopKartsFromRollingOverHandler : ILevelHandler {
		public IDictionary<Kart, SelfRightingHandler> SRHs { get; private set; }

		readonly float RAYCAST_TIME = 0.2f;
		readonly float IN_AIR_MIN_DISTANCE = 3f;

		public StopKartsFromRollingOverHandler() {
			SRHs = new Dictionary<Kart, SelfRightingHandler>();
			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		float elapsed;
		bool FrameStarted(FrameEvent evt) {
			if (elapsed > RAYCAST_TIME) {
				elapsed = 0;
				var world = LKernel.Get<PhysicsMain>().World;

				foreach (Player p in LKernel.Get<PlayerManager>().Players) {
					if (p == null)
						continue;

					Kart kart = p.Kart;
					// don't raycast for karts that don't exist! or if we're paused
					if (kart == null || kart.Body.IsDisposed || Pauser.IsPaused)
						continue;

					// get a ray pointing downwards from the kart (-Y axis)

					Vector3 from = kart.RootNode.Position + kart.RootNode.GetLocalYAxis();
					Vector3 to = from - (kart.RootNode.GetLocalYAxis() * IN_AIR_MIN_DISTANCE);

					var callback = new DynamicsWorld.ClosestRayResultCallback(from, to);
					callback.CollisionFilterMask = PonykartCollisionGroups.Environment.ToBullet();
					
					world.RayTest(from, to, callback);
#if DEBUG
					MogreDebugDrawer.Singleton.DrawLine(from, to, ColourValue.White);
#endif

					if (!callback.HasHit) {
						if (!SRHs.ContainsKey(kart)) {
							SRHs.Add(kart, new SelfRightingHandler(kart));
							System.Console.WriteLine("creating SRH for " + kart + kart.ID);
						}
					}
					else {
						SelfRightingHandler srh;
						if (SRHs.TryGetValue(kart, out srh)) {
							srh.Dispose();
							System.Console.WriteLine("disposing SRH for " + kart + kart.ID + " because it collided with " + callback.CollisionObject.GetName());
						}
					}

					//RaycastHit hit;
					// TODO: do we even need this whole ray stuff any more? Why not just use constraints?
					//Shape closestShape = LKernel.Get<PhysicsMain>().Scene.RaycastClosestShape(ray, ShapesTypes.All, out hit);

					// if the ray either didn't collide with anything or if the closest thing is >2 away, then make the kart upright
					//if (closestShape == null || hit.Distance > IN_AIR_MIN_DISTANCE) {
					//	SRHs.Add(kart, new SelfRightingHandler(kart));
					//}
				}
			}
			elapsed += evt.timeSinceLastFrame;

			return true;
		}

		public void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
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
