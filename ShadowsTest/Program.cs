using System;
using System.Collections.Generic;
using Mogre;

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
			sceneMgr.AmbientLight = ColourValue.Black;
			sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_TEXTURE_ADDITIVE;
			sceneMgr.SetShadowTextureSettings(1024, 3);
			//sceneMgr.ShowDebugShadows = true;

			camera = sceneMgr.CreateCamera("cam");
			camera.Position = new Vector3(100, 100, 100);
			camera.LookAt(new Vector3(0, 0, 0));
			camera.NearClipDistance = 1;
			camera.FarClipDistance = 1000;

			viewport = window.AddViewport(camera);
			viewport.BackgroundColour = ColourValue.Black;
			camera.AspectRatio = (float) viewport.ActualWidth / (float) viewport.ActualHeight;

			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

			TextureManager.Singleton.DefaultNumMipmaps = 1;

			root.FrameStarted += FrameStarted;
			root.FrameEnded += FrameEnded;

			CreateThings();
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

		/// <summary>
		/// create a bunch of crap
		/// </summary>
		void CreateThings() {
			// a directional light to cast shadows
			Light light = sceneMgr.CreateLight("sun");
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(0.5f, -1, -0.2f);
			light.Direction.Normalise();
			light.DiffuseColour = new ColourValue(0.4f, 0.4f, 0.4f);
			light.SpecularColour = new ColourValue(0.4f, 0.4f, 0.4f);
			light.CastShadows = true;

			// and a point light
			Light pointLight = sceneMgr.CreateLight("pointLight");
			pointLight.Type = Light.LightTypes.LT_POINT;
			pointLight.Position = new Vector3(-30, 100, 30);
			pointLight.DiffuseColour = ColourValue.Blue;
			pointLight.SpecularColour = ColourValue.Blue;
			pointLight.CastShadows = true;

			// and a spotlight
			Light spotLight = sceneMgr.CreateLight("spotLight");
			spotLight.Type = Light.LightTypes.LT_SPOTLIGHT;
			spotLight.DiffuseColour = ColourValue.Green;
			spotLight.SpecularColour = ColourValue.Green;
			spotLight.Direction = new Vector3(-1, -1, 0);
			spotLight.Position = new Vector3(100, 100, 0);
			spotLight.SetSpotlightRange(new Degree(35), new Degree(50));
			spotLight.CastShadows = true;

			// a plane for the shadows to be cast on
			Plane plane = new Plane(Vector3.UNIT_Y, 0);
			MeshManager.Singleton.CreatePlane("ground", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, plane, 1000, 1000, 100, 100, true, 1, 10, 10, Vector3.UNIT_Z);
			Entity groundEnt = sceneMgr.CreateEntity("GroundEntity", "ground");
			sceneMgr.RootSceneNode.CreateChildSceneNode().AttachObject(groundEnt);
			groundEnt.SetMaterialName("brick");
			groundEnt.CastShadows = false;

			// and then some boxes that will cast the shadows
			rotatingNode = CreateBox(new Vector3(0, 40, 0), "redbrick");
			CreateBox(new Vector3(20, 11, 20), "bluebrick");
			CreateBox(new Vector3(-10, 20, -30), "yellowbrick");
		}

		SceneNode CreateBox(Vector3 pos, string material = "brick") {
			SceneNode boxNode = sceneMgr.RootSceneNode.CreateChildSceneNode();
			Entity ent = sceneMgr.CreateEntity("kartchassis.mesh");
			ent.CastShadows = true;
			boxNode.AttachObject(ent);
			boxNode.Position = pos;
			boxNode.SetScale(5, 5, 5);
			ent.SetMaterialName(material);

			return boxNode;
		}

		void Start() {
			root.StartRendering();
		}

		readonly Quaternion rotQuat = new Quaternion(new Degree(0.5f), Vector3.UNIT_Y);

		bool FrameStarted(FrameEvent evt) {
			if (window.IsClosed)
				return false;

			// rotate our box
			rotatingNode.Rotate(rotQuat);

			return true;
		}
		bool FrameEnded(FrameEvent evt) {
			return true;
		}

		public void Dispose() {
			root.Shutdown();
			root.Dispose();
		}
	}
}
