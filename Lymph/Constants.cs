using Mogre;

namespace Lymph {
	public static class Constants {
		// Window 
		/// <summary> The width of the window, in pixels </summary>
		public static readonly uint WINDOW_WIDTH = 1024;
		/// <summary> The height of the window, in pixels </summary>
		public static readonly uint WINDOW_HEIGHT = 768;

		// Scene
		/// <summary> The distance the camera is along the Y axis from the XZ plane </summary>
		public static readonly float CAMERA_DISTANCE = 8f;
		/// <summary> The scale of the background cells </summary>
		//public static readonly float BACKGROUND_CELL_SCALE = 5f;
		/// <summary> The offset of the background cells along the Y axis from the origin </summary>
		//public static readonly float BACKGROUND_CELL_OFFSET = -400f;

		// Input
		/// <summary>
		/// How often to check for new input
		/// </summary>
		public static readonly float INPUT_CAPTURE_RATE = 0.1f;

		// Physics
		/// <summary> The desired framerate. Must be between 60 and 600 </summary>
		public static readonly int PH_FRAMERATE = 60;
		/// <summary> Uhh... kinda hard to explain but it has something to do with rotation resistance I think </summary>
		public static readonly float PH_INERTIA = 50f;
		/// <summary> The "maximum" point of the world. The world is defined by two points and a box is created using them. </summary>
		public static readonly Vector3 PH_WORLD_DEFAULT_MAX = new Vector3(1000, 200, 1000);
		/// <summary> The "minimum" point of the world. The world is defined by two points and a box is created using them. </summary>
		public static readonly Vector3 PH_WORLD_DEFAULT_MIN = new Vector3(-1000, -500, -1000);
		
		// Enemies
		//public static readonly float PH_COCCI_RADIUS = 9f;
		/// <summary> The maximum number of antibodies that can attach to a cell </summary>
#if DEBUG
		public static readonly int MAX_ANTIBODY_ATTACHMENTS = 1000;
#else
		public static readonly int MAX_ANTIBODY_ATTACHMENTS = 10;
#endif
		
		// Antibody stuff
		/// <summary> Time that an antibody has to attach to something before it disappears </summary>
		//public static readonly int FLOATING = 5000; //ms
		/// <summary> Time that an antibody will stick to a cell for. (necessary?) </summary>
		//public static readonly int ATTACHED = 10000;
		
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
		public static bool TRIPPY = false;
	}
}
