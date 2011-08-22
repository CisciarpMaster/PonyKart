using LuaNetInterface;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class LKernelWrapper {

		public LKernelWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("playerKart", "Returns the player's kart")]
		public static Kart GetPlayerKart() {
			var playermanager = LKernel.Get<PlayerManager>();
			if (playermanager != null) {
				return playermanager.MainPlayer.Kart;
			}
			else return null;
		}
	}
}
