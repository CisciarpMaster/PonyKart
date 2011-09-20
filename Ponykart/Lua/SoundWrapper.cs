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

		[LuaFunction("createAmbientSound",
			"Creates a 2D sound i.e. one that has the same volume no matter where the player is. For things like music and stuff.",
			"string filePath - The path to the sound file to play.", 
			"bool looping - Whether the sound should loop or not.")]
		public static ISound CreateAmbientSound(string filePath, bool looping) {
			return LKernel.GetG<SoundMain>().Play2D(filePath, looping);
		}

		[LuaFunction("createObjectSound",
			"Creates a 3D sound i.e. one that changes volume and stuff depending on where the player is. For things like sound effects and stuff.",
			"string filePath - The path to the sound file to play.",
			"Vector3 pos", "bool looping - Whether the sound should loop or not.")]
		public static ISound CreateObjectSound(string filePath, Vector3 pos, bool looping) {
			return LKernel.GetG<SoundMain>().Play3D(filePath, pos, looping);
		}

		[LuaFunction("isCurrentlyPlaying", "Tells whether a given sound is playing or not.", "string soundName - The name of the sound. Not a file path.")]
		public static bool IsCurrentlyPlaying(string soundName) {
			return LKernel.GetG<SoundMain>().Engine.IsCurrentlyPlaying(soundName);
		}

		[LuaFunction("setDefaultMinDistance", "Sets the default minimum distance you have to be from the sound to hear it. I think.",
			"number num - The new minimum distance")]
		public static void SetDefaultMinDistance(float num) {
			LKernel.GetG<SoundMain>().Engine.Default3DSoundMinDistance = num;
		}

		[LuaFunction("getDefaultMinDistance", "Gets the default minimum distance you have to be from the sound to hear it. I think.")]
		public static float GetDefaultMinDistance() {
			return LKernel.GetG<SoundMain>().Engine.Default3DSoundMinDistance;
		}

		[LuaFunction("setDefaultManDistance", "Sets the default maximum distance you have to be from the sound to hear it. I think.",
			"number num - The new maximum distance")]
		public static void SetDefaultMaxDistance(float num) {
			LKernel.GetG<SoundMain>().Engine.Default3DSoundMaxDistance = num;
		}

		[LuaFunction("getDefaultMaxDistance", "Gets the default maximum distance you have to be from the sound to hear it. I think.")]
		public static float GetDefaultMaxDistance() {
			return LKernel.GetG<SoundMain>().Engine.Default3DSoundMaxDistance;
		}

		[LuaFunction("setRolloffFactor", "Not sure what this does but I think it has something to do with how the sound \"decays\".", "number rolloff")]
		public static void SetRolloffFactor(float rolloff) {
			LKernel.GetG<SoundMain>().RolloffFactor = rolloff;
		}

		[LuaFunction("getRolloffFactor", "Not sure what this does but I think it has something to do with how the sound \"decays\".")]
		public static float GetRolloffFactor() {
			return LKernel.GetG<SoundMain>().RolloffFactor;
		}
	}
}
