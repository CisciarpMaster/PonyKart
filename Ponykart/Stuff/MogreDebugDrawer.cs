#if DEBUG
/// http://www.ogre3d.org/addonforums/viewtopic.php?f=8&p=81515#p81515
using System.Collections.Generic;
using Mogre;

public class IcoSphere {
	public class TriangleIndices {
		public int v1;
		public int v2;

		public int v3;
		public TriangleIndices(int _v1, int _v2, int _v3) {
			v1 = _v1;
			v2 = _v2;
			v3 = _v3;
		}


		public static bool operator <(TriangleIndices ImpliedObject, TriangleIndices o) {
			return ImpliedObject.v1 > o.v1 && ImpliedObject.v2 < o.v2 && ImpliedObject.v3 < o.v3;
		}
		public static bool operator >(TriangleIndices ImpliedObject, TriangleIndices o) {
			return ImpliedObject.v1 > o.v1 && ImpliedObject.v2 > o.v2 && ImpliedObject.v3 > o.v3;
		}
	}

	public class LineIndices {
		public int v1;

		public int v2;
		public LineIndices(int _v1, int _v2) {
			v1 = _v1;
			v2 = _v2;
		}

		public static bool operator ==(LineIndices ImpliedObject, LineIndices o) {
			return (ImpliedObject.v1 == o.v1 && ImpliedObject.v2 == o.v2) || (ImpliedObject.v1 == o.v2 && ImpliedObject.v2 == o.v1);
		}
		public static bool operator !=(LineIndices ImpliedObject, LineIndices o) {
			return (ImpliedObject.v1 != o.v1 && ImpliedObject.v2 != o.v2) || (ImpliedObject.v1 != o.v2 && ImpliedObject.v2 != o.v1);
		}

