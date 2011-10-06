using IrrKlang;
using LuaNetInterface;
using Mogre;
using Ponykart.Sound;

namespace Ponykart.Lua {
	//[LuaPackage("Sounds", "The wrapper for the SoundMain class")]
	[LuaPackage(null, null)]
	public class SoundWrapper {
		public SoundWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("create2DSound",
			"Creates a 2D sound i.e. one that has the same volume no matter where the player is. For things like music and stuff.",
			"string filePath - The path to the sound file to play.", 
			"bool looping - Whether the sound should loop or not.")]
		public static ISound Create2DSound(string filePath, bool looping) {
			return LKernel.GetG<SoundMain>().Play2D(filePath, looping);
		}

		[LuaFunction("create3DSound",
			"Creates a 3D sound i.e. one that changes volume and stuff depending on where the player is. For things like sound effects and stuff.",
			"string filePath - The path to the sound file to play.",
			"Vector3 pos", "bool looping - Whether the sound should loop or not.")]
		public static ISound Create3DSound(string filePath, Vector3 pos, bool looping) {
			return LKernel.GetG<SoundMain>().Play3D(filePath, pos, looping);
		}

		[LuaFunction("isCurrentlyPlaying", "Tells whether a given sound is playing or not.", "string soundName - The name of the sound. Not a file path.")]
		public static bool IsCurrentlyPlaying(string soundName) {
			return LKernel.GetG<SoundMain>().Engine.IsCurrentlyPlaying(soundName);
		}
	}
}
