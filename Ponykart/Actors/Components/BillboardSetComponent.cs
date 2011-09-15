using System;
using Mogre;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Represents a billboard set, aka a collection of billboards that share the same material, direction, etc.
	/// </summary>
	public class BillboardSetComponent : LDisposable {
		public int ID { get; protected set; }
		public string Name { get; protected set; }
		public BillboardSet BillboardSet { get; protected set; }

		/// <summary>
		/// Woo billboards! These are pretty dissimilar from ogre entities - they only have a size and material, and no mesh or anything
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public BillboardSetComponent(LThing lthing, ThingBlock template, BillboardSetBlock block) {
			ID = IDs.New;
			Name = block.GetStringProperty("name", template.ThingName);

			// set it up
			BillboardSet = LKernel.GetG<SceneManager>().CreateBillboardSet(Name + ID + "BillboardSet", (uint) block.BillboardBlocks.Count);
			BillboardSet.SetMaterialName(block.GetStringProperty("material", null));
			BillboardSet.CastShadows = block.GetBoolProperty("CastsShadows", false);
			BillboardSet.SetDefaultDimensions(block.GetFloatProperty("width", 1), block.GetFloatProperty("height", 1));

			// billboard type
			ThingEnum type = block.GetEnumProperty("Type", ThingEnum.Point);
			switch (type) {
				case ThingEnum.Point:
					BillboardSet.BillboardType = BillboardType.BBT_POINT; break;
				case ThingEnum.OrientedCommon:
					BillboardSet.BillboardType = BillboardType.BBT_ORIENTED_COMMON; break;
				case ThingEnum.OrientedSelf:
					BillboardSet.BillboardType = BillboardType.BBT_ORIENTED_SELF; break;
				case ThingEnum.PerpendicularCommon:
					BillboardSet.BillboardType = BillboardType.BBT_PERPENDICULAR_COMMON; break;
				case ThingEnum.PerpendicularSelf:
					BillboardSet.BillboardType = BillboardType.BBT_PERPENDICULAR_SELF; break;
			}

			Vector3 vec;
			if (block.VectorTokens.TryGetValue("UpVector", out vec))
				BillboardSet.CommonUpVector = vec;
			// sort transparent stuff
			BillboardSet.SortingEnabled = block.GetBoolProperty("Sort", true);
			// make them point the right way
			BillboardSet.CommonDirection = block.GetVectorProperty("Direction", Vector3.UNIT_Y);
			// rotation type
			BillboardSet.BillboardRotationType = block.GetBoolProperty("UseVertexRotation", false) ? BillboardRotationType.BBR_TEXCOORD : BillboardRotationType.BBR_VERTEX;
			// origin
			ThingEnum originToken;
			if (block.EnumTokens.TryGetValue("Origin", out originToken)) {
				BillboardOrigin origin;
				if (Enum.TryParse<BillboardOrigin>(originToken + string.Empty, true, out origin))
					BillboardSet.BillboardOrigin = origin;
			}

			// and then attach it to our root node
			lthing.RootNode.AttachObject(BillboardSet);

			// and then go through each billboard block and create a billboard from it
			foreach (BillboardBlock bbblock in block.BillboardBlocks)
				CreateBillboard(bbblock);
		}

		/// <summary>
		/// Make one billboard from each billboard block.
		/// </summary>
		/// <param name="block"></param>
		void CreateBillboard(BillboardBlock block) {
			// make our billboard
			Billboard bb = BillboardSet.CreateBillboard(block.GetVectorProperty("Position", null));
			// set its color if it has one
			Quaternion quat;
			if (block.QuatTokens.TryGetValue("Colour", out quat))
				bb.Colour = quat.ToColourValue();
			// and a rotation
			bb.Rotation = new Degree(block.GetFloatProperty("Rotation", 0));

			// it's best to not do this unless we really need to since it makes it less efficient
			float height, width;
			if (block.FloatTokens.TryGetValue("Width", out width) && block.FloatTokens.TryGetValue("Height", out height))
				bb.SetDimensions(width, height);
		}

		public override string ToString() {
			return Name + ID + "BillboardSet";
		}

		/// <summary>
		/// clean up our billboard set
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			var sceneMgr = LKernel.GetG<SceneManager>();
			bool valid = LKernel.GetG<LevelManager>().IsValidLevel;

			if (BillboardSet != null) {
				if (valid && disposing)
					sceneMgr.DestroyBillboardSet(BillboardSet);
				BillboardSet.Dispose();
				BillboardSet = null;
			}

			base.Dispose(disposing);
		}
	}
}
