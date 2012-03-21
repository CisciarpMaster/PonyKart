using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Physics;

namespace Ponykart.Players {
	[Handler(HandlerScope.Global)]
	public class AITriggerRegionHandler {
		CultureInfo culture = CultureInfo.InvariantCulture;

		IDictionary<int, TriggerRegion> triggerRegions;
		IDictionary<TriggerRegion, TriggerRegion> nextTriggerRegions;

		public AITriggerRegionHandler() {
			triggerRegions = new Dictionary<int, TriggerRegion>();
			nextTriggerRegions = new Dictionary<TriggerRegion, TriggerRegion>();

			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}


		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type != LevelType.Race)
				return;

			triggerRegions = new Dictionary<int, TriggerRegion>();
			nextTriggerRegions = new Dictionary<TriggerRegion, TriggerRegion>();

			/*
			 * threshold
			 * id idTO height width posX posY posZ orientX orientY orientZ orientW
			 */

			if (File.Exists("media/worlds/" + eventArgs.NewLevel.Name + ".tr")) {

				var collisionShapeMgr = LKernel.GetG<CollisionShapeManager>();
				IDictionary<TriggerRegion, int> tempDic = new Dictionary<TriggerRegion, int>();

				using (StreamReader reader = new StreamReader("media/worlds/" + eventArgs.NewLevel.Name + ".tr")) {
					float threshold = float.Parse(reader.ReadLine());

					string line;
					while ((line = reader.ReadLine()) != null) {
						// ignore comments
						if (line.StartsWith("//"))
							continue;

						string[] parts = line.Split(' ');
						if (parts.Length == 11) {
							int id = int.Parse(parts[0], culture);
							int idTo = int.Parse(parts[1], culture);
							Vector3 dims = new Vector3(
								threshold,
								float.Parse(parts[2], culture),
								float.Parse(parts[3], culture));
							Vector3 pos = new Vector3(
								float.Parse(parts[4], culture),
								float.Parse(parts[5], culture),
								float.Parse(parts[6], culture));
							Quaternion orient = new Quaternion(
								float.Parse(parts[7], culture),
								float.Parse(parts[8], culture),
								float.Parse(parts[9], culture),
								float.Parse(parts[10], culture));

							CollisionShape shape;
							if (!collisionShapeMgr.TryGetShape(parts[2] + parts[3], out shape)) {
								shape = new BoxShape(dims);
								collisionShapeMgr.RegisterShape(parts[2] + parts[3], shape);
							}

							TriggerRegion tr = new TriggerRegion("AITriggerRegion" + id, pos, orient, shape);

							triggerRegions[id] = tr;
							tempDic[tr] = idTo;
						}
					}
				}
				foreach (var kvp in tempDic) {
					nextTriggerRegions[kvp.Key] = triggerRegions[kvp.Value];
				}

				tempDic.Clear();

				LKernel.GetG<TriggerReporter>().OnTriggerEnter += OnTriggerEnter;
			}
		}

		void OnTriggerEnter(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags, CollisionReportInfo info) {
			TriggerRegion nextRegion;
			if (nextTriggerRegions.TryGetValue(region, out nextRegion)) {
				GoTo(region, nextRegion, otherBody, info);
			}
		}

		void GoTo(TriggerRegion currentRegion, TriggerRegion nextRegion, RigidBody body, CollisionReportInfo info) {
			Kart kart = null;

			if (body.UserObject is CollisionObjectDataHolder) {
				kart = (body.UserObject as CollisionObjectDataHolder).GetThingAsKart();
			}

			if (kart != null && kart.Player.IsComputerControlled) {
				(kart.Player as ComputerPlayer).CalculateNewWaypoint(currentRegion, nextRegion, info);
			}
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			LKernel.GetG<TriggerReporter>().OnTriggerEnter -= OnTriggerEnter;

			triggerRegions.Clear();
			nextTriggerRegions.Clear();
		}
	}
}
