using Mogre;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// For lyra's sitting model. Mostly the same as the BackgroundPony class, but more watered down
	/// </summary>
	public class Lyra : BackgroundPony {

		public Lyra(ThingBlock block, ThingDefinition def) : base(block, def) {
			// can't stop the constructor from doing stuff
			AnimPose = Pose.Sitting;
			PonyType = Type.Unicorn;
		}

		protected override void AnimateBodyManeAndTail(string animationName, AnimationBlendingTransition transition, float duration, bool looping) {
			bodyComponent.AnimationBlender.Blend(animationName, transition, duration, looping);
			// mane has no animations
			// tail doesn't change
		}

		public override void AddTimeToBodyManeAndTail() {
			float rand = (float) random.NextDouble();
			bodyComponent.AnimationBlender.AddTime(rand);
			tailComponent.AnimationState.AddTime(rand);
		}

		// no wings
		public override void ChangeWingAnimation(string animationName) { }

		// no standing
		public override void Stand() { }

		// no flying
		public override void Fly() { }

		// sitting only
		public override void Sit() {
			string anim = "Sit" + random.Next(1, 3);
			bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);
			
			AnimPose = Pose.Sitting;
		}

		// final destination
		public override void Cheer() { }
		public override void StopCheering() { }

		public override void PlayNext() {
			Sit();
		}
	}
}