		// to stop the warnings
		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object obj) {
			return this == obj as LineIndices;
		}
	}
	private List<Vector3> vertices = new List<Vector3>();
	private LinkedList<LineIndices> _lineIndices = new LinkedList<LineIndices>();
	private LinkedList<int> _triangleIndices = new LinkedList<int>();
	private LinkedList<TriangleIndices> faces = new LinkedList<TriangleIndices>();
	private Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();

	private int index;
	public IcoSphere() {
		index = 0;
	}
	public void Dispose() {
	}
	public void create(int recursionLevel) {
		vertices.Clear();
		_lineIndices.Clear();
		_triangleIndices.Clear();
		faces.Clear();
		middlePointIndexCache.Clear();
		index = 0;

		float t = (1f + Math.Sqrt(5f)) / 2f;

		AddVertex(new Vector3(-1f, t, 0f));
		AddVertex(new Vector3(1f, t, 0f));
		AddVertex(new Vector3(-1f, -t, 0f));
		AddVertex(new Vector3(1f, -t, 0f));

		AddVertex(new Vector3(0f, -1f, t));
		AddVertex(new Vector3(0f, 1f, t));
		AddVertex(new Vector3(0f, -1f, -t));
		AddVertex(new Vector3(0f, 1f, -t));

		AddVertex(new Vector3(t, 0f, -1f));
		AddVertex(new Vector3(t, 0f, 1f));
		AddVertex(new Vector3(-t, 0f, -1f));
		AddVertex(new Vector3(-t, 0f, 1f));

		AddFace(0, 11, 5);
		AddFace(0, 5, 1);
		AddFace(0, 1, 7);
		AddFace(0, 7, 10);
		AddFace(0, 10, 11);

		AddFace(1, 5, 9);
		AddFace(5, 11, 4);
		AddFace(11, 10, 2);
		AddFace(10, 7, 6);
		AddFace(7, 1, 8);

		AddFace(3, 9, 4);
		AddFace(3, 4, 2);
		AddFace(3, 2, 6);
		AddFace(3, 6, 8);
		AddFace(3, 8, 9);

		AddFace(4, 9, 5);
		AddFace(2, 4, 11);
		AddFace(6, 2, 10);
		AddFace(8, 6, 7);
		AddFace(9, 8, 1);

		AddLineIndices(1, 0);
		AddLineIndices(1, 5);
		AddLineIndices(1, 7);
		AddLineIndices(1, 8);
		AddLineIndices(1, 9);

		AddLineIndices(2, 3);
		AddLineIndices(2, 4);
		AddLineIndices(2, 6);
		AddLineIndices(2, 10);
		AddLineIndices(2, 11);

		AddLineIndices(0, 5);
		AddLineIndices(5, 9);
		AddLineIndices(9, 8);
		AddLineIndices(8, 7);
		AddLineIndices(7, 0);

		AddLineIndices(10, 11);
		AddLineIndices(11, 4);
		AddLineIndices(4, 3);
		AddLineIndices(3, 6);
		AddLineIndices(6, 10);

		AddLineIndices(0, 11);
		AddLineIndices(11, 5);
		AddLineIndices(5, 4);
		AddLineIndices(4, 9);
		AddLineIndices(9, 3);
		AddLineIndices(3, 8);
		AddLineIndices(8, 6);
		AddLineIndices(6, 7);
		AddLineIndices(7, 10);
		AddLineIndices(10, 0);

		for (int i = 0; i <= recursionLevel - 1; i++) {
			LinkedList<TriangleIndices> faces2 = new LinkedList<TriangleIndices>();

			LinkedList<TriangleIndices>.Enumerator j = faces.GetEnumerator();
			while (j.MoveNext()) {
				TriangleIndices f = j.Current;
				int a = GetMiddlePoint(f.v1, f.v2);
				int b = GetMiddlePoint(f.v2, f.v3);
				int c = GetMiddlePoint(f.v3, f.v1);

				removeLineIndices(f.v1, f.v2);
				removeLineIndices(f.v2, f.v3);
				removeLineIndices(f.v3, f.v1);

				faces2.AddLast(new TriangleIndices(f.v1, a, c));
				faces2.AddLast(new TriangleIndices(f.v2, b, a));
				faces2.AddLast(new TriangleIndices(f.v3, c, b));
				faces2.AddLast(new TriangleIndices(a, b, c));

				AddTriangleLines(f.v1, a, c);
				AddTriangleLines(f.v2, b, a);
				AddTriangleLines(f.v3, c, b);
			}

			faces = faces2;
		}
	}
	public void AddLineIndices(int index0, int index1) {
		_lineIndices.AddLast(new LineIndices(index0, index1));
	}
	public void removeLineIndices(int index0, int index1) {
		LinkedListNode<LineIndices> result = _lineIndices.Find(new LineIndices(index0, index1));
		if (result != null) {
			_lineIndices.Remove(result);
		}

	}
	public void AddTriangleLines(int index0, int index1, int index2) {
		AddLineIndices(index0, index1);
		AddLineIndices(index1, index2);
		AddLineIndices(index2, index0);
	}
	public int AddVertex(Vector3 vertex) {
		float length = vertex.Length;
		vertices.Add(new Vector3(vertex.x / length, vertex.y / length, vertex.z / length));

		index += 1;
		return index - 1;
	}
	public int GetMiddlePoint(int index0, int index1) {
		bool isFirstSmaller = index0 < index1;
		long smallerIndex = isFirstSmaller ? index0 : index1;
		long largerIndex = isFirstSmaller ? index1 : index0;
		long key = (smallerIndex << 32) | largerIndex;

		if (middlePointIndexCache.ContainsKey(key) && middlePointIndexCache[key] != middlePointIndexCache.Keys.Count) {
			return middlePointIndexCache[key];
		}

		Vector3 point1 = vertices[index0];
		Vector3 point2 = vertices[index1];
		Vector3 middle = point1.MidPoint(point2);

		int index = AddVertex(middle);
		if (middlePointIndexCache.ContainsKey(key) == false) {
			middlePointIndexCache.Add(key, index);
		}
		else {
			middlePointIndexCache[key] = index;

		}
		return index;
	}
	public void AddFace(int index0, int index1, int index2) {
		faces.AddLast(new TriangleIndices(index0, index1, index2));
	}
	public void AddToLineIndices(int baseIndex, LinkedList<int> tarGet) {
		LinkedList<LineIndices>.Enumerator i = _lineIndices.GetEnumerator();
		while (i.MoveNext()) {
			tarGet.AddLast(baseIndex + i.Current.v1);
			tarGet.AddLast(baseIndex + i.Current.v2);
		}
	}
	public void AddToTriangleIndices(int baseIndex, LinkedList<int> tarGet) {
		LinkedList<TriangleIndices>.Enumerator i = faces.GetEnumerator();
		while (i.MoveNext()) {
			tarGet.AddLast(baseIndex + i.Current.v1);
			tarGet.AddLast(baseIndex + i.Current.v2);
			tarGet.AddLast(baseIndex + i.Current.v3);
		}
	}
	public int AddToVertices(LinkedList<KeyValuePair<Vector3, ColourValue>> tarGet, Vector3 position, ColourValue colour, float scale) {
		Matrix4 transform = Matrix4.IDENTITY;
		transform.SetTrans(position);
		transform.SetScale(new Vector3(scale, scale, scale));

		for (int i = 0; i <= System.Convert.ToInt32(vertices.Count) - 1; i++) {
			tarGet.AddLast(new KeyValuePair<Vector3, ColourValue>(transform * vertices[i], colour));
		}

		return vertices.Count;
	}



}

