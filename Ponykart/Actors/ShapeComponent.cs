using BulletSharp;
using Mogre;
using Ponykart.IO;

namespace Ponykart.Actors {
	public class ShapeComponent : IThingComponent {
		public int ID { get; protected set; }
		public string Name { get; protected set; }
		public Matrix4 Transform { get; protected set; }
		public static readonly Matrix4 UNCHANGED = new Matrix4(Quaternion.IDENTITY);
		/// <summary>
		/// TODO move this out into a dictionary or something so we can re-use it
		/// </summary>
		public CollisionShape Shape { get; protected set; }

		public ShapeComponent(LThing lthing, ThingInstanceTemplate template, ShapeBlock block) {
			ID = IDs.New;
			var sceneMgr = LKernel.Get<SceneManager>();

			string name;
			if (block.StringTokens.TryGetValue("name", out name))
				Name = name;
			else
				Name = template.Name;

			ThingEnum shapeType = block.EnumTokens["type"];

			switch (shapeType) {
				case ThingEnum.Box:
					Shape = new BoxShape(block.VectorTokens["dimensions"]);
					break;
				case ThingEnum.Capsule:
					Shape = new CapsuleShape(block.FloatTokens["radius"], block.FloatTokens["height"]);
					break;
				case ThingEnum.CapsuleX:
					Shape = new CapsuleShapeX(block.FloatTokens["radius"], block.FloatTokens["height"]);
					break;
				case ThingEnum.CapsuleZ:
					Shape = new CapsuleShapeZ(block.FloatTokens["radius"], block.FloatTokens["height"]);
					break;
				case ThingEnum.Cylinder:
					Shape = new CylinderShape(block.VectorTokens["dimensions"]);
					break;
				case ThingEnum.CylinderX:
					Shape = new CylinderShapeX(block.VectorTokens["dimensions"]);
					break;
				case ThingEnum.CylinderZ:
					Shape = new CylinderShapeZ(block.VectorTokens["dimensions"]);
					break;
				case ThingEnum.Sphere:
					Shape = new SphereShape(block.FloatTokens["radius"]);
					break;
			}

			Vector3 rot;
			if (!block.VectorTokens.TryGetValue("rotation", out rot))
				rot = Vector3.ZERO;

			Vector3 pos;
			if (!block.VectorTokens.TryGetValue("position", out pos))
				pos = Vector3.ZERO;

			Transform = new Matrix4(rot.DegreeVectorToGlobalQuaternion());
			Transform.SetTrans(pos);
		}

		public void Dispose() {
			Shape.Dispose();
		}
	}
}
