using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mogre;

namespace Ponykart.IO {
	public class ThingDefinition : ThingTokenHolder {
		public string Name { get; protected set; }
		public int ID { get; protected set; }
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
			ID = IDs.New;
		}
	}

	public class ShapeBlock : ThingTokenHolder {
		public ThingDefinition Owner { get; protected set; }
		public IDictionary<string, ThingEnum> EnumTokens { get; protected set; }
		public IDictionary<string, string> StringTokens { get; protected set; }
		public IDictionary<string, float> FloatTokens { get; protected set; }
		public IDictionary<string, bool> BoolTokens { get; protected set; }
		public IDictionary<string, Vector3> VectorTokens { get; protected set; }

		public ShapeBlock(ThingDefinition owner) {
			Owner = owner;
			EnumTokens = new Dictionary<string, ThingEnum>();
			StringTokens = new Dictionary<string, string>();
			FloatTokens = new Dictionary<string, float>();
			BoolTokens = new Dictionary<string, bool>();
			VectorTokens = new Dictionary<string, Vector3>();
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
	}

	public interface ThingTokenHolder {
		IDictionary<string, ThingEnum> EnumTokens { get; }
		IDictionary<string, string> StringTokens { get; }
		IDictionary<string, float> FloatTokens { get; }
		IDictionary<string, bool> BoolTokens { get; }
		IDictionary<string, Vector3> VectorTokens { get; }
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
		CylinderZ
		// ...
	}
}
