using System.Collections.Generic;
using System.Linq;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.Core {
	/// <summary>
	/// Instead of each ModelComponent hooking up to FrameStarted to update any animation it might have, we just give it 
	/// to this class to update it for us. This means we only have to hook one FrameStarted method up instead of loads of them.
	/// </summary>
	public class AnimationManager {
		private IList<AnimationBlender> blenders;
		private IDictionary<string, Entity> skeletonEntities;

		public AnimationManager() {
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);

			blenders = new List<AnimationBlender>();
			skeletonEntities = new Dictionary<string, Entity>();
		}

		/// <summary>
		/// hook up to the frame started event
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		/// <summary>
		/// clear our states list and disconnect from the frame started event
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			blenders.Clear();
			skeletonEntities.Clear();
			LKernel.GetG<Root>().FrameStarted -= FrameStarted;
		}

		/// <summary>
		/// update all of our animations, but only if we aren't paused
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			if (!Pauser.IsPaused) {
				foreach (AnimationBlender b in blenders) {
					b.AddTime(evt.timeSinceLastFrame);
				}
			}
			return true;
		}

		/// <summary>
		/// Add an animation to be automatically updated
		/// </summary>
		public void Add(AnimationBlender ab) {
			blenders.Add(ab);
		}

		/// <summary>
		/// Remove an animation from being automatically updated
		/// </summary>
		public void Remove(AnimationBlender ab) {
			blenders.Remove(ab);
		}
	}
}
