using Mogre.PhysX;
using Ponykart.Phys;

namespace Ponykart.Actors {
	public class Obstacle : KinematicThing {
		protected override uint DefaultCollisionGroupID {
			get { return Groups.CollidableNonPushableID; }
		}
		protected override string DefaultMaterial {
			get { return "yellowbrick"; }
		}
		protected override string DefaultModel {
			get { return "primitives/box.mesh"; }
		}
		/*protected override MoveBehaviour DefaultMoveBehaviour {
			get { return MoveBehaviour.IGNORE; }
		}
		protected override float DefaultMoveSpeed {
			get { return 0; }
		}*/
		protected override ShapeDesc ShapeDesc {
			get {
				return new BoxShapeDesc(SpawnScale / 2f);
			}
		}

		public Obstacle(ThingTemplate tt) : base(tt) { }
	}
}
