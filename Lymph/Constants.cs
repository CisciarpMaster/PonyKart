using Mogre;

namespace Ponykart {
	public static class Constants {
		// Window 
		/// <summary> The width of the window, in pixels </summary>
		public static readonly uint WINDOW_WIDTH = 1024;
		/// <summary> The height of the window, in pixels </summary>
		public static readonly uint WINDOW_HEIGHT = 768;

		// Scene
		/// <summary> The distance the camera is along the Y axis from the XZ plane </summary>
		public static readonly float CAMERA_DISTANCE = 8f;

		// Input
		/// <summary>
		/// How often to check for new input
		/// </summary>
		public static readonly float INPUT_CAPTURE_RATE = 0.1f;

		// Physics
		/// <summary> The desired framerate. Must be between 60 and 600 </summary>
		public static readonly int PH_FRAMERATE = 60;
		/// <summary> The "maximum" point of the world. The world is defined by two points and a box is created using them. </summary>
		public static readonly Vector3 PH_WORLD_DEFAULT_MAX = new Vector3(1000, 200, 1000);
		/// <summary> The "minimum" point of the world. The world is defined by two points and a box is created using them. </summary>
		public static readonly Vector3 PH_WORLD_DEFAULT_MIN = new Vector3(-1000, -500, -1000);
		
		// Settings
		/// <summary> Ribbons enable/disable </summary>
		public static bool RIBBONS = true;
		/// <summary> Sounds enable/disable </summary>
		public static bool SOUNDS = true;
		/// <summary> Music enable/disable </summary>
#if DEBUG
		public static bool MUSIC = false;
#else
		public static bool MUSIC = true;
#endif
	}
}