/// <summary>
/// This is the port of the Ogre DebugDrawer from https://bitbucket.org/hasyimi/ogre-debug-drawing-utility/
/// </summary>
/// <remarks></remarks>
public class MogreDebugDrawer : System.IDisposable {
	#region "Singleton"
	private static MogreDebugDrawer _Singleton;
	public static MogreDebugDrawer Singleton {
		get { return _Singleton; }
	}
	public static void SetSingleton(MogreDebugDrawer DebugDrawer) {
		_Singleton = DebugDrawer;
	}
	#endregion
	readonly int DEFAULT_ICOSPHERE_RECURSION_LEVEL = 0;
	public bool Enabled {
		get { return GetEnabled(); }
		set { SetEnabled(value); }
	}
	private bool GetEnabled() {
		return isEnabled;
	}
	private void SetEnabled(bool _isEnabled) {
		isEnabled = _isEnabled;
	}
	public void SwitchEnabled() {
		isEnabled = !isEnabled;
	}


	private SceneManager sceneManager;
	private ManualObject manualObject;
	private float fillAlpha = 1;

	private IcoSphere icoSphere = new IcoSphere();

	private bool isEnabled = true;
	private LinkedList<KeyValuePair<Vector3, ColourValue>> lineVertices = new LinkedList<KeyValuePair<Vector3, ColourValue>>();
	private LinkedList<KeyValuePair<Vector3, ColourValue>> triangleVertices = new LinkedList<KeyValuePair<Vector3, ColourValue>>();
	private LinkedList<int> lineIndices = new LinkedList<int>();

	private LinkedList<int> triangleIndices = new LinkedList<int>();
	private int linesIndex;

	private int trianglesIndex;
	public MogreDebugDrawer(SceneManager _sceneManager, float _fillAlpha) {
		sceneManager = _sceneManager;
		fillAlpha = _fillAlpha;
		manualObject = null;
		linesIndex = 0;
		trianglesIndex = 0;
		Initialise();
	}

