
namespace Ponykart {
	public static class Constants {
		// Window 
		/// <summary> The width of the window, in pixels </summary>
		public static readonly uint WINDOW_WIDTH = 1024;
		/// <summary> The height of the window, in pixels </summary>
		public static readonly uint WINDOW_HEIGHT = 768;

		// Scene
		/// <summary> The distance the camera is along the Y axis from the XZ plane </summary>
		public static readonly float CAMERA_HEIGHT = 4f;
		/// <summary> The distance the camera follows the player kart </summary>
		public static readonly float CAMERA_DISTANCE = 16f;


		public static readonly int NUMBER_OF_PLAYERS = 8;

		// Input
		/// <summary>
		/// How often to check for new input
		/// </summary>
		public static readonly float INPUT_CAPTURE_RATE = 0.1f;

		// Physics
		/// <summary> The desired framerate. Must be between 60 and 600 </summary>
		public static readonly int PH_FRAMERATE = 60;
		public static readonly float GRAVITY = -35f;
		
		// Settings
		/// <summary> Ribbons enable/disable </summary>
		public static bool RIBBONS = true;
		/// <summary> Sounds enable/disable </summary>
		public static bool SOUNDS = true;
		/// <summary> Music enable/disable </summary>
#if DEBUG
		public static bool MUSIC = false;
#else
		// change this when we're doing more public releases
		public static bool MUSIC = false; //true;
#endif
	}
}
