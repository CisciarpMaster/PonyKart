using System;
using System.Collections.Generic;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.Core {
	public delegate void CameraEvent(LCamera cam);

	/// <summary>
	/// Manages all of our cameras and handles switching between them as necessary.
	/// </summary>
	public class CameraManager {
		IList<LCamera> cameras;
		/// <summary>
		/// Gets the current camera that is being used for rendering.
		/// </summary>
		public LCamera CurrentCamera { get; private set; }

		/// <summary>
		/// Is fired when we switch between cameras.
		/// </summary>
		public static event CameraEvent OnCameraSwitch;
		/// <summary>
		/// Is fired when we register a new camera.
		/// </summary>
		public static event CameraEvent OnCameraRegistration;


		public CameraManager() {
			cameras = new List<LCamera>();

			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);


		}

		/// <summary>
		/// Disposes all of our cameras and clears our list
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			foreach (LCamera cam in cameras) {
				cam.Dispose();
			}
			cameras.Clear();
			CurrentCamera = null;
		}

		/// <summary>
		/// Creates a new basic camera for the viewport to use temporarily while we set everything else up
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			BasicCamera basicCamera = new BasicCamera();
			cameras.Add(basicCamera);

			SwitchCurrentCamera(basicCamera);
		}

		/// <summary>
		/// Switch rendering to another camera. This camera must've already been created and registered.
		/// </summary>
		public void SwitchCurrentCamera(LCamera newCamera) {
			if (cameras.Contains(newCamera)) {
				CurrentCamera = newCamera;
				LKernel.GetG<Viewport>().Camera = newCamera.Camera;

				if (OnCameraSwitch != null)
					OnCameraSwitch(newCamera);
			}
			else {
				throw new ApplicationException("Tried to switch to a camera that wasn't registered to the CameraManager!");
			}
		}

		/// <summary>
		/// Registers a new camera. This camera must not've already been registered.
		/// If we aren't using a camera yet, this will switch our rendering to use it.
		/// </summary>
		/// <param name="newCamera"></param>
		public void RegisterCamera(LCamera newCamera) {
			if (cameras.Contains(newCamera)) {
				throw new ApplicationException("Tried to register a camera that was already registered!");
			}
			else {
				cameras.Add(newCamera);

				if (CurrentCamera == null)
					SwitchCurrentCamera(newCamera);

				if (OnCameraRegistration != null)
					OnCameraRegistration(newCamera);
			}
		}
	}


}
