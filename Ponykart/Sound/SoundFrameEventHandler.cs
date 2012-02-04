using System.Collections.Generic;
using IrrKlang;
using LuaInterface;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Players;
using Ponykart.UI;

namespace Ponykart.Sound {
	public delegate void SoundFrameEvent(LThing thing, ISound[] sounds);

	/// <summary>
	/// Handles stuff that needs to change sounds of lthings per-frame, such as the pitch of the engine sound based on how fast we're going
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class SoundFrameEventHandler : ILevelHandler {
		IDictionary<LThing, ISound[]> soundsDict;
		IDictionary<LThing, SoundFrameEvent> actionsDict;

		public SoundFrameEventHandler() {
			// first we have to set up something so we can quickly retrieve all sounds associated with a kart
			soundsDict = new Dictionary<LThing, ISound[]>();

			// might as well add all of the karts while we're at it
			foreach (Player player in LKernel.GetG<PlayerManager>().Players) {
				if (player.Kart.SoundComponents.Count > 0) {
					// make an array for all of the sounds
					ISound[] s = new ISound[player.Kart.SoundComponents.Count];
					for (int a = 0; a < s.Length; a++) {
						s[a] = player.Kart.SoundComponents[a].Sound;
					}

					soundsDict.Add(player.Kart, s);
				}
			}

			// and then we'll run these actions every frame
			actionsDict = new Dictionary<LThing, SoundFrameEvent>();

			LKernel.GetG<Root>().FrameEnded += FrameEnded;
		}

		/// <summary>
		/// adds an action to be ran every frame
		/// </summary>
		public void AddAction(LThing lthing, SoundFrameEvent action) {
			actionsDict[lthing] = action;
		}


		bool FrameEnded(FrameEvent evt) {
			if (!Pauser.IsPaused) {

				foreach (var kvp in actionsDict) {
					ISound[] s;
					// if the sound array isn't in the dictionary yet, add it
					if (!soundsDict.TryGetValue(kvp.Key, out s)) {
						s = new ISound[kvp.Key.SoundComponents.Count];
						for (int a = 0; a < s.Length; a++) {
							s[a] = kvp.Key.SoundComponents[a].Sound;
						}
					}

					try {
						kvp.Value.Invoke(kvp.Key, s);
					}
					catch (LuaException ex) {
						Launch.Log("[Lua] *** EXCEPTION *** at " + ex.Source + ": " + ex.Message);
						foreach (var v in ex.Data)
							Launch.Log("[Lua] " + v);
						LKernel.GetG<LuaConsoleManager>().AddLabel("ERROR: " + ex.Message);
						Launch.Log(ex.StackTrace);
					}
				}
			}

			return true;
		}

		public void Detach() {
			LKernel.GetG<Root>().FrameEnded -= FrameEnded;
			soundsDict.Clear();
			actionsDict.Clear();
		}
	}
}
