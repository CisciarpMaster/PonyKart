using Ponykart.Core;

namespace Ponykart.Handlers {
	/// <summary>
	/// Makes a player camera at the beginning of a race level.
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class CameraCreatorHandler : ILevelHandler {

		public CameraCreatorHandler() {
			var pcam = new PlayerCamera("PlayerCamera");
			pcam.Register();
			pcam.MakeActiveCamera();

			var fcam = new FreeCamera("FreeCamera");
			fcam.Register();

			var kcam = new KnightyCamera("KnightyCamera");
			kcam.Register();
		}

		public void Detach() { }
	}
}