	private void Initialise() {
		manualObject = sceneManager.CreateManualObject("debug_object");
		manualObject.CastShadows = false;

		// set up material
		string matName = "MogreDebugDrawerMaterial";
		MaterialPtr mtl = MaterialManager.Singleton.GetDefaultSettings().Clone(matName);
		mtl.ReceiveShadows = false;
		mtl.SetSceneBlending(SceneBlendType.SBT_TRANSPARENT_ALPHA);
		mtl.SetDepthBias(0.1f, 0);
		// also for the material
		TextureUnitState tu = mtl.GetTechnique(0).GetPass(0).CreateTextureUnitState();
		tu.SetColourOperationEx(LayerBlendOperationEx.LBX_SOURCE1, LayerBlendSource.LBS_DIFFUSE);
		mtl.GetTechnique(0).SetLightingEnabled(false);

		sceneManager.RootSceneNode.CreateChildSceneNode("debug_object").AttachObject(manualObject);
		manualObject.Dynamic = (true);

		icoSphere.create(this.DEFAULT_ICOSPHERE_RECURSION_LEVEL);

		manualObject.Begin(matName, RenderOperation.OperationTypes.OT_LINE_LIST);
		manualObject.Position(Vector3.ZERO);
		manualObject.Colour(ColourValue.ZERO);
		manualObject.Index(0);
		manualObject.End();
		manualObject.Begin(matName, RenderOperation.OperationTypes.OT_TRIANGLE_LIST);
		manualObject.Position(Vector3.ZERO);
		manualObject.Colour(ColourValue.ZERO);
		manualObject.Index(0);
		manualObject.End();

		trianglesIndex = 0;
		linesIndex = trianglesIndex;
	}
	public void SetIcoSphereRecursionLevel(int recursionLevel) {
		icoSphere.create(recursionLevel);
	}
	public void shutdown() {
		sceneManager.DestroySceneNode("debug_object");
		sceneManager.DestroyManualObject(manualObject);
	}
	public void BuildLine(Vector3 start, Vector3 end, ColourValue colour, float alpha) {
		int i = AddLineVertex(start, new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddLineVertex(end, new ColourValue(colour.r, colour.g, colour.b, alpha));

		AddLineIndices(i, i + 1);
	}
	public void BuildQuad(Vector3[] vertices, ColourValue colour, float alpha) {
		int index = AddLineVertex(vertices[0], new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddLineVertex(vertices[1], new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddLineVertex(vertices[2], new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddLineVertex(vertices[3], new ColourValue(colour.r, colour.g, colour.b, alpha));

		for (int i = 0; i <= 3; i++) {
			AddLineIndices(index + i, index + ((i + 1) % 4));
		}
	}
	public void BuildCircle(Vector3 centre, float radius, int segmentsCount, ColourValue colour, float alpha) {
		int index = linesIndex;
		float increment = 2 * Math.PI / segmentsCount;
		float angle = 0f;

		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddLineVertex(new Vector3(centre.x + radius * Math.Cos(angle), centre.y, centre.z + radius * Math.Sin(angle)), new ColourValue(colour.r, colour.g, colour.b, alpha));
			angle += increment;
		}

		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddLineIndices(index + i, i + 1 < segmentsCount ? index + i + 1 : index);
		}
	}
	public void BuildFilledCircle(Vector3 centre, float radius, int segmentsCount, ColourValue colour, float alpha) {
		int index = trianglesIndex;
		float increment = 2 * Math.PI / segmentsCount;
		float angle = 0f;

		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddTriangleVertex(new Vector3(centre.x + radius * Math.Cos(angle), centre.y, centre.z + radius * Math.Sin(angle)), new ColourValue(colour.r, colour.g, colour.b, alpha));
			angle += increment;
		}

		AddTriangleVertex(centre, new ColourValue(colour.r, colour.g, colour.b, alpha));

		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddTriangleIndices(i + 1 < segmentsCount ? index + i + 1 : index, index + i, index + segmentsCount);
		}
	}
	public void BuildCylinder(Vector3 centre, float radius, int segmentsCount, float height, ColourValue colour, float alpha) {
		int index = linesIndex;
		float increment = System.Convert.ToSingle(2 * Math.PI / segmentsCount);
		float angle = 0f;

		// Top circle
		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddLineVertex(new Vector3(System.Convert.ToSingle(centre.x + radius * Math.Cos(angle)), System.Convert.ToSingle(centre.y + height / 2), System.Convert.ToSingle(centre.z + radius * Math.Sin(angle))), new ColourValue(colour.r, colour.g, colour.b, alpha));
			angle += increment;
		}

		angle = 0f;

