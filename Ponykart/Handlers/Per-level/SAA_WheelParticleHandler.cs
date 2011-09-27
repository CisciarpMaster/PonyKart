using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Physics;
using Ponykart.Players;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race, "saa")]
	public class SAA_WheelParticleHandler : ILevelHandler {
		// our dictionaries of particles
		private IDictionary<string, ParticleSystem> dirtParticles, grassParticles;
		private IList<KartSpeedState> kartSpeedStates;

		// default emission rates
		float defaultDirtEmissionRate = -1;
		float defaultGrassEmissionRate = -1;
		// woo microimprovements
		static readonly string _hyphen = "-";

		/// <summary>
		/// I mostly put this in C# because running a lua function 10 times every second per kart seemed like it could cause some slowdowns
		/// </summary>
		public SAA_WheelParticleHandler() {
			// init the dictionaries
			dirtParticles = new Dictionary<string, ParticleSystem>();
			grassParticles = new Dictionary<string, ParticleSystem>();
			kartSpeedStates = new List<KartSpeedState>();

			// hook up to events
			var kartHandler = LKernel.Get<KartHandler>();
			kartHandler.OnGround += OnGround;
			kartHandler.OnGroundChanged += OnGroundChanged;
			kartHandler.OnTouchdown += OnTouchdown;
			kartHandler.OnLiftoff += OnLiftoff;

			var sceneMgr = LKernel.GetG<SceneManager>();

			// create particles for each wheel
			foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
				for (int a = 0; a < 4; a++) {
					var dirt = sceneMgr.CreateParticleSystem(string.Concat("wheelDirtParticle", p.ID, _hyphen, a), "dirt");
					var grass = sceneMgr.CreateParticleSystem(string.Concat("wheelGrassParticle", p.ID, _hyphen, a), "grass");

					dirt.Emitting = false;
					grass.Emitting = false;

					var wheel = p.Kart.GetWheel(a);
					wheel.Node.AttachObject(dirt);
					wheel.Node.AttachObject(grass);

					dirtParticles[string.Concat(p.ID, _hyphen, a)] = dirt;
					grassParticles[string.Concat(p.ID, _hyphen, a)] = grass;

					if (defaultDirtEmissionRate == -1)
						defaultDirtEmissionRate = dirt.GetEmitter(0).EmissionRate;
					if (defaultGrassEmissionRate == -1)
						defaultGrassEmissionRate = grass.GetEmitter(0).EmissionRate;
				}

				// add one of these to the states list
				kartSpeedStates.Add(KartSpeedState.None);
			}
		}

		/// <summary>
		/// little helper
		/// </summary>
		bool IsDirt(string objName) {
			return objName == "SAA_Road" || objName == "SAA_Barn_Area";
		}
		/// <summary>
		/// little helper
		/// </summary>
		bool IsGrass(string objName) {
			return objName == "SAA_HP_Ground_Collidable";
		}

		/// <summary>
		/// Sets all of the emission states of the dirt particles of a kart
		/// </summary>
		void DirtEmitting(int kartID, bool isEmitting) {
			string idWithHyphen = kartID + _hyphen;
			for (int a = 0; a < 4; a++) {
				dirtParticles[idWithHyphen + a].Emitting = isEmitting;
			}
		}

		/// <summary>
		/// Sets all of the emission states of the grass particles of a kart
		/// </summary>
		void GrassEmitting(int kartID, bool isEmitting) {
			string idWithHyphen = kartID + _hyphen;
			for (int a = 0; a < 4; a++) {
				grassParticles[idWithHyphen + a].Emitting = isEmitting;
			}
		}

		/// <summary>
		/// Sets all of the emission states of both the dirt and grass particles of a kart
		/// </summary>
		void BothEmitting(int kartID, bool isEmitting) {
			string idWithHyphen = kartID + _hyphen;
			for (int a = 0; a < 4; a++) {
				dirtParticles[idWithHyphen + a].Emitting = isEmitting;
				grassParticles[idWithHyphen + a].Emitting = isEmitting;
			}
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
			string name = callback.CollisionObject.GetName();
			if (IsDirt(name))
				DirtEmitting(kart.OwnerID, true);
			else if (IsGrass(name))
				GrassEmitting(kart.OwnerID, true);
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

		enum KartSpeedState {
			None, Slow, Medium, Fast
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
				string idWithHypen = kart.OwnerID + _hyphen;
				// make some new emission rates
				float dirtEmissionRate = (speed / 150) * defaultDirtEmissionRate;
				float grassEmissionRate = (speed / 150) * defaultGrassEmissionRate;
				for (int a = 0; a < 4; a++) {
					// and update the particles
					dirtParticles[idWithHypen + a].GetEmitter(0).EmissionRate = dirtEmissionRate;
					grassParticles[idWithHypen + a].GetEmitter(0).EmissionRate = grassEmissionRate;
				}
			}
			// and if we're moving at a fast speed
			else {
				if (kartSpeedStates[kart.OwnerID] != KartSpeedState.Fast) {
					kartSpeedStates[kart.OwnerID] = KartSpeedState.Fast;

					string idWithHypen = kart.OwnerID + _hyphen;
					for (int a = 0; a < 4; a++) {
						// update all of the particles to use the default emission rate instead of whatever it was set to before
						dirtParticles[idWithHypen + a].GetEmitter(0).EmissionRate = defaultDirtEmissionRate;
						grassParticles[idWithHypen + a].GetEmitter(0).EmissionRate = defaultGrassEmissionRate;
					}
				}
			}
		}

		public void Detach() {
			var kartHandler = LKernel.Get<KartHandler>();
			kartHandler.OnGround -= OnGround;
			kartHandler.OnGroundChanged -= OnGroundChanged;
			kartHandler.OnTouchdown -= OnTouchdown;
			kartHandler.OnLiftoff -= OnLiftoff;
		}
	}
}
