using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.Players;

namespace Ponykart.Handlers {
	public delegate void KartEvent(Kart kart, DynamicsWorld.ClosestRayResultCallback callback);

	/// <summary>
	/// This handler finds karts that are flying in the air and turns them around so they are facing upwards.
	/// This stops them from bouncing all over the place when they land.
	/// At the moment it raycasts downwards and if it's in the air, then it self-rights.
	/// When it approaches the ground, it changes direction to the ground's normal.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class StopKartsFromRollingOverHandler {
		/// <summary>
		/// This holds all of our self-righting handlers
		/// </summary>
		public IDictionary<Kart, SelfRightingHandler> SRHs { get; private set; }
		/// <summary>
		/// This tells us if a certain kart is in the air or not
		/// </summary>
		private IDictionary<Kart, bool> IsInAir;
		/// <summary>
		/// Dictionary of our nlerpers
		/// </summary>
		public IDictionary<Kart, Nlerper> Nlerpers { get; private set; }
		/// <summary>
		/// Dictionary of our skidders
		/// </summary>
		public IDictionary<Kart, Skidder> Skidders { get; private set; }

		public event KartEvent OnLiftoff;
		public event KartEvent OnInAir;
		public event KartEvent OnCloseToTouchdown;
		public event KartEvent OnTouchdown;

		/// <summary>
		/// How often should we raycast?
		/// </summary>
		private readonly float RAYCAST_TIME = 0.1f;
		/// <summary>
		/// Our "short" ray
		/// </summary>
		private readonly float SHORT_RAY_LENGTH = 1.5f;
		/// <summary>
		/// Our "long" ray
		/// </summary>
		private readonly float LONG_RAY_LENGTH = 5f;

		public StopKartsFromRollingOverHandler() {
			LKernel.GetG<LevelManager>().OnLevelLoad += new LevelEventHandler(OnLevelLoad);
			LKernel.GetG<LevelManager>().OnLevelUnload += new LevelEventHandler(OnLevelUnload);
		}

