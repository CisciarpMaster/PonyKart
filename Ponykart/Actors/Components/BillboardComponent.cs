using System;
using Mogre;
using PonykartParsers;

namespace Ponykart.Actors {
	public class BillboardComponent : IDisposable {
		public Billboard Billboard { get; protected set; }

		/// <summary>
		/// Woo billboards! These are pretty dissimilar from ogre entities - they only have a size and material, and no mesh or anything
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public BillboardComponent(LThing lthing, ThingBlock template, BillboardBlock block) {

			// make our billboard
			Billboard = lthing.BillboardSet.CreateBillboard(block.GetVectorProperty("Position", null));
			// set its color if it has one
			Quaternion quat;
			if (block.QuatTokens.TryGetValue("Colour", out quat))
				Billboard.Colour = quat.ToColourValue();
			// and a rotation
			//Billboard.Rotation = block.GetFloatProperty("Rotation", 0);

			// it's best to not do this unless we really need to since it makes it less efficient
			float height, width;
			if (block.FloatTokens.TryGetValue("Width", out width) && block.FloatTokens.TryGetValue("Height", out height))
				Billboard.SetDimensions(width, height);
		}

		/// <summary>
		/// we don't want to dispose of the billboard since the BillboardSet is still using it.
		/// </summary>
		public void Dispose() {
		}
	}
}
