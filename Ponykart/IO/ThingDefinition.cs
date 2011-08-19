using System.Collections.Generic;
using System.Collections.ObjectModel;
using BulletSharp;
using Mogre;

namespace Ponykart.IO {
	public class ThingDefinition : ThingTokenHolder {
		public string Name { get; protected set; }
		public IDictionary<string, ThingEnum> EnumTokens { get; protected set; }
		public IDictionary<string, string> StringTokens { get; protected set; }
		public IDictionary<string, float> FloatTokens { get; protected set; }
		public IDictionary<string, bool> BoolTokens { get; protected set; }
		public IDictionary<string, Vector3> VectorTokens { get; protected set; }
		public ICollection<ShapeBlock> ShapeBlocks { get; protected set; }
		public ICollection<ModelBlock> ModelBlocks { get; protected set; }

		public ThingDefinition(string name) {
			Name = name;
			EnumTokens = new Dictionary<string, ThingEnum>();
			StringTokens = new Dictionary<string, string>();
			FloatTokens = new Dictionary<string, float>();
			BoolTokens = new Dictionary<string, bool>();
			VectorTokens = new Dictionary<string, Vector3>();
			ShapeBlocks = new Collection<ShapeBlock>();
			ModelBlocks = new Collection<ModelBlock>();
		}

		/// <summary>
		/// Must be called after you're done importing everything into the dictionaries
		/// </summary>
		public void Finish() {
			foreach (ShapeBlock sb in ShapeBlocks) {
				sb.Finish();
			}
			foreach (ModelBlock mb in ModelBlocks) {
				mb.Finish();
			}
		}
	}

	public class ShapeBlock : ThingTokenHolder {
		public static readonly Matrix4 UNCHANGED = new Matrix4(Quaternion.IDENTITY);

		public ThingDefinition Owner { get; protected set; }
		public IDictionary<string, ThingEnum> EnumTokens { get; protected set; }
		public IDictionary<string, string> StringTokens { get; protected set; }
		public IDictionary<string, float> FloatTokens { get; protected set; }
		public IDictionary<string, bool> BoolTokens { get; protected set; }
		public IDictionary<string, Vector3> VectorTokens { get; protected set; }
		public Matrix4 Transform { get; protected set; }
		public CollisionShape Shape { get; protected set; }

		public ShapeBlock(ThingDefinition owner) {
			Owner = owner;
			EnumTokens = new Dictionary<string, ThingEnum>();
			StringTokens = new Dictionary<string, string>();
			FloatTokens = new Dictionary<string, float>();
			BoolTokens = new Dictionary<string, bool>();
			VectorTokens = new Dictionary<string, Vector3>();
		}

		public void Finish() {
			ThingEnum shapeType = EnumTokens["type"];

			switch (shapeType) {
				case ThingEnum.Box:
					Shape = new BoxShape(VectorTokens["dimensions"] / 2f);
					break;
				case ThingEnum.Capsule:
					Shape = new CapsuleShape(FloatTokens["radius"], FloatTokens["height"]);
					break;
				case ThingEnum.CapsuleX:
					Shape = new CapsuleShapeX(FloatTokens["radius"], FloatTokens["height"]);
					break;
				case ThingEnum.CapsuleZ:
					Shape = new CapsuleShapeZ(FloatTokens["radius"], FloatTokens["height"]);
					break;
				case ThingEnum.Cone:
					Shape = new ConeShape(FloatTokens["radius"], FloatTokens["height"]);
					break;
				case ThingEnum.ConeX:
					Shape = new ConeShapeX(FloatTokens["radius"], FloatTokens["height"]);
					break;
				case ThingEnum.ConeZ:
					Shape = new ConeShapeZ(FloatTokens["radius"], FloatTokens["height"]);
					break;
				case ThingEnum.Cylinder:
					Shape = new CylinderShape(VectorTokens["dimensions"] / 2f);
					break;
				case ThingEnum.CylinderX:
					Shape = new CylinderShapeX(VectorTokens["dimensions"] / 2f);
					break;
				case ThingEnum.CylinderZ:
					Shape = new CylinderShapeZ(VectorTokens["dimensions"] / 2f);
					break;
				case ThingEnum.Sphere:
					Shape = new SphereShape(FloatTokens["radius"]);
					break; 
			}

			Vector3 rot;
			if (!VectorTokens.TryGetValue("rotation", out rot))
				rot = Vector3.ZERO;

			Vector3 pos;
			if (!VectorTokens.TryGetValue("position", out pos))
				pos = Vector3.ZERO;

			Transform = new Matrix4(rot.DegreeVectorToGlobalQuaternion());
			Transform.SetTrans(pos);
		}
	}

	public class ModelBlock : ThingTokenHolder {
		public ThingDefinition Owner { get; protected set; }
		public IDictionary<string, ThingEnum> EnumTokens { get; protected set; }
		public IDictionary<string, string> StringTokens { get; protected set; }
		public IDictionary<string, float> FloatTokens { get; protected set; }
		public IDictionary<string, bool> BoolTokens { get; protected set; }
		public IDictionary<string, Vector3> VectorTokens { get; protected set; }

		public ModelBlock(ThingDefinition owner) {
			Owner = owner;
			EnumTokens = new Dictionary<string, ThingEnum>();
			StringTokens = new Dictionary<string, string>();
			FloatTokens = new Dictionary<string, float>();
			BoolTokens = new Dictionary<string, bool>();
			VectorTokens = new Dictionary<string, Vector3>();
		}

		public void Finish() { }
	}

	public interface ThingTokenHolder {
		IDictionary<string, ThingEnum> EnumTokens { get; }
		IDictionary<string, string> StringTokens { get; }
		IDictionary<string, float> FloatTokens { get; }
		IDictionary<string, bool> BoolTokens { get; }
		IDictionary<string, Vector3> VectorTokens { get; }

		void Finish();
	}

	public enum ThingEnum {
		Unassigned = 0,
		Dynamic,
		Kinematic,
		Static,
		None,
		Box,
		/// <summary> a capsule aligned with the Y axis </summary>
		Capsule,
		/// <summary> a capsule aligned with the X axis </summary>
		CapsuleX,
		/// <summary> a capsule aligned with the Z axis </summary>
		CapsuleZ,
		Sphere,
		Cylinder,
		CylinderX,
		CylinderZ,
		Cone,
		ConeX,
		ConeZ
		// ...
	}
}
