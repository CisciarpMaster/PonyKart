using System;
using BulletSharp;
using Mogre;
using IDisposable = System.IDisposable;

namespace PonykartParsers {
	/// <summary>
	/// Represents a Shape { } block in a .thing file.
	/// </summary>
	public class ShapeBlock : TokenHolder, IDisposable {
		public ThingDefinition Owner { get; protected set; }
		public Matrix4 Transform { get; protected set; }
		public CollisionShape Shape { get; protected set; }

		public ShapeBlock(ThingDefinition owner) {
			Owner = owner;
			SetUpDictionaries();
			IsDisposed = false;
		}

		public override void Finish() {
			ThingEnum shapeType = GetEnumProperty("type", null);

			switch (shapeType) {
				case ThingEnum.Box:
					Shape = new BoxShape(GetVectorProperty("dimensions", null) / 2f);
					break;
				case ThingEnum.Capsule:
					Shape = new CapsuleShape(GetFloatProperty("radius", null), GetFloatProperty("height", null));
					break;
				case ThingEnum.Cone:
					Shape = new ConeShape(GetFloatProperty("radius", null), GetFloatProperty("height", null));
					break;
				case ThingEnum.Cylinder:
					Shape = new CylinderShape(GetVectorProperty("dimensions", null) / 2f);
					break;
				case ThingEnum.Sphere:
					Shape = new SphereShape(GetFloatProperty("radius", null));
					break;
			}

			Quaternion quat = GetQuatProperty("orientation", Quaternion.IDENTITY);
			Vector3 rot;
			if (quat == Quaternion.IDENTITY) {
				rot = GetVectorProperty("rotation", Vector3.ZERO);
				quat = GlobalEulerToQuat(new Degree(rot.x), new Degree(rot.y), new Degree(rot.z));
			}

			Vector3 pos = GetVectorProperty("position", Vector3.ZERO);

			Transform = new Matrix4();
			Transform.MakeTransform(pos, Vector3.UNIT_SCALE, quat);


			// sometimes shapes want to override this and force a compound collision object even if they only have one shape
			if (Owner.GetBoolProperty("forcecompound", false) && Shape != null) {
				CompoundShape comp = new CompoundShape();
				comp.AddChildShape(Transform, Shape);
				Shape = comp;
			}
		}

		#region IDisposable stuff
		public bool IsDisposed { get; protected set; }

		~ShapeBlock() {
			Dispose(false);
		}

		public new void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (Shape != null)
				Shape.Dispose();

			if (disposing)
				base.Dispose();

			IsDisposed = true;
		}
		#endregion

		static Quaternion GlobalEulerToQuat(Radian rotX, Radian rotY, Radian rotZ) {
			Quaternion q1 = new Quaternion(),
					   q2 = new Quaternion(),
					   q3 = new Quaternion(),
					   q = new Quaternion();
			q1.FromAngleAxis(rotX, Vector3.UNIT_X);
			q2.FromAngleAxis(rotY, Vector3.UNIT_Y);
			q3.FromAngleAxis(rotZ, Vector3.UNIT_Z);

			// global axes
			q = q3 * q2 * q1;
			return q;
		}
	}
}
