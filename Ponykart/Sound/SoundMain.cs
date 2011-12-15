using System;
using System.Collections.Generic;
using IrrKlang;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Sound {
	public class SoundMain {
		public ISoundEngine Engine { get; private set; }
		private IList<ISound> sounds2D;
		private IList<ISound> sounds3D;

		private bool enable2D;
		private bool enable3D;

		/// <summary>
		/// The sound manager class.
		/// </summary>
		public SoundMain() {
			Launch.Log("[Loading] Creating IrrKlang and SoundMain...");
			sounds2D = new List<ISound>();
			sounds3D = new List<ISound>();

			enable2D = Options.GetBool("Music");
			enable3D = Options.GetBool("Sounds");

			
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
			LKernel.GetG<Pauser>().PauseEvent += new PauseEvent(PauseEvent);
			LKernel.GetG<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);

			SoundEngineOptionFlag flags = SoundEngineOptionFlag.DefaultOptions | SoundEngineOptionFlag.MuteIfNotFocused | SoundEngineOptionFlag.MultiThreaded;
			Engine = new ISoundEngine(SoundOutputDriver.AutoDetect, flags);
			Engine.Default3DSoundMinDistance = 15;

			Launch.Log("[Loading] IrrKlang and SoundMain initialised!");
		}

		void PauseEvent(PausingState state) {
			Engine.SetAllSoundsPaused(state == PausingState.Pausing);
		}

		/// <summary>
		/// Dispose all of the sound sources
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			Engine.RemoveAllSoundSources();
			Engine.SetListenerPosition(0, 0, 0, 0, 0, -1);
			sounds2D.Clear();
			sounds3D.Clear();
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
					Vector3 pos = cam.DerivedPosition;
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


		public void Enable2DSounds() {
			foreach (ISound sound in sounds2D) {
				sound.Volume += 1;
				sound.Paused = false;
			}
			enable2D = true;
		}

		public void Disable2DSounds() {
			foreach (ISound sound in sounds2D) {
				sound.Volume -= 1;
				sound.Paused = true;
			}
			enable2D = false;
		}

		public void Enable3DSounds() {
			foreach (ISound sound in sounds3D) {
				sound.Volume += 1;
				sound.Paused = false;
			}
			enable3D = true;
		}

		public void Disable3DSounds() {
			foreach (ISound sound in sounds3D) {
				sound.Volume -= 1;
				sound.Paused = true;
			}
			enable3D = false;
		}

		/// <summary>
		/// Creates an ambient sound. These have no 3D position or effects or anything, so this is ideal for level music and whatnot.
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
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound Play2D(ISoundSource source, bool looping, bool startPaused = false, bool sfx = false) {
			Launch.Log("[Sounds] Creating 2D sound: " + source.Name + " Looping: " + looping);

			ISound sound = Engine.Play2D(source, looping, startPaused, sfx);
			sounds2D.Add(sound);

			if (!enable2D) {
				sound.Paused = true;
				sound.Volume = 0;
			}
			else if (startPaused)
				sound.Paused = true;

			return sound;
		}

		/// <summary>
		/// Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
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
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play.</param>
		/// <param name="pos">The Position you want this sound to play at.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound Play3D(ISoundSource source, Vector3 pos, bool looping, bool startPaused = false, bool sfx = false) {
			if (pos == null)
				throw new ArgumentException("Position cannot be null!", "pos");

			Launch.Log("[Sounds] Creating 3D sound: " + source.Name + " Looping: " + looping);

			ISound sound = Engine.Play3D(source, pos.x, pos.y, pos.z, looping, startPaused, sfx);
			sounds3D.Add(sound);

			if (!enable3D) {
				sound.Paused = true;
				sound.Volume = 0;
			}
			else if (startPaused)
				sound.Paused = true;

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

		public bool Is2DSoundEnabled {
			get {
				return enable2D;
			}
		}

		public bool Is3DSoundEnabled {
			get {
				return enable3D;
			}
		}
	}
}
