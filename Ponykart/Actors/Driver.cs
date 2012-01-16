using Mogre;
using PonykartParsers;

namespace Ponykart.Actors {
	public class Driver : LThing {

		public Kart Kart { get; set; }


		public Driver(ThingBlock block, ThingDefinition def)
			: base(block, def) {

		}

		public void AttachToKart(Kart kart, Vector3 offset) {
			this.RootNode.Parent.RemoveChild(this.RootNode);
			kart.RootNode.AddChild(this.RootNode);

			this.RootNode.Position = offset;
			this.RootNode.Orientation = Quaternion.IDENTITY;
			this.RootNode.SetInitialState();
		}

		public void ChangeAnimation(string animationName, AnimationBlender.BlendingTransition transition, float duration = 0.2f) {
			ModelComponents[0].Animation.Blend(animationName, transition, duration, true);
		}
	}
}
