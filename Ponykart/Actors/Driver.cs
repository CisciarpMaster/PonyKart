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
			this.RootNode.Orientation = kart.RootNode.Orientation;
		}
	}
}
