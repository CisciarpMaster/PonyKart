using System;
using System.Collections.Generic;
using Mogre;
using MOIS;
using Vector3 = Mogre.Vector3;

namespace ShadowsTest {
	/// <summary>
	/// A little program to find what I'm doing wrong with texture shadows
	/// </summary>
	public class Program : IDisposable {
		Root root;
		RenderSystem renderSystem;
		RenderWindow window;
		SceneManager sceneMgr;
		Camera camera;
		Viewport viewport;
		SceneNode rotatingNode;
		InputManager InputManager;
		Keyboard InputKeyboard;
		Light directionalLight, directionalLight2, pointLight, spotLight;
		bool quit = false;

		static void Main(string[] args) {
			using (Program p = new Program()) {
				p.Start();
			}
		}

		/// <summary>
		/// set up ogre
		/// </summary>
		public Program() {
			root = new Root("plugins.cfg", "", "Ogre.log");

			renderSystem = root.GetRenderSystemByName("Direct3D9 Rendering Subsystem");
			renderSystem.SetConfigOption("Full Screen", "No");
			renderSystem.SetConfigOption("Video Mode", "800 x 600 @ 32-bit colour");
			root.RenderSystem = renderSystem;

			SetupResources();
			window = root.Initialise(true, "shadow test");

			sceneMgr = root.CreateSceneManager(SceneType.ST_GENERIC, "sceneMgr");
			sceneMgr.AmbientLight = new ColourValue(0.3f, 0.3f, 0.3f);


			camera = sceneMgr.CreateCamera("cam");
			camera.Position = new Vector3(12, 12, 12);
			camera.LookAt(new Vector3(0, 0, 0));
			camera.SetAutoTracking(true, sceneMgr.RootSceneNode);
			camera.NearClipDistance = 0.1f;
			camera.FarClipDistance = 2000;

			viewport = window.AddViewport(camera);
			viewport.BackgroundColour = ColourValue.Black;
			camera.AspectRatio = (float) viewport.ActualWidth / (float) viewport.ActualHeight;

			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

			TextureManager.Singleton.DefaultNumMipmaps = 1;

			CreateThings();

			SetupShadows();

			SetupInput();

			root.FrameStarted += FrameStarted;

			Console.WriteLine(
@"



Press 1, 2, 3, 4 to enable/disable lights, or Esc to quit.
The red and blue textures have PSSM and self-shadowing enabled.
The yellow one does not."
			);
		}

		/// <summary>
		/// set up our resources file
		/// </summary>
		void SetupResources() {
			ConfigFile file = new ConfigFile();
			file.Load("resources.cfg", "\t:=", true);
			ConfigFile.SectionIterator sectionIterator = file.GetSectionIterator();

			while (sectionIterator.MoveNext()) {
				string currentKey = sectionIterator.CurrentKey;
				foreach (KeyValuePair<string, string> pair in sectionIterator.Current) {
					string key = pair.Key;
					string name = pair.Value;
					ResourceGroupManager.Singleton.AddResourceLocation(name, key, currentKey);
				}
			}
		}

		void SetupShadows() {
			sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_TEXTURE_ADDITIVE_INTEGRATED;

			sceneMgr.SetShadowTextureCountPerLightType(Light.LightTypes.LT_DIRECTIONAL, 3);
			sceneMgr.ShadowTextureCount = 3;
			sceneMgr.SetShadowTextureConfig(0, 1024, 1024, PixelFormat.PF_FLOAT32_R);
			sceneMgr.SetShadowTextureConfig(1, 512, 512, PixelFormat.PF_FLOAT32_R);
			sceneMgr.SetShadowTextureConfig(2, 512, 512, PixelFormat.PF_FLOAT32_R);
			sceneMgr.ShadowTextureSelfShadow = true;
			sceneMgr.ShadowCasterRenderBackFaces = false;
			sceneMgr.ShadowFarDistance = 150;
			sceneMgr.SetShadowTextureCasterMaterial("PSSM/shadow_caster");
			sceneMgr.SetShadowTextureFadeStart(0.3f);

			PSSMShadowCameraSetup pssm = new PSSMShadowCameraSetup();
			pssm.SplitPadding = 1f;
			pssm.CalculateSplitPoints(3, 0.0000001f, sceneMgr.ShadowFarDistance);
			pssm.SetOptimalAdjustFactor(0, 2);
			pssm.SetOptimalAdjustFactor(1, 1);
			pssm.SetOptimalAdjustFactor(2, 0.5f);
			pssm.UseSimpleOptimalAdjust = false;

			sceneMgr.SetShadowCameraSetup(new ShadowCameraSetupPtr(pssm));
		}

