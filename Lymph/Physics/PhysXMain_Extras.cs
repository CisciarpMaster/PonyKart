using System.IO;
using Mogre;
using Mogre.PhysX;

namespace Ponykart.Phys {
	public partial class PhysXMain {

		/// <summary>
		/// Get the mesh information for the given mesh.
		/// 
		/// Source: http://www.ogre3d.org/tikiwiki/tiki-index.php?page=Raycasting+to+the+polygon+level+-+Mogre#GetMeshInformation
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="vertex_count"></param>
		/// <param name="vertices"></param>
		/// <param name="index_count"></param>
		/// <param name="indices"></param>
		/// <param name="position"></param>
		/// <param name="orientation"></param>
		/// <param name="scale"></param>
		public unsafe void GetMeshInformation(MeshPtr mesh, ref uint vertex_count, ref Vector3[] vertices, ref uint index_count,
			ref uint[] indices, Vector3 position, Quaternion orientation, Vector3 scale)
		{
			bool added_shared = false;
			uint current_offset = 0;
			uint shared_offset = 0;
			uint next_offset = 0;
			uint index_offset = 0;

			vertex_count = index_count = 0;

			for (ushort i = 0; i < mesh.NumSubMeshes; ++i) {
				SubMesh submesh = mesh.GetSubMesh(i);
				if (submesh.useSharedVertices) {
					if (!added_shared) {
						vertex_count += mesh.sharedVertexData.vertexCount;
						added_shared = true;
					}
				} else {
					vertex_count += submesh.vertexData.vertexCount;
				}

				index_count += submesh.indexData.indexCount;
			}

			vertices = new Vector3[vertex_count];
			indices = new uint[index_count];
			added_shared = false;

			for (ushort i = 0; i < mesh.NumSubMeshes; ++i) {
				SubMesh submesh = mesh.GetSubMesh(i);
				VertexData vertex_data = submesh.useSharedVertices ? mesh.sharedVertexData : submesh.vertexData;

				if (!submesh.useSharedVertices || (submesh.useSharedVertices && !added_shared)) {
					if (submesh.useSharedVertices) {
						added_shared = true;
						shared_offset = current_offset;
					}

					VertexElement posElem =
						vertex_data.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.VES_POSITION);
					HardwareVertexBufferSharedPtr vbuf =
						vertex_data.vertexBufferBinding.GetBuffer(posElem.Source);

					byte* vertex = (byte*)vbuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
					float* pReal;

					for (int j = 0; j < vertex_data.vertexCount; ++j, vertex += vbuf.VertexSize) {
						posElem.BaseVertexPointerToElement(vertex, &pReal);
						Vector3 pt = new Vector3(pReal[0], pReal[1], pReal[2]);
						vertices[current_offset + j] = (orientation * (pt * scale)) + position;
					}
					vbuf.Unlock();
					next_offset += vertex_data.vertexCount;
				}

				IndexData index_data = submesh.indexData;
				uint numTris = index_data.indexCount / 3;
				HardwareIndexBufferSharedPtr ibuf = index_data.indexBuffer;

				bool use32bitindexes = (ibuf.Type == HardwareIndexBuffer.IndexType.IT_32BIT);

				uint* pInt = (uint*)ibuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
				ushort* pShort = (ushort*)pInt;
				uint offset = submesh.useSharedVertices ? shared_offset : current_offset;
				if (use32bitindexes) {
					for (int k = 0; k < index_data.indexCount; ++k) {
						indices[index_offset++] = (uint)pInt[k] + (uint)offset;
					}
				} else {
					for (int k = 0; k < index_data.indexCount; ++k) {
						indices[index_offset++] = (uint)pShort[k] + (uint)offset;
					}
				}
				ibuf.Unlock();
				current_offset = next_offset;
			}

		}

		public void MakeTriangleMesh(Entity levelEnt, SceneNode levelNode, out TriangleMeshShapeDesc tmsd, out ActorDesc levelActorDesc) {
			uint vertex_count = 0;
			uint index_count = 0;
			Vector3[] vertices = new Vector3[0]; // these are [0] now but will be overwritten in GetMeshInformation
			uint[] indices = new uint[0];

			GetMeshInformation(levelEnt.GetMesh(), ref vertex_count, ref vertices, ref index_count, ref indices, levelNode._getDerivedPosition(),
				levelNode._getDerivedOrientation(), levelNode.GetScale());

			float[] verts = new float[vertex_count * 3];
			for (int a = 0; a < vertex_count; a++) {
				int b = a * 3;
				verts[b] = vertices[a].x;
				verts[b + 1] = vertices[a].y;
				verts[b + 2] = vertices[a].z;
			}

			MemoryStream stream = new MemoryStream();
			CookingInterface.InitCooking();
			CookingInterface.CookTriangleMesh(verts, indices, stream);
			CookingInterface.CloseCooking();
			stream.Position = 0;

			TriangleMesh triMesh = physics.CreateTriangleMesh(stream);

			tmsd = new TriangleMeshShapeDesc() {
				TriangleMesh = triMesh,
				ShapeFlags = ShapeFlags.Visualization
			};

			levelActorDesc = new ActorDesc();
			levelActorDesc.Body = null;
			levelActorDesc.Density = 1;
			levelActorDesc.Shapes.Add(tmsd);
		}
	}
}
