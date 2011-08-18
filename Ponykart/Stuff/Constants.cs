
namespace Ponykart {
	public static class Constants {
		// ===== Window =====
		/// <summary> The width of the window, in pixels </summary>
		public static readonly uint WINDOW_WIDTH = 1024;
		/// <summary> The height of the window, in pixels </summary>
		public static readonly uint WINDOW_HEIGHT = 768;

		// ===== Scene =====
		/// <summary> How high up is the camera node? </summary>
		public static readonly float CAMERA_NODE_Y_OFFSET = 5f;
		/// <summary> The distance the camera follows the player kart </summary>
		public static readonly float CAMERA_NODE_Z_OFFSET = -20f;
		/// <summary> How high up is the camera target node from the player kart? </summary>
		public static readonly float CAMERA_TARGET_Y_OFFSET = 3f;
		/// <summary> Affects how much we smooth the camera by. 1 means no smoothing, 0 means it doesn't move at all.</summary>
		public static readonly float CAMERA_TIGHTNESS = 0.1f;


		public static readonly int NUMBER_OF_PLAYERS = 8;

		// ===== Input =====
		/// <summary>
		/// How often to check for new input
		/// </summary>
		public static readonly float INPUT_CAPTURE_RATE = 0.1f;

		// ===== Physics =====
		/// <summary>
		/// The desired physics framerate. You generally want it to be higher than the regular ogre FPS otherwise things start vibrating.
		/// </summary>
		public static readonly float PH_FIXED_TIMESTEP = 1f / 120f;
		/// <summary>
		/// see http://bulletphysics.org/mediawiki-1.5.8/index.php/Stepping_The_World
		/// </summary>
		public static readonly int PH_MAX_SUBSTEPS = 15;
		public static readonly float GRAVITY = -35f;
		
		// ===== Settings =====
		/// <summary> Ribbons enable/disable </summary>
		public static bool RIBBONS = true;
		/// <summary> Sounds enable/disable </summary>
		public static bool SOUNDS = true;
		/// <summary> Music enable/disable </summary>
#if DEBUG
		public static bool MUSIC = false;
#else
		// change this when we're doing more public releases
		public static bool MUSIC = true;
#endif
	}
}
