using System.Collections.Generic;
using BulletSharp;
using LuaNetInterface;
using Mogre;
using Ponykart.Levels;
using Ponykart.Physics;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class TriggerWrapper {
		/// <summary>
		/// for cleaning up all of our events we'll have to dispose of
		/// </summary>
		static IList<KeyValuePair<TriggerRegion, TriggerReportEvent>> toDispose;

		public TriggerWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);

			toDispose = new List<KeyValuePair<TriggerRegion, TriggerReportEvent>>();

			LKernel.GetG<LevelManager>().OnLevelUnload += OnLevelUnload;
		}

		/// <summary>
		/// Go through all of the functions we've hooked up to trigger regions and unhook them. We need to do this otherwise the
		/// regions won't be disposed properly.
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			foreach (KeyValuePair<TriggerRegion, TriggerReportEvent> pair in toDispose) {
				pair.Key.OnTrigger -= pair.Value;
			}
		}

		/// <summary>
		/// Shortcut method to add something to the dispose queue.
		/// </summary>
		static void AddToDispose(TriggerRegion region, TriggerReportEvent handler) {
			toDispose.Add(new KeyValuePair<TriggerRegion, TriggerReportEvent>(region, handler));
		}

		//-----------------------------------------------

		[LuaFunction("isEnterFlag", "Is this flag an enter flag?", "The flag you want to check")]
		public static bool IsEnterFlag(TriggerReportFlags flag) {
			return flag.HasFlag(TriggerReportFlags.Enter);
		}

		[LuaFunction("isLeaveFlag", "Is this flag an leave flag?", "The flag you want to check")]
		public static bool IsLeaveFlag(TriggerReportFlags flag) {
			return flag.HasFlag(TriggerReportFlags.Leave);
		}

		/// <summary>
		/// create a box trigger region
		/// </summary>
		[LuaFunction("createBoxTriggerRegion", "Creates a box trigger region given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerRegion, otherShape, triggerFlags)",
			"number width", "number height", "number length", "number posX", "number posY", "number posZ", "number rotX", "number rotY", "number rotZ")]
		public static void CreateBoxTriggerRegion(
			string name, TriggerReportEvent trh,
			float width, float height, float length,
			float posX, float posY, float posZ,
			float rotX, float rotY, float rotZ)
		{
			var tr = new TriggerRegion(name, new Vector3(posX, posY, posZ), new Vector3(rotX, rotY, rotZ).DegreeVectorToGlobalQuaternion(), new BoxShape(new Vector3(width, height, length)));
			tr.OnTrigger += trh;
			AddToDispose(tr, trh);
		}

		/// <summary>
		/// create a capsule trigger region
		/// </summary>
		[LuaFunction("createCapsuleTriggerRegion", "Creates a capsule trigger region given a name and some info and a function to call.", 
			"string name - The name of the shape", "function() trigger report handler - (triggerRegion, otherShape, triggerFlags)",
			"number radius", "number height", "number posX", "number posY", "number posZ", "number rotX", "number rotY", "number rotZ")]
		public static void CreateCapsuleTriggerRegion(
			string name, TriggerReportEvent trh,
			float radius, float height,
			float posX, float posY, float posZ, 
			float rotX, float rotY, float rotZ)
		{
			var tr = new TriggerRegion(name, new Vector3(posX, posY, posZ), new Vector3(rotX, rotY, rotZ).DegreeVectorToGlobalQuaternion(), new CapsuleShape(radius, height));
			tr.OnTrigger += trh;
			AddToDispose(tr, trh);
		}

		/// <summary>
		/// create a sphere trigger region
		/// </summary>
		[LuaFunction("createSphereTriggerRegion", "Creates a sphere trigger region given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerRegion, otherShape, triggerFlags)",
			"number radius", "number posX", "number posY", "number posZ")]
		public static void CreateSphereTriggerRegion(
			string name, TriggerReportEvent trh,
			float radius,
			float posX, float posY, float posZ)
		{
			var tr = new TriggerRegion(name, new Vector3(posX, posY, posZ), new SphereShape(radius));
			tr.OnTrigger += trh;
			AddToDispose(tr, trh);
		}

		/// <summary>
		/// Hooks up a function to a triggerregion event so it will run whenever an actor enters of leaves the specified trigger Region.
		/// </summary>
		/// <param name="nameOfRegion">The name of the trigger region</param>
		/// <param name="trh">(Shape triggerShape, Shape otherShape, TriggerFlags flags)</param>
		[LuaFunction("hookFunctionToTriggerRegion",
			"Hooks up a function to a trigger region event so it will run whenever an actor enters of leaves the specified trigger region.",
			"string nameOfRegion - The name of the trigger region",
			"function() trigger report handler - (Shape triggerShape, Shape otherShape, TriggerFlags flags)")]
		public static void HookFunctionToTriggerRegion(string nameOfRegion, TriggerReportEvent trh) {
			TriggerRegion tr = LKernel.Get<TriggerReporter>().AddEvent(nameOfRegion, trh);
			if (tr != null)
				AddToDispose(tr, trh);
		}
	}
}
