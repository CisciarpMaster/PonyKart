using System;
using Ponykart.Phys;
using Mogre;
using Mogre.PhysX;

namespace Ponykart.Handlers {
	/// <summary>
	/// just a little test for the trigger regions
	/// </summary>
	public class TriggerRegionsTest {
		Shape shape;

		public TriggerRegionsTest() {

			// make some physics objects
			ShapeDesc sd = new SphereShapeDesc(1);
			sd.ShapeFlags |= ShapeFlags.TriggerEnable;

			ActorDesc ad = new ActorDesc(sd);
			ad.Name = "test trigger area";
			ad.GlobalPosition = new Vector3(5, 0, 5);

			Actor a = LKernel.Get<PhysXMain>().Scene.CreateActor(ad);
			shape = a.Shapes[0];
			shape.Group = Groups.CollidableNonPushableID;

			// make some mogre objects so we can see what we're doing
			var sceneMgr = LKernel.Get<SceneManager>();
			Entity e = sceneMgr.CreateEntity("test trigger area entity", "LymphyOuterMembrane.mesh");
			e.SetMaterialName("Lymphy_InnerGlow");
			SceneNode sn = sceneMgr.RootSceneNode.CreateChildSceneNode("test trigger area node");
			sn.AttachObject(e);
			sn.SetScale(2, 2, 2);
			sn.SetPosition(5, 0, 5);

			// attach the handler
			LKernel.Get<TriggerReporter>().AddEvent(a.Name, doSomething);
		}

		private void doSomething(Shape triggerShape, Shape otherShape, TriggerFlags flags) {
			if (TriggerReporter.IsEnterFlag(flags))
				Console.WriteLine(otherShape.Actor.Name + " has entered trigger area \"" + triggerShape.Actor.Name + "\"");
			if (TriggerReporter.IsLeaveFlag(flags))
				Console.WriteLine(otherShape.Actor.Name + " has left trigger area \"" + triggerShape.Actor.Name + "\"");
		}
	}
}
