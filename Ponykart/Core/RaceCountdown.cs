﻿//#define ENABLE_COUNTDOWN
using System;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.Core {
	/// <summary>
	/// A delegate for each of the countdown states
	/// </summary>
	public delegate void RaceCountEvent();

	public class RaceCountdown {
		float elapsed;
		bool preCount, three, two, one, go, oneSecondAfterGo;
		const float INITIAL_DELAY = 1;

		/// <summary>
		/// our events
		/// </summary>
		public event RaceCountEvent OnThree, OnTwo, OnOne, OnGo, OnOneSecondAfterGo;

		/// <summary>
		/// Hook up to the level un/load events
		/// </summary>
		public RaceCountdown() {
			LKernel.GetG<LevelManager>().OnLevelPostLoad += new LevelEvent(OnLevelPostLoad);
			LKernel.GetG<LevelManager>().OnLevelUnload += (ea) => Detach();
		}

		/// <summary>
		/// Reset the elapsed time, reset the bools, disable control of all of the players, and hook up to the frame started event.
		/// </summary>
		void OnLevelPostLoad(LevelChangedEventArgs eventArgs) {
			// only run this on race levels!
			if (eventArgs.NewLevel.Type == LevelType.Race) {
				preCount = three = two = one = go = oneSecondAfterGo = false;
				elapsed = 0;

#if ENABLE_COUNTDOWN
				foreach (var player in LKernel.Get<PlayerManager>().Players) {
					// first make sure all of the karts can't be controlled
					player.IsControlEnabled = false;
				}
#endif

				LKernel.Get<Root>().FrameStarted += FrameStarted;
			}
		}

		/// <summary>
		/// Count down!
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			if (!preCount && elapsed >= INITIAL_DELAY) {
				elapsed = 0;
				preCount = true;
			}
			else if (!three && elapsed >= INITIAL_DELAY) {
				Console.WriteLine("3");
				Invoke(OnThree);
				three = true;
				elapsed = INITIAL_DELAY;
			}
			else if (!two && elapsed >= INITIAL_DELAY + 1) {
				Console.WriteLine("2");
				Invoke(OnTwo);
				two = true;
				elapsed = INITIAL_DELAY + 1;
			}
			else if (!one && elapsed >= INITIAL_DELAY + 2) {
				Console.WriteLine("1");
				Invoke(OnOne);
				one = true;
				elapsed = INITIAL_DELAY + 2;
			}
			else if (!go && elapsed >= INITIAL_DELAY + 3) {
#if ENABLE_COUNTDOWN
				foreach (var player in LKernel.Get<PlayerManager>().Players) {
					// first make sure all of the karts can't be controlled
					player.IsControlEnabled = true;
				}
#endif

				Console.WriteLine("Go!");
				Invoke(OnGo);
				go = true;
				elapsed = INITIAL_DELAY + 3;
			}
			else if (!oneSecondAfterGo && elapsed >= INITIAL_DELAY + 4) {
				Console.WriteLine("One second after go");
				Invoke(OnOneSecondAfterGo);
				oneSecondAfterGo = true;

				// don't need to keep checking the time any more
				Detach();
			}

			elapsed += evt.timeSinceLastFrame;
			return true;
		}

		/// <summary>
		/// helper method
		/// </summary>
		void Invoke(RaceCountEvent evt) {
			if (evt != null)
				evt();
		}

		/// <summary>
		/// Unhook from the frame started event
		/// </summary>
		void Detach() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
		}
	}
}