using System.Collections.Generic;
using System.Collections.ObjectModel;
using BulletSharp;
using LuaNetInterface;
using Mogre;
using Ponykart.Levels;
using Ponykart.Physics;

namespace Ponykart.Lua {
	//[LuaPackage("Triggers", "A wrapper for the TriggerReporter.")]
	[LuaPackage(null, null)]
	public class TriggerWrapper {
		/// <summary>
		/// for cleaning up all of our events we'll have to dispose of
		/// </summary>
		static ICollection<KeyValuePair<TriggerRegion, TriggerReportHandler>> toDispose;

		public TriggerWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);

			toDispose = new Collection<KeyValuePair<TriggerRegion, TriggerReportHandler>>();

			LKernel.Get<LevelManager>().OnLevelUnload += OnLevelUnload;
		}

		/// <summary>
		/// Go through all of the functions we've hooked up to trigger regions and unhook them. We need to do this otherwise the
		/// regions won't be disposed properly.
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			foreach (KeyValuePair<TriggerRegion, TriggerReportHandler> pair in toDispose) {
				pair.Key.OnTrigger -= pair.Value;
			}
		}

		/// <summary>
		/// Shortcut method to add something to the dispose queue.
		/// </summary>
		static void AddToDispose(TriggerRegion region, TriggerReportHandler handler) {
			toDispose.Add(new KeyValuePair<TriggerRegion, TriggerReportHandler>(region, handler));
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
			string name, TriggerReportHandler trh,
			float width, float height, float length,
			float posX, float posY, float posZ,
			float rotX, float rotY, float rotZ)
		{
			var tr = new TriggerRegion(name, new Vector3(posX, posY, posZ), new Vector3(rotX, rotY, rotZ), new BoxShape(new Vector3(width, height, length)));
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
			string name, TriggerReportHandler trh,
			float radius, float height,
			float posX, float posY, float posZ, 
			float rotX, float rotY, float rotZ)
		{
			var tr = new TriggerRegion(name, new Vector3(posX, posY, posZ), new Vector3(rotX, rotY, rotZ), new CapsuleShape(radius, height));
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
			string name, TriggerReportHandler trh,
			float radius,
			float posX, float posY, float posZ)
		{
			var tr = new TriggerRegion(name, new Vector3(posX, posY, posZ), Vector3.ZERO, new SphereShape(radius));
			tr.OnTrigger += trh;
			AddToDispose(tr, trh);
		}

		// TODO: we need to unhook these functions on level unload, otherwise the regions won't be disposed of properly

		/// <summary>
		/// Hooks up a script file to a trigger region event so it will run whenever an actor enters or leaves the specified trigger region.
		/// The problem with this is that I lose the variables from the event so I can't check whether it's an enter or leave event.
		/// Still... might be useful so I'll leave it
		/// </summary>
		/// <param name="nameOfRegion">The name of the trigger region. Well technically it's the actor name of the shape of the trigger region, but eh whatever.</param>
		/// <param name="filePath">The file path of the script you want to run when the event fires. Ex: "media/scripts/example.lua"</param>
		[LuaFunction("hookScriptToTriggerRegion",
			"Hooks up a script file to a trigger region event so it will run whenever an actor enters or leaves the specified trigger region.",
			"string nameOfRegion - The name of the trigger region. Well technically it's the actor name of the shape of the trigger region, but eh whatever.",
			"string filePath - The file path of the script you want to run when the event fires. Ex: \"media/scripts/example.lua\"")]
		public static void HookScriptToTriggerRegion(string nameOfRegion, string filePath) {
			TriggerReporter reporter = LKernel.Get<TriggerReporter>();

			if (reporter != null) {
				TriggerReportHandler handler = (region, otherShape, flags) => LKernel.Get<LuaMain>().DoFile(filePath);
				TriggerRegion tr = reporter.AddEvent(nameOfRegion, handler);
				if (tr != null)
					AddToDispose(tr, handler);
			}
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
		public static void HookFunctionToTriggerRegion(string nameOfRegion, TriggerReportHandler trh) {
			TriggerReporter reporter = LKernel.Get<TriggerReporter>();

			if (reporter != null) {
				TriggerRegion tr = reporter.AddEvent(nameOfRegion, trh);
				if (tr != null)
					AddToDispose(tr, trh);
			}
		}
	}
}
