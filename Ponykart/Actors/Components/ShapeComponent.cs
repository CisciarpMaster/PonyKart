using BulletSharp;
using PonykartParsers;
using Mogre;

namespace Ponykart.Actors {
	public class ShapeComponent : IThingComponent {
		public int ID { get; protected set; }
		public string Name { get; protected set; }
		public CollisionShape Shape { get; protected set; }
		public Matrix4 Transform { get; protected set; }

		public ShapeComponent(LThing lthing, ThingBlock template, ShapeBlock block) {
			ID = IDs.New;
			var sceneMgr = LKernel.GetG<SceneManager>();

			Name = block.GetStringProperty("name", template.ThingName);

			Shape = block.Shape;
			Transform = block.Transform;
		}

		public void Dispose() { }
	}
}
