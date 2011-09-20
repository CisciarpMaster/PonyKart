using System.Collections.Generic;
using IrrKlang;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Properties;

/* http://www.ogre3d.org/tikiwiki/MogreFreeSL
 * 
 * PROTIPS: 
 * 
 * - The Reference Distance is the distance away from the camera at which the
 *   SoundObject's volume starts decreasing, and the Maximum Distance is the distance
 *   from the camera at which the SoundObject will be at its quietest.
 */
namespace Ponykart.Sound {
	public class SoundMain {
		bool quit = false;
		public ISoundEngine Engine { get; private set; }
		/// <summary>
		/// The key is the filename
		/// </summary>
		public IDictionary<string, ISoundSource> Sources { get; private set; }

		/// <summary>
		/// The sound manager class.
		/// </summary>
		public SoundMain() {
			Launch.Log("[Loading] Creating IrrKlang and SoundMain...");
			Sources = new Dictionary<string, ISoundSource>();

			var levelManager = LKernel.GetG<LevelManager>();
			levelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			levelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);

			LKernel.GetG<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);

			LKernel.GetG<RaceCountdown>().OnGo += new RaceCountEvent(OnGo);

			SoundEngineOptionFlag flags = SoundEngineOptionFlag.DefaultOptions | SoundEngineOptionFlag.MuteIfNotFocused;
			Engine = new ISoundEngine(SoundOutputDriver.AutoDetect, flags);

			Launch.Log("[Loading] IrrKlang and SoundMain initialised!");
		}

		/// <summary>
		/// Runs whenever a new level is loaded. We create the background music now so it loads, then pause it until we need to play it.
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			// only want to load this on nonempty levels
			if (eventArgs.NewLevel.Type != LevelType.EmptyLevel) {
				// get the property from the .muffin file, if it has one
				string musicFile = eventArgs.NewLevel.Definition.GetStringProperty("Music", string.Empty);
				if (musicFile != string.Empty) {
					Sources["music"] = Engine.AddSoundSourceFromFile(Settings.Default.SoundFileLocation + musicFile, StreamMode.AutoDetect, true);
					
					// if it's a race level, pause the music until we need it
					if (eventArgs.NewLevel.Type != LevelType.Race)
						Engine.Play2D(Sources["music"], true, false, false);
				}
			}
		}

		/// <summary>
		/// Dispose all of the sound sources
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			Engine.RemoveAllSoundSources();
			Sources.Clear();
		}

		/// <summary>
		/// Start the background music!
		/// </summary>
		void OnGo() {
			if (Sources.ContainsKey("music"))
				Engine.Play2D(Sources["music"], true, false, false);
		}


		private float timesince = 0;
		bool FrameStarted(FrameEvent evt) {
			if (!LKernel.GetG<LevelManager>().IsValidLevel)
				return true;

			if (timesince > 0.1f) {
				timesince = 0;
				// only update this if the level's playable
				if (LKernel.GetG<LevelManager>().IsPlayableLevel) {
					var player = LKernel.GetG<PlayerManager>().MainPlayer;
					Vector3 pos = player.NodePosition;
					Vector3 rot = player.Orientation.ZAxis;
					Vector3 vel = player.Body.LinearVelocity;
					Vector3 up = player.Orientation.YAxis;

					Engine.SetListenerPosition(
						pos.x, pos.y, pos.z,
						rot.x, rot.y, rot.z,
						vel.x, vel.y, vel.z,
						up.x, up.y, up.z);
				}
				Engine.Update();
			}
			timesince += evt.timeSinceLastFrame;

			return !quit;
		}

		/// <summary>
		/// Creates an ambient sound. These have no 3D position or effects or anything, so this is ideal for level music and whatnot.
		/// Also hooks in a stop event receiver so the sound disposes of itself when it's finished playing.
		/// </summary>
		/// <param name="filename">The file path of the sound you want to play.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <returns>The ISound you just created</returns>
		public ISound CreateAmbientSound(string filename, bool looping) {
			if (!Settings.Default.EnableMusic)
				return null;
			Launch.Log("[Sounds] Creating ambient sound: " + filename + " Looping: " + looping);

			ISound sound = Engine.Play2D(GetSource(filename), looping, false, false);
			return sound;
		}

		/// <summary>
		/// Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
		/// Also hooks in a stop event receiver so the sound disposes of itself when it's finished playing.
		/// </summary>
		/// <param name="filename">The file path of the sound you want to play.</param>
		/// <param name="pos">The Position you want this sound to play at.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <returns>The ISound you just created</returns>
		// TODO: update the position of these sounds every frame - should that maybe go in MogreMotionState?
		public ISound CreateObjectSound(string filename, Vector3 pos, bool looping) {
			if (!Settings.Default.EnableSounds || pos == null)
				return null;
			Launch.Log("[Sounds] Creating object sound: " + filename + " Looping: " + looping);

			ISound sound = Engine.Play3D(GetSource(filename), pos.x, pos.y, pos.z, looping, false, false);
			return sound;
		}

		/// <summary>
		/// Gets a sound source. If we have it already, then we just get the one from the dictionary, otherwise we load it.
		/// </summary>
		ISoundSource GetSource(string filename) {
			ISoundSource source;
			if (!Sources.TryGetValue(filename, out source)) {
				source = Engine.AddSoundSourceFromFile(Settings.Default.SoundFileLocation + filename, StreamMode.AutoDetect, true);
				Sources[filename] = source;
			}
			return source;
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
