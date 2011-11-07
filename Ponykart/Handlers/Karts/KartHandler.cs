using System.Collections.Concurrent;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Handlers {
	public delegate void KartRayEvent(Kart kart, DynamicsWorld.ClosestRayResultCallback callback);
	public delegate void KartGroundEvent(Kart kart, CollisionObject newGround, CollisionObject oldGround);

	/// <summary>
	/// This handler finds karts that are flying in the air and turns them around so they are facing upwards.
	/// This stops them from bouncing all over the place when they land.
	/// At the moment it raycasts downwards and if it's in the air, then it self-rights.
	/// When it approaches the ground, it changes direction to the ground's normal and makes the kart skid a little.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class KartHandler {
		/// <summary>
		/// This holds all of our self-righting handlers
		/// </summary>
		public ConcurrentDictionary<Kart, SelfRighter> SelfRighters { get; private set; }
		/// <summary>
		/// Dictionary of our nlerpers
		/// </summary>
		public ConcurrentDictionary<Kart, Nlerper<Kart>> Nlerpers { get; private set; }
		/// <summary>
		/// Dictionary of our skidders
		/// </summary>
		public ConcurrentDictionary<Kart, Skidder> Skidders { get; private set; }
		/// <summary>
		/// What is the kart currently driving on?
		/// </summary>
		public ConcurrentDictionary<Kart, CollisionObject> CurrentlyDrivingOn { get; private set; }

		/// <summary>
		/// Fired once when a kart stops touching the ground and is now in the air, i.e. the short ray doesn't penetrate the ground
		/// </summary>
		public static event KartRayEvent OnLiftoff;
		/// <summary>
		/// Fired every frame when a kart is in the air
		/// </summary>
		public static event KartRayEvent OnInAir;
		/// <summary>
		/// Fired once just before a kart lands, i.e. when the long ray penetrates the ground
		/// </summary>
		public static event KartRayEvent OnCloseToTouchdown;
		/// <summary>
		/// Fired once when the kart lands, i.e. when the short ray penetrates the ground again
		/// </summary>
		public static event KartRayEvent OnTouchdown;
		/// <summary>
		/// Fired every frame when a kart is on the ground
		/// </summary>
		public static event KartRayEvent OnGround;
		/// <summary>
		/// Fired once when the mesh the kart is currently driving on changes
		/// </summary>
		public static event KartGroundEvent OnGroundChanged;

#region Settings
		private readonly float raycastTime = Settings.Default.SelfRighterRaycastTime,
							   longRayLength = Settings.Default.SelfRighterLongRayLength,
							   shortRayLength = Settings.Default.SelfRighterShortRayLength,
							   kartGravityMultiplier = Settings.Default.AdjustKartGravityMultiplier,
							   skidderDuration = Settings.Default.SkidderDuration;
		private readonly bool kartGravityEnabled = Settings.Default.AdjustKartGravityEnabled,
							  useNlerpers = Settings.Default.KartHandler_UseNlerpers,
							  useSkidders = Settings.Default.KartHandler_UseSkidders,
							  useSelfRighters = Settings.Default.KartHandler_UseSelfRighters;
		private readonly Vector3 gravity = new Vector3(0, Settings.Default.Gravity, 0);
#endregion


		public KartHandler() {
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		/// <summary>
		/// Sets up our dictionaries and hooks up to the frame started event
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type == LevelType.Race) {
				SelfRighters = new ConcurrentDictionary<Kart, SelfRighter>();
				Nlerpers = new ConcurrentDictionary<Kart, Nlerper<Kart>>();
				Skidders = new ConcurrentDictionary<Kart, Skidder>();
				CurrentlyDrivingOn = new ConcurrentDictionary<Kart, CollisionObject>();

				PhysicsMain.PreSimulate += PreSimulate;
			}
		}

		
		private float elapsed;

		private void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (Pauser.IsPaused || !LKernel.GetG<LevelManager>().IsValidLevel)
				return;

			if (elapsed > raycastTime) {
				elapsed = 0;

				// loop through each player's kart
				foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
					// if the player is null, then skip it
					//if (p == null)
					//	continue;

					Kart kart = p.Kart;
					// don't raycast for karts that don't exist
					if (kart == null || kart.Body.IsDisposed)
						continue;

					// then cast our ray!
					var callback = CastRay(kart, (kart.IsInAir && SelfRighters.ContainsKey(kart) ? longRayLength : shortRayLength), world);


					// this helps it stick to the road more
					if (kartGravityEnabled) {
						if (kart.IsInAir)
							kart.Body.Gravity = gravity;
						else if (callback.HasHit && callback.CollisionObject.GetCollisionGroup() == PonykartCollisionGroups.Road)
							kart.Body.Gravity = gravity + (kart.RootNode.GetLocalYAxis() * kartGravityMultiplier);
					}

					// if the ray did not hit
					if (!callback.HasHit) {
						// if we do not have an SRH, that means we were just on the ground
						if (!SelfRighters.ContainsKey(kart))
							Liftoff(kart, callback);
						// if we do have one, that means we're still in the air
						else
							InAir(kart, callback);
					}
					// if the ray did hit
					else {
						SelfRighter srh;
						// if we have an SRH, that means we were just in the air but we haven't touched down yet
						if (SelfRighters.TryGetValue(kart, out srh))
							GettingCloseToTouchingDown(kart, callback, srh);
						// we don't have an SRH, so we're near the ground, but we aren't quite there yet
						else if (kart.IsInAir)
							TouchDown(kart, callback);
						// yeah we're still on the ground
						else
							Ground(kart, callback);
					}

					callback.Dispose();
				}
			}
			elapsed += evt.timeSinceLastFrame;
		}

		readonly CollisionFilterGroups rayFilterGroup = (PonykartCollisionGroups.Environment | PonykartCollisionGroups.Road).ToBullet();
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
			callback.CollisionFilterMask = rayFilterGroup;
			
			world.RayTest(from, to, callback);
