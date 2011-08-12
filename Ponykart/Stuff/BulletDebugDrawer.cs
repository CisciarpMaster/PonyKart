using System.Collections.ObjectModel;
using BulletSharp;
using Mogre;
using Ponykart.Physics;

namespace Ponykart.Stuff {
	public class BulletDebugDrawer : IDebugDraw, System.IDisposable {
		SceneManager sceneMgr;
		ManualObject lines;
		ManualObject triangles;
		Collection<ContactPoint> contactPoints;
		public DebugDrawModes DebugMode { get; set; }

		bool begin = false;

		public BulletDebugDrawer() {
			sceneMgr = LKernel.Get<SceneManager>();
			contactPoints = new Collection<ContactPoint>();

			lines = new ManualObject("physics lines");
			triangles = new ManualObject("physics triangles");
			lines.Dynamic = true;
			triangles.Dynamic = true;

			sceneMgr.RootSceneNode.AttachObject(lines);
			sceneMgr.RootSceneNode.AttachObject(triangles);

			string matName = "OgreBulletCollisionsDebugDefault";
			MaterialPtr mtl = MaterialManager.Singleton.GetDefaultSettings().Clone(matName);
			mtl.ReceiveShadows = false;
			mtl.SetSceneBlending(SceneBlendType.SBT_TRANSPARENT_ALPHA);
			mtl.SetDepthBias(0.1f, 0);

			TextureUnitState tu = mtl.GetTechnique(0).GetPass(0).CreateTextureUnitState();
			tu.SetColourOperationEx(LayerBlendOperationEx.LBX_SOURCE1, LayerBlendSource.LBS_DIFFUSE);
			mtl.GetTechnique(0).SetLightingEnabled(false);

			lines.Begin(matName, RenderOperation.OperationTypes.OT_LINE_LIST);
			begin = true;
			lines.Position(Vector3.ZERO);
			lines.Colour(ColourValue.Blue);
			lines.Position(Vector3.ZERO);
			lines.Colour(ColourValue.Blue);
			lines.End();

			triangles.Begin(matName, RenderOperation.OperationTypes.OT_TRIANGLE_LIST);
			triangles.Position(Vector3.ZERO);
			triangles.Colour(ColourValue.Blue);
			triangles.Position(Vector3.ZERO);
			triangles.Colour(ColourValue.Blue);
			triangles.Position(Vector3.ZERO);
			triangles.Colour(ColourValue.Blue);
			triangles.End();
			begin = false;

			DebugMode = DebugDrawModes.DrawWireframe;

			LKernel.Get<PhysicsMain>().PreSimulate += PreSimulate;
			LKernel.Get<PhysicsMain>().PostSimulate += PostSimulate;

			LKernel.Get<Physics.PhysicsMain>().World.DebugDrawer = this;
		}

		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (!begin) {
				lines.BeginUpdate(0);
				triangles.BeginUpdate(0);
				begin = true;
			}
		}

		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			foreach (ContactPoint point in contactPoints) {
				lines.Position(point.from);
				lines.Colour(point.colour);
				lines.Position(point.to);
				lines.Colour(point.colour);
			}
			contactPoints.Clear();

			if (begin) {
				lines.End();
				triangles.End();
				begin = false;
			}
		}

		public void Dispose() {
			LKernel.Get<PhysicsMain>().PreSimulate -= PreSimulate;
			LKernel.Get<PhysicsMain>().PostSimulate -= PostSimulate;
			lines.Dispose();
			triangles.Dispose();
		}









		public void Draw3dText(Vector3 location, string textString) { }

		public void DrawAabb(Vector3 from, Vector3 to, ColourValue colour) {
			
		}

		public void DrawArc(Vector3 center, Vector3 normal, Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, ColourValue colour, bool drawSect, float stepDegrees) {
		
		}

		public void DrawArc(Vector3 center, Vector3 normal, Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, ColourValue colour, bool drawSect) {
		
		}

		public void DrawBox(Vector3 bbMin, Vector3 bbMax, Matrix4 trans, ColourValue colour) {
			
		}

		public void DrawBox(Vector3 bbMin, Vector3 bbMax, ColourValue colour) {
			
		}

		public void DrawCapsule(float radius, float halfHeight, int upAxis, Matrix4 transform, ColourValue colour) {
			
		}

		public void DrawCone(float radius, float height, int upAxis, Matrix4 transform, ColourValue colour) {
			
		}

		public void DrawContactPoint(Vector3 pointOnB, Vector3 normalOnB, float distance, int lifeTime, ColourValue colour) {
			ContactPoint p = new ContactPoint();

			p.from = pointOnB;
			p.to = p.from + normalOnB * distance;
			p.dieTime = LKernel.Get<Root>().Timer.Milliseconds + (uint)lifeTime;
			p.colour = colour;

			contactPoints.Add(p);
		}

		public void DrawCylinder(float radius, float halfHeight, int upAxis, Matrix4 transform, ColourValue colour) {
			
		}

		public void DrawLine(Vector3 from, Vector3 to, ColourValue colour) {
			lines.Position(from);
			lines.Colour(colour);
			lines.Position(to);
			lines.Colour(colour);
		}

		public void DrawLine(Vector3 from, Vector3 to, ColourValue fromcolour, ColourValue tocolour) {
			lines.Position(from);
			lines.Colour(fromcolour);
			lines.Position(to);
			lines.Colour(tocolour);
		}

		public void DrawPlane(Vector3 planeNormal, float planeConst, Matrix4 transform, ColourValue colour) {
			
		}

		public void DrawSphere(Vector3 p, float radius, ColourValue colour) {
			
		}

		public void DrawSphere(float radius, Matrix4 transform, ColourValue colour) {
			
		}

		public void DrawSpherePatch(Vector3 center, Vector3 up, Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ColourValue colour, float stepDegrees) {
			
		}

		public void DrawSpherePatch(Vector3 center, Vector3 up, Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ColourValue colour) {
			
		}

		public void DrawTransform(Matrix4 transform, float orthoLen) {
			
		}

		/// <param name="__unnamed004">alpha?</param>
		public void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, ColourValue colour, float __unnamed004) {
			triangles.Position(v0);
			triangles.Colour(colour);
			triangles.Position(v1);
			triangles.Colour(colour);
			triangles.Position(v2);
			triangles.Colour(colour);
		}

		/// <param name="__unnamed003">no idea</param>
		/// <param name="__unnamed004">no idea</param>
		/// <param name="__unnamed005">no idea</param>
		public void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 __unnamed003, Vector3 __unnamed004, Vector3 __unnamed005, ColourValue colour, float alpha) {
			DrawTriangle(v0, v1, v2, colour, alpha);
		}

		public void ReportErrorWarning(string warningString) {
			Launch.Log("[WARNING] (BulletDebugManager): " + warningString);
		}



		struct ContactPoint {
			public Vector3 from;
			public Vector3 to;
			public ColourValue colour;
			public uint dieTime;
		}
	}
}
