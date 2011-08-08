using BulletSharp;
using Ponykart.Physics;

namespace Ponykart.Actors {
	public class Obstacle : DynamicThing {
		protected override string DefaultMaterial {
			get { return "yellowbrick"; }
		}
		protected override string DefaultModel {
			get { return "primitives/box.mesh"; }
		}
		protected override CollisionShape CollisionShape {
			get { return new BoxShape(SpawnScale); }
		}
		protected override float Mass {
			get { return 0; } // immovable
		}
		protected override PonykartCollisionGroups CollisionGroup {
			get { return PonykartCollisionGroups.Environment; }
		}
		protected override PonykartCollidesWithGroups CollidesWith {
			get { return PonykartCollidesWithGroups.Environment; }
		}

		public Obstacle(ThingTemplate tt) : base(tt) { }

	}
}
