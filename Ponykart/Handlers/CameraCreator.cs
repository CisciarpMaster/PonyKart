using Ponykart.Core;

namespace Ponykart.Handlers {
	/// <summary>
	/// Makes a player camera at the beginning of a race level.
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class PlayerCameraCreator : ILevelHandler {

		public PlayerCameraCreator() {
			var cam = new PlayerCamera("PlayerCamera");
			cam.Register();
			cam.MakeActiveCamera();
		}

		public void Detach() { }
	}

	/// <summary>
	/// Makes a free camera at the beginning of a race level.
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class FreeCameraCreator : ILevelHandler {

		public FreeCameraCreator() {
			var cam = new FreeCamera("FreeCamera");
			cam.Register();
		}

		public void Detach() { }
	}
}
