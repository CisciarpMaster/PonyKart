using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Lymph {
	public static partial class LKernel {
		private static IDictionary<Type, object> GlobalObjects;
		private static IDictionary<Type, object> LevelObjects;

		/// <summary>
		/// This should be the very first thing that Launch.cs runs
		/// </summary>
		public static void Initialise() {
			GlobalObjects = new Dictionary<Type, object>();
			LevelObjects = new Dictionary<Type, object>();

			var main = new Main();
			AddGlobalObject<Main>(main);
			AddGlobalObject<Form>(main);
		}

		/// <summary>
		/// Gets an object from the global dictionary
		/// </summary>
		/// <typeparam name="T">The type of the object you want to get</typeparam>
		/// <returns></returns>
		public static T GetGlobalObject<T>() {
			object value = null;
			GlobalObjects.TryGetValue(typeof(T), out value);
			return (T)value;
		}

		/// <summary>
		/// Gets an object from the level dictionary
		/// </summary>
		/// <typeparam name="T">The type of the object you want to get</typeparam>
		/// <returns></returns>
		public static T GetLevelObject<T>() {
			object value = null;
			LevelObjects.TryGetValue(typeof(T), out value);
			return (T)value;
		}

		/// <summary>
		/// Gets an object from either dictionary. More useful.
		/// </summary>
		/// <typeparam name="T">The type of the object you want to get</typeparam>
		/// <returns></returns>
		public static T Get<T>() {
			var obj = GetLevelObject<T>();
			if (obj == null)
				obj = GetGlobalObject<T>();
			if (obj == null)
				throw new ArgumentException("This class (" + typeof(T) + ") is not registered to the kernel!", typeof(T).ToString());
			return obj;
		}

		/// <summary>
		/// Adds an object to the global dictionary
		/// </summary>
		/// <typeparam name="T">You don't need to specify this unless you're adding an object to a different type, such
		/// as adding a class to an interface type. "AddGlobalObject&lt;LevelManager&gt;(LevelManager)"</typeparam>
		/// <param name="obj"></param>
		/// <returns>Returns the object you add, for convenience</returns>
		public static T AddGlobalObject<T>(T obj) {
			var type = typeof(T);
			if (GlobalObjects.ContainsKey(type))
				throw new InvalidOperationException("Global object already added " + type.ToString());

			GlobalObjects.Add(type, obj);
			return obj;
		}

		/// <summary>
		/// Adds an object to the level dictionary
		/// </summary>
		/// <typeparam name="T">You don't need to specify this unless you're adding an object to a different type, such
		/// as adding a class to an interface type. "AddLevelObject&lt;LevelManager&gt;(LevelManager)"</typeparam>
		/// <param name="obj"></param>
		/// <returns>Returns the object you add, for convenience</returns>
		public static T AddLevelObject<T>(T obj) {
			var type = typeof(T);
			if (LevelObjects.ContainsKey(type))
				throw new InvalidOperationException("Level object already added " + type.ToString());

			LevelObjects.Add(type, obj);
			return obj;
		}
	}
}
