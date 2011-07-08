using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lymph.Core;
using Lymph.Stuff;

namespace Lymph.Actors {
	/// <summary>
	/// Enemy base class
	/// </summary>
	public abstract class Enemy : Cell {
		public ICollection<AntigenColour> Colours { get; private set; }
		public ICollection<AntibodyAttachment> Antibodies { get; private set; }

		public Enemy(ThingTemplate tt, params AntigenColour[] colours) : base(tt)
		{
			this.Colours = colours;
			this.Antibodies = new Collection<AntibodyAttachment>();
		}

		#region IDisposable stuff
		public override void Dispose() {
			foreach (AntibodyAttachment a in Antibodies) {
				a.Dispose();
			}
			base.Dispose();
		}
		#endregion
	}
}
