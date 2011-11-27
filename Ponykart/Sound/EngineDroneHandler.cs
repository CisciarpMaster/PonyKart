using System.Collections.Generic;
using IrrKlang;
using Mogre;
using Ponykart.Actors;
using Ponykart.Players;
using Ponykart.Core;

namespace Ponykart.Sound {
	/// <summary>
	/// Handles the pitch of the engine sound based on how fast we're going
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class EngineDroneHandler : ILevelHandler {
		IList<Pair<Kart, ISound>> list;

		public EngineDroneHandler() {
			list = new List<Pair<Kart, ISound>>();

			foreach (Player player in LKernel.GetG<PlayerManager>().Players) {
				list.Add(new Pair<Kart, ISound>(player.Kart, player.Kart.SoundComponents[0].Sound));
			}

			LKernel.GetG<Root>().FrameEnded += FrameEnded;
		}


		bool FrameEnded(FrameEvent evt) {
			if (!Pauser.IsPaused) {

				foreach (var pair in list) {
					pair.second.PlaybackSpeed = (pair.first.VehicleSpeed * 0.015f) + 1;
				}
			}

			return true;
		}

		public void Detach() {
			LKernel.GetG<Root>().FrameEnded -= FrameEnded;
		}
	}
}
