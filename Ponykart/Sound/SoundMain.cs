using IrrKlang;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Sound {
	public class SoundMain {
		public ISoundEngine Engine { get; private set; }

		public static bool EnableMusic;
		public static bool EnableSounds;

		/// <summary>
		/// The sound manager class.
		/// </summary>
		public SoundMain() {
			Launch.Log("[Loading] Creating IrrKlang and SoundMain...");
			EnableMusic = Options.GetBool("Music");
			EnableSounds = Options.GetBool("Sounds");

			
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);

			LKernel.GetG<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);

			SoundEngineOptionFlag flags = SoundEngineOptionFlag.DefaultOptions | SoundEngineOptionFlag.MuteIfNotFocused;
			Engine = new ISoundEngine(SoundOutputDriver.AutoDetect, flags);

			Launch.Log("[Loading] IrrKlang and SoundMain initialised!");
		}

		/// <summary>
		/// Dispose all of the sound sources
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			Engine.RemoveAllSoundSources();
		}


		private float timesince = 0;
		bool FrameStarted(FrameEvent evt) {
			if (!LKernel.GetG<LevelManager>().IsValidLevel)
				return true;

			if (timesince > 0.1f) {
				timesince = 0;
				// only update this if the level's playable
				if (LKernel.GetG<LevelManager>().IsPlayableLevel) {
					var cam = LKernel.GetG<CameraManager>().CurrentCamera.Camera;
					var player = LKernel.GetG<PlayerManager>().MainPlayer;
					Vector3 pos = cam.RealPosition;
					Vector3 rot = cam.Orientation.ZAxis;
					Vector3 vel = player.Body.LinearVelocity;
					Vector3 up = cam.Orientation.YAxis;

					Engine.SetListenerPosition(
						pos.x, pos.y, pos.z,
						rot.x, rot.y, rot.z,
						vel.x, vel.y, vel.z,
						up.x, up.y, up.z);
				}
				Engine.Update();
			}
			timesince += evt.timeSinceLastFrame;

			return true;
		}

		/// <summary>
		/// Creates an ambient sound. These have no 3D position or effects or anything, so this is ideal for level music and whatnot.
		/// Also hooks in a stop event receiver so the sound disposes of itself when it's finished playing.
		/// </summary>
		/// <param name="filename">The file path of the sound you want to play.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound Play2D(string filename, bool looping, bool startPaused = false, bool sfx = false) {
			return Play2D(GetSource(filename), looping, startPaused, sfx);
		}

		/// <summary>
		/// Creates an ambient sound. These have no 3D position or effects or anything, so this is ideal for level music and whatnot.
		/// Also hooks in a stop event receiver so the sound disposes of itself when it's finished playing.
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound Play2D(ISoundSource source, bool looping, bool startPaused = false, bool sfx = false) {
			if (!EnableMusic)
				return null;
			Launch.Log("[Sounds] Creating 2D sound: " + source.Name + " Looping: " + looping);

			ISound sound = Engine.Play2D(source, looping, startPaused, sfx);
			return sound;
		}

		/// <summary>
		/// Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
		/// Also hooks in a stop event receiver so the sound disposes of itself when it's finished playing.
		/// </summary>
		/// <param name="filename">The file path of the sound you want to play.</param>
		/// <param name="pos">The Position you want this sound to play at.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound Play3D(string filename, Vector3 pos, bool looping, bool startPaused = false, bool sfx = false) {
			return Play3D(GetSource(filename), pos, looping, startPaused, sfx);
		}

		/// <summary>
		/// Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
		/// Also hooks in a stop event receiver so the sound disposes of itself when it's finished playing.
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play.</param>
		/// <param name="pos">The Position you want this sound to play at.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound Play3D(ISoundSource source, Vector3 pos, bool looping, bool startPaused = false, bool sfx = false) {
			if (!EnableSounds || pos == null)
				return null;
			Launch.Log("[Sounds] Creating 3D sound: " + source.Name + " Looping: " + looping);

			ISound sound = Engine.Play3D(source, pos.x, pos.y, pos.z, looping, startPaused, sfx);
			return sound;
		}

		/// <summary>
		/// Gets a sound source. The engine keeps track of all of these.
		/// </summary>
		/// <param name="filename">Don't include the "media/sound/" bit.</param>
		public ISoundSource GetSource(string filename) {
			return Engine.GetSoundSource(Settings.Default.SoundFileLocation + filename, true);
		}


		float rolloff;
		/// <summary>
		/// 0 = no rolloff
		/// 1 = default rolloff
		/// </summary>
		public float RolloffFactor {
			get {
				return rolloff;
			}
			set {
				rolloff = value;
				Engine.SetRolloffFactor(value);
			}
		}
	}
}
