using Ponykart.Core;

namespace Ponykart.Handlers {
	/// <summary>
	/// Makes a kart camera at the beginning of a race level.
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class KartCameraCreator : ILevelHandler {

		public KartCameraCreator() {
			var cam = new PlayerCamera();
			cam.Register();
			cam.MakeActive();
		}

		public void Detach() { }
	}
}
