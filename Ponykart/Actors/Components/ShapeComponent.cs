using BulletSharp;
using Mogre;
using Ponykart.IO;

namespace Ponykart.Actors {
	public class ShapeComponent : IThingComponent {
		public int ID { get; protected set; }
		public string Name { get; protected set; }
		public CollisionShape Shape { get; protected set; }
		public Matrix4 Transform { get; protected set; }

		public ShapeComponent(LThing lthing, ThingInstanceTemplate template, ShapeBlock block) {
			ID = IDs.New;
			var sceneMgr = LKernel.Get<SceneManager>();

			Name = block.GetStringProperty("name", template.Name);

			Shape = block.Shape;
			Transform = block.Transform;
		}

		public void Dispose() { }
	}
}
