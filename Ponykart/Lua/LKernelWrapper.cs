using LuaNetInterface;
using Ponykart.Actors;
using Ponykart.Handlers;
using Ponykart.Levels;
using Ponykart.Players;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class LKernelWrapper {

		public LKernelWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("playerKart", "Returns the player's kart")]
		public static Kart GetPlayerKart() {
			if (LKernel.GetG<LevelManager>().IsPlayableLevel) {
				return LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			}
			else
				return null;
		}

		[LuaFunction("getKartHandler", "Gets the KartHandler")]
		public static KartHandler GetKartHandler() {
			return LKernel.GetG<KartHandler>();
		}
	}
}
