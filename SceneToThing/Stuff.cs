
namespace SceneToThing {
	/// <summary>
	/// just so we can cast Nodes and Shapes to the same type for the listbox
	/// </summary>
	public abstract class Block {
		public Vector3 Position { get; set; }
		public Quaternion Orientation { get; set; }
		/// <summary>
		/// Scale, for nodes
		/// </summary>
		public Vector3 Dimensions { get; set; }
	}

	/// <summary>
	/// Representation of Mogre.SceneNode
	/// </summary>
	public class Node : Block {
		public string Name { get; set; }
		public Entity Entity { get; set; }

		public override string ToString() {
			return Name + " (" + Entity.Mesh + ")";
		}
	}

	/// <summary>
	/// Representation of Mogre.Entity
	/// </summary>
	public class Entity {
		public Node Owner { get; set; }
		public string Mesh { get; set; }
		public string Material { get; set; }
		public bool CastsShadows { get; set; }
		public string Name { get; set; }
		public bool Visible { get; set; }
		/// <summary>
		/// Not sure what this is for... LOD?
		/// </summary>
		public float RenderingDistance { get; set; }
	}

	/// <summary>
	/// Representation of Mogre.Vector3
	/// </summary>
	public class Vector3 {
		public float x;
		public float y;
		public float z;

		public Vector3(float X, float Y, float Z) {
			this.x = X;
			this.y = Y;
			this.z = Z;
		}

		public override string ToString() {
			return "{" + x + ", " + y + ", " + z + "}";
		}
	}

	/// <summary>
	/// Representation of Mogre.Quaternion
	/// </summary>
	public class Quaternion {
		public float x;
		public float y;
		public float z;
		public float w;

		public Quaternion(float X, float Y, float Z, float W) {
			this.x = X;
			this.y = Y;
			this.z = Z;
			this.w = W;
		}

		public override string ToString() {
			return "{" + x + ", " + y + ", " + z + ", " + w + "}";
		}
	}

	/// <summary>
	/// Representation of BulletSharp.CollisionShape
	/// </summary>
	public class Shape : Block {
		public ShapeTypes Type { get; set; }
		public float Height { get; set; }
		public float Radius { get; set; }
		public string Name { get; set; }

		public override string ToString() {
			/*if (Type == ShapeTypes.Box || Type == ShapeTypes.Cylinder)
				return Type + " " + Dimensions;
			else if (Type == ShapeTypes.Capsule || Type == ShapeTypes.Cone)
				return Type + " {" + Radius + ", " + Height + "}";
			else
				return Type + " {" + Radius + "}";*/
			return Name + " (" + Type + ")";
		}
	}

	/// <summary>
	/// For the different CollisionShape types
	/// 
	/// Casting these to an int gives you the matching index in the type combo box
	/// </summary>
	public enum ShapeTypes {
		Box = 0, 
		Sphere = 1,
		Capsule = 2,
		Cylinder = 3,
		Cone = 4
	}
}
