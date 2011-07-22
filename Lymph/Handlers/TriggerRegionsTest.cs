using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Phys;

namespace Ponykart.Handlers {
	/// <summary>
	/// just a little test for the trigger regions
	/// </summary>
	public class TriggerRegionsTest {

		public TriggerRegionsTest() {

			// make some physics objects
			ShapeDesc sd = new SphereShapeDesc(1);
			sd.ShapeFlags |= ShapeFlags.TriggerEnable;

			ActorDesc ad = new ActorDesc(sd);
			ad.Name = "test trigger area";
			ad.GlobalPosition = new Vector3(5, 0, 5);

			Actor a = LKernel.Get<PhysXMain>().Scene.CreateActor(ad);
			Shape shape = a.Shapes[0];
			shape.Group = Groups.CollidableNonPushableID;

			// make some mogre objects so we can see what we're doing
			var sceneMgr = LKernel.Get<SceneManager>();
			Entity e = sceneMgr.CreateEntity("test trigger area entity", "primitives/ellipsoid.mesh");
			e.SetMaterialName("BalloonGlow_orange");
			e.RenderQueueGroup = GlowHandler.RENDER_QUEUE_BUBBLE_GLOW;
			SceneNode sn = sceneMgr.RootSceneNode.CreateChildSceneNode("test trigger area node");
			sn.AttachObject(e);
			sn.Position = a.GlobalPosition;

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
