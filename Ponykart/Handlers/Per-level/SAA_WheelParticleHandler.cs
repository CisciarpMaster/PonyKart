using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Physics;
using Ponykart.Players;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race, "SweetAppleAcres")]
	public class SAA_WheelParticleHandler : ILevelHandler {
		// our dictionaries of particles
		private IDictionary<int, Pair<WheelHelper, WheelHelper>> wheelHelpers;
		private IList<KartSpeedState> kartSpeedStates;

		// default emission rates
		//float defaultDustEmissionRate = -1;
		//float defaultGrassEmissionRate = -1;
		// woo microimprovements
		static readonly string _hyphen = "-";

		/// <summary>
		/// I mostly put this in C# because running a lua function 10 times every second per kart seemed like it could cause some slowdowns
		/// </summary>
		public SAA_WheelParticleHandler() {
			// init the dictionaries
			wheelHelpers = new Dictionary<int, Pair<WheelHelper, WheelHelper>>();
			kartSpeedStates = new List<KartSpeedState>();

			// hook up to events
			KartHandler.OnGround += OnGround;
			KartHandler.OnGroundChanged += OnGroundChanged;
			KartHandler.OnTouchdown += OnTouchdown;
			KartHandler.OnLiftoff += OnLiftoff;

			var sceneMgr = LKernel.GetG<SceneManager>();

			// create particles for each wheel
			foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
				

				WheelHelper lefthelper = new WheelHelper();

				lefthelper.dust1 = sceneMgr.CreateParticleSystem("wheelDust1LeftParticle" + p.ID, "dust1");
				lefthelper.dust2 = sceneMgr.CreateParticleSystem("wheelDust2LeftParticle" + p.ID, "dust1");
				lefthelper.dust3 = sceneMgr.CreateParticleSystem("wheelDust3LeftParticle" + p.ID, "dust1");
				lefthelper.dust4 = sceneMgr.CreateParticleSystem("wheelDust4LeftParticle" + p.ID, "dust1");
				lefthelper.mud = sceneMgr.CreateParticleSystem("wheelMudLeftParticle" + p.ID, "SAA/mud");
				lefthelper.grass = sceneMgr.CreateParticleSystem("wheelGrassLeftParticle" + p.ID, "SAA/grass");

				p.Kart.LeftParticleNode.AttachObject(lefthelper.dust1);
				p.Kart.LeftParticleNode.AttachObject(lefthelper.dust2);
				p.Kart.LeftParticleNode.AttachObject(lefthelper.dust3);
				p.Kart.LeftParticleNode.AttachObject(lefthelper.dust4);
				p.Kart.LeftParticleNode.AttachObject(lefthelper.mud);
				//p.Kart.LeftParticleNode.AttachObject(lefthelper.grass);

				lefthelper.DisableDust();
				lefthelper.DisableGrass();

				WheelHelper righthelper = new WheelHelper();

				righthelper.dust1 = sceneMgr.CreateParticleSystem("wheelDust1RightParticle" + p.ID, "dust1");
				righthelper.dust2 = sceneMgr.CreateParticleSystem("wheelDust2RightParticle" + p.ID, "dust1");
				righthelper.dust3 = sceneMgr.CreateParticleSystem("wheelDust3RightParticle" + p.ID, "dust1");
				righthelper.dust4 = sceneMgr.CreateParticleSystem("wheelDust4RightParticle" + p.ID, "dust1");
				righthelper.mud = sceneMgr.CreateParticleSystem("wheelMudRightParticle" + p.ID, "SAA/mud");
				righthelper.grass = sceneMgr.CreateParticleSystem("wheelGrassRightParticle" + p.ID, "SAA/grass");

				p.Kart.RightParticleNode.AttachObject(righthelper.dust1);
				p.Kart.RightParticleNode.AttachObject(righthelper.dust2);
				p.Kart.RightParticleNode.AttachObject(righthelper.dust3);
				p.Kart.RightParticleNode.AttachObject(righthelper.dust4);
				p.Kart.RightParticleNode.AttachObject(righthelper.mud);
				//p.Kart.RightParticleNode.AttachObject(righthelper.grass);

				righthelper.DisableDust();
				righthelper.DisableGrass();

				/*if (defaultDustEmissionRate == -1)
					defaultDustEmissionRate = helper.dust1.GetEmitter(0).EmissionRate;
				if (defaultGrassEmissionRate == -1)
					defaultGrassEmissionRate = helper.grass.GetEmitter(0).EmissionRate;*/

				wheelHelpers.Add(p.ID, new Pair<WheelHelper, WheelHelper>(lefthelper, righthelper));

				// add one of these to the states list
				kartSpeedStates.Add(KartSpeedState.None);
			}
		}

		/// <summary>
		/// little helper
		/// </summary>
		bool IsDirt(string objName) {
			return objName == "SAA_Road_Combined";
		}
		/// <summary>
		/// little helper
		/// </summary>
		bool IsGrass(string objName) {
			return objName == "SAA_Ground_Collidable";
		}

		/// <summary>
		/// Sets all of the emission states of the dirt particles of a kart
		/// </summary>
		void DirtEmitting(int kartID, bool isEmitting) {
			wheelHelpers[kartID].first.Dust(isEmitting);
			wheelHelpers[kartID].second.Dust(isEmitting);
		}

		/// <summary>
		/// Sets all of the emission states of the grass particles of a kart
		/// </summary>
		void GrassEmitting(int kartID, bool isEmitting) {
			wheelHelpers[kartID].first.Grass(isEmitting);
			wheelHelpers[kartID].second.Grass(isEmitting);
		}

		/// <summary>
		/// Sets all of the emission states of both the dirt and grass particles of a kart
		/// </summary>
		void BothEmitting(int kartID, bool isEmitting) {
			wheelHelpers[kartID].first.Dust(isEmitting);
			wheelHelpers[kartID].second.Dust(isEmitting);

			wheelHelpers[kartID].first.Grass(isEmitting);
			wheelHelpers[kartID].second.Grass(isEmitting);
		}

		/// <summary>
		/// turn off all particles
		/// </summary>
		void OnLiftoff(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			BothEmitting(kart.OwnerID, false);
		}

		/// <summary>
		/// turn on the appropriate particles
		/// </summary>
		void OnTouchdown(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			if (kart.WheelSpeed > 20f || kart.WheelSpeed < -20f) {
				string name = callback.CollisionObject.GetName();
				if (IsDirt(name))
					DirtEmitting(kart.OwnerID, true);
				else if (IsGrass(name))
					GrassEmitting(kart.OwnerID, true);
			}
			else {
				DirtEmitting(kart.OwnerID, false);
				GrassEmitting(kart.OwnerID, false);
			}
		}

		/// <summary>
		/// Change the particles appropriately
		/// </summary>
		void OnGroundChanged(Kart kart, CollisionObject newGround, CollisionObject oldGround) {
			string newName = newGround.GetName();
			string oldName = oldGround.GetName();

			if (IsDirt(newName))
				DirtEmitting(kart.OwnerID, true);
			else if (IsDirt(oldName))
				DirtEmitting(kart.OwnerID, false);

			if (IsGrass(newName))
				GrassEmitting(kart.OwnerID, true);
			else if (IsGrass(oldName))
				GrassEmitting(kart.OwnerID, false);
		}

		/// <summary>
		/// update the particle emission rates depending on speed
		/// </summary>
		void OnGround(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			float speed = System.Math.Abs(kart.WheelSpeed);

			// if the kart is moving slowly, then just turn the particles completely off
			if (speed < 30) {
				if (kartSpeedStates[kart.OwnerID] != KartSpeedState.Slow) {
					// update this if we need to
					kartSpeedStates[kart.OwnerID] = KartSpeedState.Slow;
					BothEmitting(kart.OwnerID, false);
				}
			}
			// if we're moving at a medium speed
			else if (speed >= 30 && speed < 150) {
				if (kartSpeedStates[kart.OwnerID] != KartSpeedState.Medium) {
					// update this if we need to
					kartSpeedStates[kart.OwnerID] = KartSpeedState.Medium;

					if (IsDirt(callback.CollisionObject.GetName()))
						DirtEmitting(kart.OwnerID, true);
					else if (IsGrass(callback.CollisionObject.GetName()))
						GrassEmitting(kart.OwnerID, true);
				}

				// update the particles
				/*string idWithHypen = kart.OwnerID + _hyphen;
				// make some new emission rates
				float dirtEmissionRate = (speed / 150) * defaultDustEmissionRate;
				float grassEmissionRate = (speed / 150) * defaultGrassEmissionRate;
				for (int a = 0; a < 4; a++) {
					// and update the particles
					dirtParticles[idWithHypen + a].GetEmitter(0).EmissionRate = dirtEmissionRate;
					grassParticles[idWithHypen + a].GetEmitter(0).EmissionRate = grassEmissionRate;
				}*/
			}
			// and if we're moving at a fast speed
			else {
				/*if (kartSpeedStates[kart.OwnerID] != KartSpeedState.Fast) {
					kartSpeedStates[kart.OwnerID] = KartSpeedState.Fast;

					string idWithHypen = kart.OwnerID + _hyphen;
					for (int a = 0; a < 4; a++) {
						// update all of the particles to use the default emission rate instead of whatever it was set to before
						dirtParticles[idWithHypen + a].GetEmitter(0).EmissionRate = defaultDustEmissionRate;
						grassParticles[idWithHypen + a].GetEmitter(0).EmissionRate = defaultGrassEmissionRate;
					}
				}*/
			}
		}

		public void Detach() {
			KartHandler.OnGround -= OnGround;
			KartHandler.OnGroundChanged -= OnGroundChanged;
			KartHandler.OnTouchdown -= OnTouchdown;
			KartHandler.OnLiftoff -= OnLiftoff;
		}

		private enum KartSpeedState {
			None, Slow, Medium, Fast
		}

		private class WheelHelper {
			public ParticleSystem mud, grass, dust1, dust2, dust3, dust4;

			public void EnableGrass() {
				mud.Emitting = grass.Emitting = true;
			}

			public void Grass(bool enabled) {
				mud.Emitting = grass.Emitting = enabled;
			}

			public void DisableGrass() {
				mud.Emitting = grass.Emitting = false;
			}

			public void EnableDust() {
				dust1.Emitting = dust2.Emitting = dust3.Emitting = dust4.Emitting = true;
			}

			public void Dust(bool enabled) {
				dust1.Emitting = dust2.Emitting = dust3.Emitting = dust4.Emitting = enabled;
			}

			public void DisableDust() {
				dust1.Emitting = dust2.Emitting = dust3.Emitting = dust4.Emitting = false;
			}
		}
	}
}