		/// <summary>
		/// Sets up our dictionaries and hooks up to the frame started event
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type == LevelType.Race) {
				SRHs = new Dictionary<Kart, SelfRightingHandler>();
				IsInAir = new Dictionary<Kart, bool>();
				Nlerpers = new Dictionary<Kart, Nlerper>();
				Skidders = new Dictionary<Kart, Skidder>();

				// set up our dictionary
				foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
					IsInAir.Add(p.Kart, false);
				}

				LKernel.GetG<PhysicsMain>().PreSimulate += PreSimulate;
			}
		}

		private float elapsed;
		private void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (elapsed > RAYCAST_TIME) {
				elapsed = 0;

				// loop through each player's kart
				foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
					// if the player is null, then skip it
					if (p == null || Pauser.IsPaused)
						continue;

					Kart kart = p.Kart;
					// don't raycast for karts that don't exist
					if (kart == null || kart.Body.IsDisposed ) // 3 degrees
						continue;


					var callback = CastRay(kart, (IsInAir[kart] && SRHs.ContainsKey(kart) ? LONG_RAY_LENGTH : SHORT_RAY_LENGTH), world);

					// if the ray did not hit
					if (!callback.HasHit) {
						// if we do not have an SRH, that means we were just on the ground
						if (!SRHs.ContainsKey(kart))
							Liftoff(kart, callback);
						// if we do have one, that means we're still in the air
						else
							InAir(kart, callback);
					}
					// if the ray did hit
					else {
						SelfRightingHandler srh;
						// if we have an SRH, that means we were just in the air but we haven't touched down yet
						if (SRHs.TryGetValue(kart, out srh))
							GettingCloseToTouchingDown(kart, callback, srh);
						// we don't have an SRH, so we're near the ground, but we aren't quite there yet
						else if (IsInAir[kart])
							TouchDown(kart, callback);
						// if we had an "else" here, it would run every frame that the kart is touching the ground.
					}
				}
			}
			elapsed += evt.timeSinceLastFrame;
		}

		/// <summary>
		/// Casts a ray downwards from the given kart
		/// </summary>
		/// <returns>The ray result callback</returns>
		private DynamicsWorld.ClosestRayResultCallback CastRay(Kart kart, float rayLength, DiscreteDynamicsWorld world) {
			// get a ray pointing downwards from the kart (-Y axis)
			Vector3 from = kart.RootNode.Position + kart.RootNode.GetLocalYAxis(); // have to move it up a bit
			Vector3 to = from - kart.RootNode.GetLocalYAxis() * (rayLength + 1); // add 1 to compensate for the "moving up" we did to the "from" vector

			// make our ray
			var callback = new DynamicsWorld.ClosestRayResultCallback(from, to);
			// we only want the ray to collide with the environment and nothing else
			callback.CollisionFilterMask = PonykartCollisionGroups.Environment.ToBullet();
			
			world.RayTest(from, to, callback);
#if DEBUG
			MogreDebugDrawer.Singleton.DrawLine(from, to, ColourValue.White);
#endif
			return callback;
		}

		/// <summary>
		/// Run when our short ray detects that we're not on the ground any more
		/// </summary>
		private void Liftoff(Kart kart, DynamicsWorld.ClosestRayResultCallback callback) {
			// make a new SRH
			SRHs.Add(kart, new SelfRightingHandler(kart));
			// we are in the air
			IsInAir[kart] = true;

			if (OnLiftoff != null)
				OnLiftoff(kart, callback);
		}

		/// <summary>
		/// Run when our kart is currently in the air
		/// </summary>
		private void InAir(Kart kart, DynamicsWorld.ClosestRayResultCallback callback) {
			if (OnInAir != null)
				OnInAir(kart, callback);
		}

		/// <summary>
		/// Run when our long ray penetrates the ground
		/// </summary>
		private void GettingCloseToTouchingDown(Kart kart, DynamicsWorld.ClosestRayResultCallback callback, SelfRightingHandler srh) {
			// getting rid of our SRH means that we're close to landing but we haven't landed yet
			srh.Dispose();
			SRHs.Remove(kart);

			if (kart.Body.LinearVelocity.y > 20)
				kart.Body.LinearVelocity = new Vector3(kart.Body.LinearVelocity.x, 20, kart.Body.LinearVelocity.z);

			AlignKartWithNormal(kart, callback, true, 1);

			if (OnCloseToTouchdown != null)
				OnCloseToTouchdown(kart, callback);
		}

		/// <summary>
		/// Run when our short ray penetrates the ground but we have not yet "landed"
		/// </summary>
		private void TouchDown(Kart kart, DynamicsWorld.ClosestRayResultCallback callback) {
			// now we have actually landed
			IsInAir[kart] = false;

			// if we have a nlerper, get rid of it
			Nlerper n;
			if (Nlerpers.TryGetValue(kart, out n)) {
				n.Dispose();
				Nlerpers.Remove(kart);
			}

			// add a skidder!
			Skidder s;
			if (Skidders.TryGetValue(kart, out s)) {
				s.Dispose();
				Skidders.Remove(kart);
			}
			Skidders[kart] = new Skidder(kart, 0.7f);

			// align the kart just to make sure
			AlignKartWithNormal(kart, callback, false);

			if (OnTouchdown != null)
				OnTouchdown(kart, callback);
		}

		/// <summary>
		/// Aligns the kart with the callback's normal
		/// </summary>
		/// <param name="useNlerp">Should we use a nlerper? If false, this just updates the orientation instantly.</param>
		private void AlignKartWithNormal(Kart kart, DynamicsWorld.ClosestRayResultCallback callback, bool useNlerp, float duration = 1) {
			// don't bother if they're already within 1 degree of each other
			if (kart.Body.Orientation.YAxis.DirectionEquals(callback.HitNormalWorld, 0.01745f))
				return;

			// get the rotation we need to do to rotate this kart to the ground's normal
			Quaternion rotTo = kart.Body.Orientation.YAxis.GetRotationTo(callback.HitNormalWorld);
			// rotTo * old orientation is the same as rotate(rotTo) on SceneNodes, but since this isn't a scene node we have to do it this way
			Quaternion newOrientation = rotTo * kart.Body.Orientation;
#if DEBUG
			// make a little axis to show our normal
			//LKernel.Get<Spawner>().Spawn("Axis", callback.HitPointWorld).RootNode.Orientation = newOrientation;
#endif
			if (useNlerp) {
				// if we already have a nlerper, get rid of it
				Nlerper n;
				if (Nlerpers.TryGetValue(kart, out n)) {
					n.Dispose();
				}
				Nlerpers[kart] = new Nlerper(kart, duration, newOrientation);
			}
			else {
				// update our body
				kart.Body.SetOrientation(newOrientation);
			}
		}

		/// <summary>
		/// cleanup
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			if (eventArgs.OldLevel.Type == LevelType.Race) {
				LKernel.GetG<PhysicsMain>().PreSimulate -= PreSimulate;

				// have to do this because if we change levels while SRHs is being modified, we get an exception
				SelfRightingHandler[] srh = new SelfRightingHandler[SRHs.Count];
				SRHs.Values.CopyTo(srh, 0);
				foreach (SelfRightingHandler h in srh) {
					h.Dispose();
				}
				SRHs.Clear();

				// same for these
				Nlerper[] ns = new Nlerper[Nlerpers.Count];
				Nlerpers.Values.CopyTo(ns, 0);
				foreach (Nlerper n in ns) {
					n.Dispose();
				}
				Nlerpers.Clear();

				// and these
				Skidder[] ss = new Skidder[Skidders.Count];
				Skidders.Values.CopyTo(ss, 0);
				foreach (Skidder s in ss) {
					s.Dispose();
				}
				Skidders.Clear();

				// and finally
				IsInAir.Clear();
			}
		}
	}
}