		/// <summary>
		/// create a bunch of crap
		/// </summary>
		void CreateThings() {
			// a directional light to cast shadows
			directionalLight = sceneMgr.CreateLight("sun");
			directionalLight.Type = Light.LightTypes.LT_DIRECTIONAL;
			directionalLight.Direction = new Vector3(0.5f, -1, -0.2f);
			directionalLight.Direction.Normalise();
			directionalLight.DiffuseColour = ColourValue.Red;
			directionalLight.SpecularColour = ColourValue.Red;
			directionalLight.Position = new Vector3(0, 10, 0);
			directionalLight.CastShadows = true;

			directionalLight2 = sceneMgr.CreateLight("sun2");
			directionalLight2.Type = Light.LightTypes.LT_DIRECTIONAL;
			directionalLight2.Direction = new Vector3(-0.1f, -1, 0.1f);
			directionalLight2.Direction.Normalise();
			directionalLight2.DiffuseColour = new ColourValue(0.7f, 0.7f, 0.7f);
			directionalLight2.SpecularColour = new ColourValue(0.7f, 0.7f, 0.7f);
			directionalLight.Position = new Vector3(0, 10, 0);
			directionalLight2.CastShadows = true;

			// and a point light
			pointLight = sceneMgr.CreateLight("pointLight");
			pointLight.Type = Light.LightTypes.LT_POINT;
			pointLight.Position = new Vector3(-3, 10, 3);
			pointLight.DiffuseColour = ColourValue.Blue;
			pointLight.SpecularColour = ColourValue.Blue;
			pointLight.CastShadows = true;

			// and a spotlight
			spotLight = sceneMgr.CreateLight("spotLight");
			spotLight.Type = Light.LightTypes.LT_SPOTLIGHT;
			spotLight.DiffuseColour = ColourValue.Green;
			spotLight.SpecularColour = ColourValue.Green;
			spotLight.Direction = new Vector3(-1, -1, 0);
			spotLight.Position = new Vector3(10, 10, 0);
			spotLight.SetSpotlightRange(new Degree(35), new Degree(50));
			spotLight.CastShadows = true;

			// a plane for the shadows to be cast on
			Plane plane = new Plane(Vector3.UNIT_Y, 0);
			MeshManager.Singleton.CreatePlane("ground", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, plane,
				100, 100, 10, 10, true, 1, 10, 10, Vector3.UNIT_Z);
			Entity groundEnt = sceneMgr.CreateEntity("GroundEntity", "ground");
			sceneMgr.RootSceneNode.CreateChildSceneNode().AttachObject(groundEnt);
			groundEnt.SetMaterialName("brick");
			groundEnt.CastShadows = false;

			// and then some boxes that will cast the shadows
			rotatingNode = CreateBox(new Vector3(0, 4, 0), "redbrick");
			CreateBox(new Vector3(2, 1.1f, 2), "bluebrick");
			CreateBox(new Vector3(-1, 2, -3), "yellowbrick");
		}

		SceneNode CreateBox(Vector3 pos, string material = "brick") {
			SceneNode boxNode = sceneMgr.RootSceneNode.CreateChildSceneNode();
			Entity ent = sceneMgr.CreateEntity("kartchassis.mesh");
			ent.CastShadows = true;
			boxNode.AttachObject(ent);
			boxNode.Position = pos;
			ent.SetMaterialName(material);

			return boxNode;
		}

		void SetupInput() {
			ParamList pl = new ParamList();
			IntPtr windowHnd;
			window.GetCustomAttribute("WINDOW", out windowHnd); // window is your RenderWindow!
			pl.Insert("WINDOW", windowHnd.ToString());

			InputManager = InputManager.CreateInputSystem(pl);
			InputKeyboard = (Keyboard) InputManager.CreateInputObject(MOIS.Type.OISKeyboard, true);

			InputKeyboard.KeyPressed += new KeyListener.KeyPressedHandler(InputKeyboard_KeyPressed);
		}

		bool InputKeyboard_KeyPressed(KeyEvent arg) {
			Console.WriteLine(arg.key);

			switch (arg.key) {
				case KeyCode.KC_ESCAPE:
					quit = true;
					break;
				case KeyCode.KC_1:
					directionalLight.Visible = !directionalLight.Visible;
					break;
				case KeyCode.KC_2:
					pointLight.Visible = !pointLight.Visible;
					break;
				case KeyCode.KC_3:
					spotLight.Visible = !spotLight.Visible;
					break;
				case KeyCode.KC_4:
					directionalLight2.Visible = !directionalLight2.Visible;
					break;
				case KeyCode.KC_W:
					camera.Position += new Vector3(1, 0, 0);
					break;
				case KeyCode.KC_S:
					camera.Position -= new Vector3(1, 0, 0);
					break;
				case KeyCode.KC_A:
					camera.Position += new Vector3(0, 0, 1);
					break;
				case KeyCode.KC_D:
					camera.Position -= new Vector3(0, 0, 1);
					break;
				case KeyCode.KC_Q:
					camera.Position += new Vector3(0, 1, 0);
					break;
				case KeyCode.KC_E:
					camera.Position -= new Vector3(0, 1, 0);
					break;
			}

			return !quit;
		}

		void Start() {
			root.StartRendering();
		}

		readonly Quaternion rotQuat = new Quaternion(new Degree(0.5f), Vector3.UNIT_Y);

		bool FrameStarted(FrameEvent evt) {
			if (quit || window.IsClosed)
				return false;

			InputKeyboard.Capture();

			// rotate our box
			rotatingNode.Rotate(rotQuat);

			return !quit;
		}

		public void Dispose() {
			root.Shutdown();
			root.Dispose();
		}
	}
}
