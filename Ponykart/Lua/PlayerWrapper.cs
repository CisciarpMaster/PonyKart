using System;
using LuaNetInterface;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class PlayerWrapper {

		public PlayerWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		// some helpers to do things to every kart or player

		[LuaFunction("forEachPlayer", "Runs a function for each player.", "function(player, int playerID)")]
		public static void ForEachPlayer(Action<Player, int> action) {
			foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
				action.Invoke(p, p.ID);
			}
		}

		[LuaFunction("forEachKart", "Runs a function for each kart.", "function(kart, int playerID)")]
		public static void ForEachKart(Action<Kart, int> action) {
			foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
				action.Invoke(p.Kart, p.ID);
			}
		}
	}
}
