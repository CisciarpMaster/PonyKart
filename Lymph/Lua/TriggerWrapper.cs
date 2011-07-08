using LuaNetInterface;
using Lymph.Phys;
using Mogre;
using Mogre.PhysX;

namespace Lymph.Lua {
	[LuaPackage("Triggers", "A wrapper for the TriggerReporter.")]
	public class TriggerWrapper {

		public TriggerWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("isEnterFlag", "Determines whether the given flag is an 'enter' flag.", "TriggerFlags tf - The flags you want to check.")]
		public static bool IsEnterFlag(TriggerFlags tf) {
			return TriggerReporter.IsEnterFlag(tf);
		}

		[LuaFunction("isLeaveFlag", "Determines whether the given flag is a 'leave' flag.", "TriggerFlags tf - The flags you want to check.")]
		public static bool IsLeaveFlag(TriggerFlags tf) {
			return TriggerReporter.IsLeaveFlag(tf);
		}

		[LuaFunction("createBoxTriggerArea", "Creates a box trigger area given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerShape, otherShape, triggerFlags)",
			"number width", "number height", "number length", "number posX", "number posY", "number posZ", "number rotX", "number rotY", "number rotZ")]
		public static void CreateBoxTriggerArea(string name, TriggerReportHandler trh,
			float width, float height, float length, float posX, float posY, float posZ, float rotX, float rotY, float rotZ)
		{
			ShapeDesc sd = new BoxShapeDesc(new Vector3(width, height, length));
			sd.ShapeFlags |= ShapeFlags.TriggerEnable;

			CreateTriggerArea(name, trh, sd, new Vector3(posX, posY, posZ), new Vector3(rotX, rotY, rotZ));
		}

		// not sure if this supports overloading or not
		/*[LuaFunction("createBoxTriggerArea", "Creates a box trigger area given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerShape, otherShape, triggerFlags)",
			"number width", "number height", "number length", "number posX", "number posY", "number posZ")]
		public static void CreateBoxTriggerArea(string name, TriggerReportHandler trh,
			float width, float height, float length, float posX, float posY, float posZ)
		{
			CreateBoxTriggerArea(name, trh, width, height, length, posX, posY, posZ, 0, 0, 0);
		}*/

		[LuaFunction("createCapsuleTriggerArea", "Creates a capsule trigger area given a name and some info and a function to call.", 
			"string name - The name of the shape", "function() trigger report handler - (triggerShape, otherShape, triggerFlags)",
			"number radius", "number height", "number posX", "number posY", "number posZ", "number rotX", "number rotY", "number rotZ")]
		public static void CreateCapsuleTriggerArea(string name, TriggerReportHandler trh, float radius, float height, float posX, float posY, float posZ, 
			float rotX, float rotY, float rotZ)
		{
			ShapeDesc sd = new CapsuleShapeDesc(radius, height);
			sd.ShapeFlags |= ShapeFlags.TriggerEnable;

			CreateTriggerArea(name, trh, sd, new Vector3(posX, posY, posZ), new Vector3(rotX, rotY, rotZ));
		}

		// not sure if it supports overloading or not
		/*[LuaFunction("createCapsuleTriggerArea", "Creates a capsule trigger area given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerShape, otherShape, triggerFlags)",
			"number radius", "number height", "number posX", "number posY", "number posZ")]
		public static void CreateCapsuleTriggerArea(string name, TriggerReportHandler trh, float radius, float height, float posX, float posY, float posZ) {

			CreateCapsuleTriggerArea(name, trh, radius, height, posX, posY, posZ, 0, 0, 0);
		}*/

		[LuaFunction("createSphereTriggerArea", "Creates a sphere trigger area given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerShape, otherShape, triggerFlags)",
			"number radius", "number posX", "number posY", "number posZ")]
		public static void CreateSphereTriggerArea(string name, TriggerReportHandler trh, float radius, float posX, float posY, float posZ) {

			ShapeDesc sd = new SphereShapeDesc(radius);
			sd.ShapeFlags |= ShapeFlags.TriggerEnable;

			CreateTriggerArea(name, trh, sd, new Vector3(posX, posY, posZ), Vector3.ZERO);
		}

		/// <summary>
		/// Creates a trigger area out of the given paramaters
		/// </summary>
		/// <param name="name">The name of the actor/trigger area</param>
		/// <param name="trh">The function to call</param>
		/// <param name="shapeDesc">The description of the shape we will make</param>
		/// <param name="position">Position of the trigger area</param>
		private static void CreateTriggerArea(string name, TriggerReportHandler trh, ShapeDesc shapeDesc, Vector3 position, Vector3 rotation) {
			ActorDesc ad = new ActorDesc(shapeDesc);
			ad.Name = name;
			ad.GlobalPosition = position;
			Matrix3 m = new Matrix3();
			m.FromEulerAnglesXYZ(rotation.x, rotation.y, rotation.z);
			ad.GlobalOrientation = m;

			Actor a = LKernel.Get<PhysXMain>().Scene.CreateActor(ad);
			Shape shape = a.Shapes[0];

			LKernel.Get<TriggerReporter>().AddEvent(name, trh);

			// here we could make a sceneNode and entity if we wanted to see it
			// but we don't - we can just use the physX debugger instead
			// TODO: some way to specify a rotation would be nice
		}

		/// <summary>
		/// Hooks up a script file to a trigger area event so it will run whenever an actor enters or leaves the specified trigger area.
		/// The problem with this is that I lose the variables from the event so I can't check whether it's an enter or leave event.
		/// Still... might be useful so I'll leave it
		/// </summary>
		/// <param name="nameOfArea">The name of the trigger area. Well technically it's the actor name of the shape of the trigger area, but eh whatever.</param>
		/// <param name="filePath">The file path of the script you want to run when the event fires. Ex: "media/scripts/example.lua"</param>
		[LuaFunction("hookScriptToTriggerArea",
			"Hooks up a script file to a trigger area event so it will run whenever an actor enters or leaves the specified trigger area.",
			"string nameOfArea - The name of the trigger area. Well technically it's the actor name of the shape of the trigger area, but eh whatever.",
			"string filePath - The file path of the script you want to run when the event fires. Ex: \"media/scripts/example.lua\"")]
		public static void HookScriptToTriggerArea(string nameOfArea, string filePath) {
			TriggerReporter tr = LKernel.Get<TriggerReporter>();

			if (tr != null) {
				Shape shape = tr.GetShapeFromName(nameOfArea);

				if (shape != null)
					tr.AddEvent(nameOfArea, (triggerShape, otherShape, flags) => LKernel.Get<LuaMain>().DoFile(filePath));
			}
		}

		/// <summary>
		/// Hooks up a function to a trigger area event so it will run whenever an actor enters of leaves the specified trigger area.
		/// </summary>
		/// <param name="nameOfArea">The name of the trigger area</param>
		/// <param name="trh">(Shape triggerShape, Shape otherShape, TriggerFlags flags)</param>
		[LuaFunction("hookFunctionToTriggerArea",
			"Hooks up a function to a trigger area event so it will run whenever an actor enters of leaves the specified trigger area.",
			"string nameOfArea - The name of the trigger area",
			"function() trigger report handler - (Shape triggerShape, Shape otherShape, TriggerFlags flags)")]
		public static void HookFunctionToTriggerArea(string nameOfArea, TriggerReportHandler trh) {
			TriggerReporter tr = LKernel.Get<TriggerReporter>();
			if (tr != null)
				tr.AddEvent(/*tr.GetShapeFromName(*/nameOfArea/*)*/, trh);
		}

		// not sure if I should make wrapper functions for when any actor enters any area
	}
}