		// Bottom circle
		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddLineVertex(new Vector3(centre.x + radius * Math.Cos(angle), centre.y - height / 2, centre.z + radius * Math.Sin(angle)), new ColourValue(colour.r, colour.g, colour.b, alpha));
			angle += increment;
		}

		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddLineIndices(index + i, i + 1 < segmentsCount ? index + i + 1 : index);
			AddLineIndices(segmentsCount + index + i, i + 1 < segmentsCount ? segmentsCount + index + i + 1 : segmentsCount + index);
			AddLineIndices(index + i, segmentsCount + index + i);
		}
	}
	public void BuildFilledCylinder(Vector3 centre, float radius, int segmentsCount, float height, ColourValue colour, float alpha) {
		int index = trianglesIndex;
		float increment = 2 * Math.PI / segmentsCount;
		float angle = 0f;

		// Top circle
		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddTriangleVertex(new Vector3(centre.x + radius * Math.Cos(angle), centre.y + height / 2, centre.z + radius * Math.Sin(angle)), new ColourValue(colour.r, colour.g, colour.b, alpha));
			angle += increment;
		}

		AddTriangleVertex(new Vector3(centre.x, centre.y + height / 2, centre.z), new ColourValue(colour.r, colour.g, colour.b, alpha));

		angle = 0f;

		// Bottom circle
		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddTriangleVertex(new Vector3(centre.x + radius * Math.Cos(angle), centre.y - height / 2, centre.z + radius * Math.Sin(angle)), new ColourValue(colour.r, colour.g, colour.b, alpha));
			angle += increment;
		}

		AddTriangleVertex(new Vector3(centre.x, centre.y - height / 2, centre.z), new ColourValue(colour.r, colour.g, colour.b, alpha));

		for (int i = 0; i <= segmentsCount - 1; i++) {
			AddTriangleIndices(i + 1 < segmentsCount ? index + i + 1 : index, index + i, index + segmentsCount);

			AddTriangleIndices(i + 1 < segmentsCount ? (segmentsCount + 1) + index + i + 1 : (segmentsCount + 1) + index, (segmentsCount + 1) + index + segmentsCount, (segmentsCount + 1) + index + i);

			AddQuadIndices(index + i, i + 1 < segmentsCount ? index + i + 1 : index, i + 1 < segmentsCount ? (segmentsCount + 1) + index + i + 1 : (segmentsCount + 1) + index, (segmentsCount + 1) + index + i);
		}
	}
	public void BuildCuboid(Vector3[] vertices, ColourValue colour, float alpha) {
		int index = AddLineVertex(vertices[0], new ColourValue(colour.r, colour.g, colour.b, alpha));
		for (int i = 1; i <= 7; i++) {
			AddLineVertex(vertices[i], new ColourValue(colour.r, colour.g, colour.b, alpha));
		}

		for (int i = 0; i <= 3; i++) {
			AddLineIndices(index + i, index + ((i + 1) % 4));
		}
		for (int i = 4; i <= 7; i++) {
			AddLineIndices(index + i, i == 7 ? index + 4 : index + i + 1);
		}
		AddLineIndices(index + 1, index + 5);
		AddLineIndices(index + 2, index + 4);
		AddLineIndices(index, index + 6);
		AddLineIndices(index + 3, index + 7);
	}
	public void BuildFilledCuboid(Vector3[] vertices, ColourValue colour, float alpha) {
		int index = AddTriangleVertex(vertices[0], new ColourValue(colour.r, colour.g, colour.b, alpha));
		for (int i = 1; i <= 7; i++) {
			AddTriangleVertex(vertices[i], new ColourValue(colour.r, colour.g, colour.b, alpha));
		}

		AddQuadIndices(index, index + 1, index + 2, index + 3);
		AddQuadIndices(index + 4, index + 5, index + 6, index + 7);

		AddQuadIndices(index + 1, index + 5, index + 4, index + 2);
		AddQuadIndices(index, index + 3, index + 7, index + 6);

		AddQuadIndices(index + 1, index, index + 6, index + 5);
		AddQuadIndices(index + 4, index + 7, index + 3, index + 2);
	}
	public void BuildFilledQuad(Vector3[] vertices, ColourValue colour, float alpha) {
		int index = AddTriangleVertex(vertices[0], new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddTriangleVertex(vertices[1], new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddTriangleVertex(vertices[2], new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddTriangleVertex(vertices[3], new ColourValue(colour.r, colour.g, colour.b, alpha));

		AddQuadIndices(index, index + 1, index + 2, index + 3);
	}
	public void BuildFilledTriangle(Vector3[] vertices, ColourValue colour, float alpha) {
		int index = AddTriangleVertex(vertices[0], new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddTriangleVertex(vertices[1], new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddTriangleVertex(vertices[2], new ColourValue(colour.r, colour.g, colour.b, alpha));

		AddTriangleIndices(index, index + 1, index + 2);
	}
	public void BuildTetrahedron(Vector3 centre, float scale, ColourValue colour, float alpha) {
		int index = linesIndex;

		// Distance from the centre
		float bottomDistance = scale * 0.2f;
		float topDistance = scale * 0.62f;
		float frontDistance = scale * 0.289f;
		float backDistance = scale * 0.577f;
		float leftRightDistance = scale * 0.5f;

		AddLineVertex(new Vector3(centre.x, centre.y + topDistance, centre.z), new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddLineVertex(new Vector3(centre.x, centre.y - bottomDistance, centre.z + frontDistance), new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddLineVertex(new Vector3(centre.x + leftRightDistance, centre.y - bottomDistance, centre.z - backDistance), new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddLineVertex(new Vector3(centre.x - leftRightDistance, centre.y - bottomDistance, centre.z - backDistance), new ColourValue(colour.r, colour.g, colour.b, alpha));

		AddLineIndices(index, index + 1);
		AddLineIndices(index, index + 2);
		AddLineIndices(index, index + 3);

		AddLineIndices(index + 1, index + 2);
		AddLineIndices(index + 2, index + 3);
		AddLineIndices(index + 3, index + 1);
	}
	public void BuildFilledTetrahedron(Vector3 centre, float scale, ColourValue colour, float alpha) {
		int index = trianglesIndex;

		// Distance from the centre
		float bottomDistance = scale * 0.2f;
		float topDistance = scale * 0.62f;
		float frontDistance = scale * 0.289f;
		float backDistance = scale * 0.577f;
		float leftRightDistance = scale * 0.5f;

		AddTriangleVertex(new Vector3(centre.x, centre.y + topDistance, centre.z), new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddTriangleVertex(new Vector3(centre.x, centre.y - bottomDistance, centre.z + frontDistance), new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddTriangleVertex(new Vector3(centre.x + leftRightDistance, centre.y - bottomDistance, centre.z - backDistance), new ColourValue(colour.r, colour.g, colour.b, alpha));
		AddTriangleVertex(new Vector3(centre.x - leftRightDistance, centre.y - bottomDistance, centre.z - backDistance), new ColourValue(colour.r, colour.g, colour.b, alpha));

		AddTriangleIndices(index, index + 1, index + 2);
		AddTriangleIndices(index, index + 2, index + 3);
		AddTriangleIndices(index, index + 3, index + 1);

		AddTriangleIndices(index + 1, index + 3, index + 2);
	}
	public void DrawLine(Vector3 start, Vector3 end, ColourValue colour) {
		BuildLine(start, end, colour, 1);
	}
	public void DrawCircle(Vector3 centre, float radius, int segmentsCount, ColourValue colour, bool isFilled) {
		BuildCircle(centre, radius, segmentsCount, colour, 1);
		if (isFilled) {
			BuildFilledCircle(centre, radius, segmentsCount, colour, fillAlpha);
		}
	}
	public void DrawCylinder(Vector3 centre, float radius, int segmentsCount, float height, ColourValue colour, bool isFilled) {
		BuildCylinder(centre, radius, segmentsCount, height, colour, 1);
		if (isFilled) {
			BuildFilledCylinder(centre, radius, segmentsCount, height, colour, fillAlpha);
		}
	}
	public void DrawQuad(Vector3[] vertices, ColourValue colour, bool isFilled) {
		BuildQuad(vertices, colour, 1);
		if (isFilled) {
			BuildFilledQuad(vertices, colour, fillAlpha);
		}
	}
	public void DrawCuboid(Vector3[] vertices, ColourValue colour, bool isFilled) {
		BuildCuboid(vertices, colour, 1);
		if (isFilled) {
			BuildFilledCuboid(vertices, colour, fillAlpha);
		}
	}
	public void DrawSphere(Vector3 centre, float radius, ColourValue colour, bool isFilled) {
		int baseIndex = linesIndex;
		linesIndex += icoSphere.AddToVertices(lineVertices, centre, colour, radius);
		icoSphere.AddToLineIndices(baseIndex, lineIndices);

		if (isFilled) {
			baseIndex = trianglesIndex;
			trianglesIndex += icoSphere.AddToVertices(triangleVertices, centre, new ColourValue(colour.r, colour.g, colour.b, fillAlpha), radius);
			icoSphere.AddToTriangleIndices(baseIndex, triangleIndices);
		}
	}
	public void DrawTetrahedron(Vector3 centre, float scale, ColourValue colour, bool isFilled) {
		BuildTetrahedron(centre, scale, colour, 1);
		if (isFilled) {
			BuildFilledTetrahedron(centre, scale, colour, fillAlpha);
		}
	}
	public void Build() {
		manualObject.BeginUpdate(0);
		if (lineVertices.Count > 0 && isEnabled) {
			manualObject.EstimateVertexCount(System.Convert.ToUInt32(lineVertices.Count));
			manualObject.EstimateIndexCount(System.Convert.ToUInt32(lineIndices.Count));
			LinkedList<KeyValuePair<Vector3, ColourValue>>.Enumerator i = lineVertices.GetEnumerator();
			while (i.MoveNext()) {
				manualObject.Position(i.Current.Key);
				manualObject.Colour(i.Current.Value);
			}
			LinkedList<int>.Enumerator i2 = lineIndices.GetEnumerator();
			while (i2.MoveNext()) {
				manualObject.Index(System.Convert.ToUInt16(i2.Current));
			}
		}
		manualObject.End();

		manualObject.BeginUpdate(1);
		if (triangleVertices.Count > 0 && isEnabled) {
			manualObject.EstimateVertexCount(System.Convert.ToUInt16((triangleVertices.Count)));
			manualObject.EstimateIndexCount(System.Convert.ToUInt16(triangleIndices.Count));
			LinkedList<KeyValuePair<Vector3, ColourValue>>.Enumerator i = triangleVertices.GetEnumerator();
			while (i.MoveNext()) {
				manualObject.Position(i.Current.Key);
				manualObject.Colour(i.Current.Value.r, i.Current.Value.g, i.Current.Value.b, fillAlpha);
			}
			LinkedList <int>.Enumerator i2 = triangleIndices.GetEnumerator();
			while (i2.MoveNext()) {
				manualObject.Index(System.Convert.ToUInt16(i2.Current));
			}
		}
		manualObject.End();
	}
	public void Clear() {
		lineVertices.Clear();
		triangleVertices.Clear();
		lineIndices.Clear();
		triangleIndices.Clear();
		trianglesIndex = 0;
		linesIndex = trianglesIndex;
	}
	public int AddLineVertex(Vector3 vertex, ColourValue colour) {
		lineVertices.AddLast(new KeyValuePair<Vector3, ColourValue>(vertex, colour));

		linesIndex += 1;
		return linesIndex - 1;
	}
	public void AddLineIndices(int index1, int index2) {
		lineIndices.AddLast(index1);
		lineIndices.AddLast(index2);
	}
	public int AddTriangleVertex(Vector3 vertex, ColourValue colour) {
		triangleVertices.AddLast(new KeyValuePair<Vector3, ColourValue>(vertex, colour));

		trianglesIndex += 1;
		return trianglesIndex - 1;
	}
	public void AddTriangleIndices(int index1, int index2, int index3) {
		triangleIndices.AddLast(index1);
		triangleIndices.AddLast(index2);
		triangleIndices.AddLast(index3);
	}
	public void AddQuadIndices(int index1, int index2, int index3, int index4) {
		triangleIndices.AddLast(index1);
		triangleIndices.AddLast(index2);
		triangleIndices.AddLast(index3);

		triangleIndices.AddLast(index1);
		triangleIndices.AddLast(index3);
		triangleIndices.AddLast(index4);
	}
	#region "IDisposable Support"
	// So ermitteln Sie überflüssige Aufrufe
	private bool isDisposed;

	// IDisposable
	protected virtual void Dispose(bool disposing) {
		if (!this.isDisposed) {
			if (disposing) {
				// TO DO: Verwalteten Zustand löschen (verwaltete Objekte).
			}
			shutdown();
			// TO DO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalize() unten überschreiben.
			// TO DO: Große Felder auf NULL festlegen.
		}
		this.isDisposed = true;
	}

	// TO DO: Finalize() nur überschreiben, wenn Dispose(ByVal disposing As Boolean) oben über Code zum Freigeben von nicht verwalteten Ressourcen verfügt.
	//Protected Overrides Sub Finalize()
	//    ' Ändern Sie diesen Code nicht. Fügen Sie oben in Dispose(ByVal disposing As Boolean) Bereinigungscode ein.
	//    Dispose(False)
	//    MyBase.Finalize()
	//End Sub

	// Dieser Code wird von Visual Basic hinzugefügt, um das Dispose-Muster richtig zu implementieren.
	public void Dispose() {
		// Ändern Sie diesen Code nicht. Fügen Sie oben in Dispose(ByVal disposing As Boolean) Bereinigungscode ein.
		Dispose(true);
		System.GC.SuppressFinalize(this);
	}
	#endregion

}
#endif