﻿using System;
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
		private IList<ISound> musics;
		private IList<ISound> sounds;

		private bool enableMusic;
		private bool enableSounds;

		/// <summary>
		/// The sound manager class.
		/// </summary>
		public SoundMain() {
			Launch.Log("[Loading] Creating IrrKlang and SoundMain...");
			musics = new List<ISound>();
			sounds = new List<ISound>();

			enableMusic = Options.GetBool("Music");
			enableSounds = Options.GetBool("Sounds");

			
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
			LKernel.GetG<Pauser>().PauseEvent += new PauseEvent(PauseEvent);
			LKernel.GetG<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);

			SoundEngineOptionFlag flags = SoundEngineOptionFlag.DefaultOptions | SoundEngineOptionFlag.MuteIfNotFocused | SoundEngineOptionFlag.MultiThreaded;
			Engine = new ISoundEngine(SoundOutputDriver.AutoDetect, flags);
			Engine.Default3DSoundMinDistance = 50;

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
			musics.Clear();
			sounds.Clear();
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


		public void EnableMusic() {
			foreach (ISound sound in musics) {
				sound.Volume += 1;
				sound.Paused = false;
			}
			enableMusic = true;
		}

		public void DisableMusic() {
			foreach (ISound sound in musics) {
				sound.Volume -= 1;
				sound.Paused = true;
			}
			enableMusic = false;
		}

		public void EnableSounds() {
			foreach (ISound sound in sounds) {
				sound.Volume += 1;
				sound.Paused = false;
			}
			enableSounds = true;
		}

		public void DisableSounds() {
			foreach (ISound sound in sounds) {
				sound.Volume -= 1;
				sound.Paused = true;
			}
			enableSounds = false;
		}

		/// <summary>
		/// Creates a 2D music sound. This has no position and is controlled by a different option than the Play2D and Play3D methods.
		/// </summary>
		/// <param name="filename">The file path of the sound you want to play</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound PlayMusic(string filename, bool startPaused = false) {
			return PlayMusic(GetSource(filename), startPaused);
		}

		/// <summary>
		/// Creates a 2D music sound. This has no position and is controlled by a different option than the Play2D and Play3D methods.
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound PlayMusic(ISoundSource source, bool startPaused = false) {
			Launch.Log("[Sounds] Creating music: " + source.Name);

			ISound music = Engine.Play2D(source, true, startPaused, false);
			musics.Add(music);

			if (!enableMusic) {
				music.Paused = true;
				music.Volume = 0;
			}
			else if (startPaused)
				music.Paused = true;

			return music;
		}

		/// <summary>
		/// Creates an ambient sound. These have no 3D position or effects or anything, so this is ideal for level ambients and whatnot.
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
		/// Creates an ambient sound. These have no 3D position or effects or anything, so this is ideal for level ambients and whatnot.
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created</returns>
		public ISound Play2D(ISoundSource source, bool looping, bool startPaused = false, bool sfx = false) {
			Launch.Log("[Sounds] Creating 2D sound: " + source.Name + " Looping: " + looping);

			ISound sound = Engine.Play2D(source, looping, startPaused, sfx);
			sounds.Add(sound);

			if (!enableSounds) {
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
			sounds.Add(sound);

			if (!enableSounds) {
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

		public bool IsMusicEnabled {
			get {
				return enableMusic;
			}
		}

		public bool IsSoundEnabled {
			get {
				return enableSounds;
			}
		}
	}
}
