using IrrKlang;
using LuaNetInterface;
using Lymph.Sound;
using Mogre;

namespace Lymph.Lua {
	[LuaPackage("Sounds", "The wrapper for the SoundMain class")]
	public class SoundWrapper {
		public SoundWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("createAmbientSound",
			"Creates a 2D sound i.e. one that has the same volume no matter where the player is. For things like music and stuff.",
			"string filePath - The path to the sound file to play.", "string objectName - The name of the sound", 
			"bool looping - Whether the sound should loop or not.")]
		public static ISound CreateAmbientSound(string filePath, string objectName, bool looping) {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				return s.CreateAmbientSound(filePath, objectName, looping);
			return null;
		}

		[LuaFunction("createObjectSound",
			"Creates a 3D sound i.e. one that changes volume and stuff depending on where the player is. For things like sound effects and stuff.",
			"string filePath - The path to the sound file to play.", "string objectName - The name of the sound",
			"number posX", "number posY", "number posZ", "bool looping - Whether the sound should loop or not.")]
		public static ISound CreateObjectSound(string filePath, string objectName, float posX, float posY, float posZ, bool looping) {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				return s.CreateObjectSound(filePath, new Vector3(posX, posY, posZ), objectName, looping);
			return null;
		}

		[LuaFunction("isCurrentlyPlaying", "Tells whether a given sound is playing or not.", "string soundName - The name of the sound. Not a file path.")]
		public static bool IsCurrentlyPlaying(string soundName) {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				return s.Engine.IsCurrentlyPlaying(soundName);
			return false;
		}

		#region properties
		[LuaFunction("setDefaultMinDistance", "Sets the default minimum distance you have to be from the sound to hear it. I think.",
			"number num - The new minimum distance")]
		public static void SetDefaultMinDistance(float num) {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				s.Engine.Default3DSoundMinDistance = num;
		}

		[LuaFunction("getDefaultMinDistance", "Gets the default minimum distance you have to be from the sound to hear it. I think.")]
		public static float GetDefaultMinDistance() {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				return s.Engine.Default3DSoundMinDistance;
			return -1;
		}

		[LuaFunction("setDefaultManDistance", "Sets the default maximum distance you have to be from the sound to hear it. I think.",
			"number num - The new maximum distance")]
		public static void SetDefaultMaxDistance(float num) {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				s.Engine.Default3DSoundMaxDistance = num;
		}

		[LuaFunction("getDefaultMaxDistance", "Gets the default maximum distance you have to be from the sound to hear it. I think.")]
		public static float GetDefaultMaxDistance() {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				return s.Engine.Default3DSoundMaxDistance;
			return -1;
		}

		[LuaFunction("setRolloffFactor", "Not sure what this does but I think it has something to do with how the sound \"decays\".", "number rolloff")]
		public static void SetRolloffFactor(float rolloff) {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				s.RolloffFactor = rolloff;
		}

		[LuaFunction("getRolloffFactor", "Not sure what this does but I think it has something to do with how the sound \"decays\".")]
		public static float GetRolloffFactor() {
			SoundMain s = LKernel.Get<SoundMain>();
			if (s != null)
				return s.RolloffFactor;
			return -1;
		}

		#endregion
	}
}
