using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PonykartParsers {
	/// <summary>
	/// Represents a .thing file - these are one per file and not one per LThing object!
	/// </summary>
	public class ThingDefinition : TokenHolder {
		public string Name { get; protected set; }
		public ICollection<ShapeBlock> ShapeBlocks { get; protected set; }
		public ICollection<ModelBlock> ModelBlocks { get; protected set; }
		public ICollection<RibbonBlock> RibbonBlocks { get; protected set; }
		public ICollection<BillboardBlock> BillboardBlocks { get; protected set; }

		public ThingDefinition(string name) {
			Name = name;
			SetUpDictionaries();
		}

		public override void SetUpDictionaries() {
			base.SetUpDictionaries();
			ShapeBlocks = new Collection<ShapeBlock>();
			ModelBlocks = new Collection<ModelBlock>();
			RibbonBlocks = new Collection<RibbonBlock>();
			BillboardBlocks = new Collection<BillboardBlock>();
		}

		/// <summary>
		/// Must be called after you're done importing everything into the dictionaries
		/// </summary>
		public override void Finish() {
			foreach (ShapeBlock sb in ShapeBlocks)
				sb.Finish();
			foreach (ModelBlock mb in ModelBlocks)
				mb.Finish();
			foreach (RibbonBlock rb in RibbonBlocks)
				rb.Finish();
			foreach (BillboardBlock bb in BillboardBlocks) {
				bb.Finish();
			}
		}

		public override void Dispose() {
			foreach (ShapeBlock sb in ShapeBlocks)
				sb.Dispose();
			foreach (ModelBlock mb in ModelBlocks)
				mb.Dispose();
			foreach (RibbonBlock rb in RibbonBlocks)
				rb.Dispose();
			foreach (BillboardBlock bb in BillboardBlocks) {
				bb.Dispose();
			}
			base.Dispose();
		}
	}
}