#if DEBUG
			//MogreDebugDrawer.Singleton.DrawLine(from, to, ColourValue.Red);
#endif
			return callback;
		}

		/// <summary>
		/// Run when our short ray detects that we're not on the ground any more
		/// </summary>
		private void Liftoff(Kart kart, DynamicsWorld.ClosestRayResultCallback callback) {
			// make a new SRH
			if (useSelfRighters)
				SelfRighters.GetOrAdd(kart, new SelfRighter(kart));
			// we are in the air
			kart.IsInAir = true;

			CurrentlyDrivingOn[kart] = null;

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
		private void GettingCloseToTouchingDown(Kart kart, DynamicsWorld.ClosestRayResultCallback callback, SelfRighter srh) {
			// getting rid of our SRH means that we're close to landing but we haven't landed yet
			if (useSelfRighters) {
				srh.Detach();
				SelfRighters.TryRemove(kart, out srh);
			}

			if (useNlerpers)
				AlignKartWithNormal(kart, callback, true, 0.2f);

			if (OnCloseToTouchdown != null)
				OnCloseToTouchdown(kart, callback);
		}

		/// <summary>
		/// Run when our short ray penetrates the ground but we have not yet "landed"
		/// </summary>
		private void TouchDown(Kart kart, DynamicsWorld.ClosestRayResultCallback callback) {
			// now we have actually landed
			kart.IsInAir = false;

			// if we have a nlerper, get rid of it
			if (useNlerpers) {
				Nlerper<Kart> n;
				if (Nlerpers.TryGetValue(kart, out n)) {
					n.Detach();
					Nlerpers.TryRemove(kart, out n);
				}
			}

			// add a skidder!
			if (useSkidders) {
				Skidder s;
				if (Skidders.TryGetValue(kart, out s)) {
					s.Detach();
					Skidders.TryRemove(kart, out s);
				}
				Skidders[kart] = new Skidder(kart, skidderDuration);
			}

			// align the kart just to make sure
			if (useNlerpers)
				AlignKartWithNormal(kart, callback, true, 0.1f);

			CurrentlyDrivingOn[kart] = callback.CollisionObject;

			if (OnTouchdown != null)
				OnTouchdown(kart, callback);
		}

		/// <summary>
		/// Run when our kart is still on the ground
		/// </summary>
		private void Ground(Kart kart, DynamicsWorld.ClosestRayResultCallback callback) {
			// first get the object we're currently driving on
			CollisionObject current;
			if (CurrentlyDrivingOn.TryGetValue(kart, out current)) {
				// if it's different than the old one, then we need to run our event and update the dictionary
				if (current != callback.CollisionObject) {
					// run the event
					if (OnGroundChanged != null)
						OnGroundChanged(kart, callback.CollisionObject, CurrentlyDrivingOn[kart]);

					// and then update the dictionary
					CurrentlyDrivingOn[kart] = callback.CollisionObject;
				}
			}

			// run this too
			if (OnGround != null)
				OnGround(kart, callback);
		}

		/// <summary>
		/// Aligns the kart with the callback's normal
		/// </summary>
		/// <param name="useNlerp">Should we use a nlerper? If false, this just updates the orientation instantly.</param>
		private void AlignKartWithNormal(Kart kart, DynamicsWorld.ClosestRayResultCallback callback, bool useNlerp, float duration = 1) {
			// don't add a nlerper if we're drifting
			if (kart.IsDriftingAtAll)
				return;

			// don't bother if they're already within 1 degree of each other
			if (kart.Body.Orientation.YAxis.DirectionEquals(callback.HitNormalWorld, 0.01745f))
				return;

			// get the rotation we need to do to rotate this kart to the ground's normal
			Quaternion rotTo = kart.Body.Orientation.YAxis.GetRotationTo(callback.HitNormalWorld);
			// rotTo * old orientation is the same as rotate(rotTo) on SceneNodes, but since this isn't a scene node we have to do it this way
			Quaternion newOrientation = rotTo * kart.Body.Orientation;

			if (useNlerp && useNlerpers) {
				// if we already have a nlerper, get rid of it
				Nlerper<Kart> n;
				if (Nlerpers.TryGetValue(kart, out n)) {
					n.Detach();
				}
				Nlerpers[kart] = new Nlerper<Kart>(kart, duration, newOrientation);
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
				PhysicsMain.PreSimulate -= PreSimulate;

				// clean up these
				foreach (SelfRighter h in SelfRighters.Values) {
					h.Detach();
				}
				SelfRighters.Clear();

				// same for these
				foreach (Nlerper<Kart> n in Nlerpers.Values) {
					n.Detach();
				}
				Nlerpers.Clear();

				// and these
				foreach (Skidder s in Skidders.Values) {
					s.Detach();
				}
				Skidders.Clear();
			}
		}
	}
}
