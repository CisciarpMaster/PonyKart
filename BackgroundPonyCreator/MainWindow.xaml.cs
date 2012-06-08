using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mogre;
using MogreInWpf;
//using MOIS;
using Color = System.Drawing.Color;
using Size = System.Windows.Size;
using Vector3 = Mogre.Vector3;

namespace BackgroundPonyCreator {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static bool AreWeDoneLoading = false;
		CultureInfo culture = CultureInfo.InvariantCulture;
		bool quit = false;

		public readonly bool InitMogreAsync = true;
		public static readonly Size StartupViewportSize = new Size(400, 300);
		public readonly bool AutoUpdateViewportSize = true;

		Camera camera;

		//InputManager InputManager;
		//Keyboard InputKeyboard;
		Root root;
		SceneManager sceneMgr;

		SceneNode ponyNode;
		Entity ponyEnt, wingsEnt, hornEnt, foldedWingsEnt, eyesEnt;
		Entity[] hairEnts, maneEnts, tailEnts;
		int activeHairstyle = 0;

		AnimationState blinkState;
		AnimationState animState;
		AnimationState wingsState;
		AnimationState[] maneStates, tailStates;
		string currentAnimation;

		ColourValue bodyColour = new ColourValue(0.707f, 0.688f, 0.902f),
			bodyAOColour = new ColourValue(0.445f, 0.465f, 0.758f),
			eyeColour1 = new ColourValue(0.387f, 0.254f, 0.387f),
			eyeColour2 = new ColourValue(0.820f, 0.609f, 0.707f),
			eyeHighlightColour = new ColourValue(0.981f, 0.871f, 0.871f),
			hairColour1 = new ColourValue(0.934f, 0.777f, 0.449f),
			hairAOColour1 = new ColourValue(0.805f, 0.543f, 0.16f),
			hairColour2 = new ColourValue(0.917f, 0.902f, 0.563f),
			hairAOColour2 = new ColourValue(0.844f, 0.641f, 0.246f);

		MogreImage mogreImageSource;
		
		// ---------------------------------------

		public MainWindow() {
			AreWeDoneLoading = false;

			// create the window and everything
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(MainWindow_Loaded);

			// set the window's icon to our resource
			MemoryStream iconStream = new MemoryStream();
			Properties.Resources.Icon_1.Save(iconStream);
			iconStream.Seek(0, SeekOrigin.Begin);
			this.Icon = BitmapFrame.Create(iconStream);

			// okay, done initialising
			AreWeDoneLoading = true;
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			InitializeImage();

			if (InitMogreAsync) {
				mogreImageSource.InitOgreAsync(System.Threading.ThreadPriority.Normal, new RoutedEventHandler(OnOgreInitComplete));
			}
			else {
				mogreImageSource.InitOgreImage();

				OnOgreInitComplete(null, null);
			}
		}

		private void OnOgreInitComplete(object sender, RoutedEventArgs args) {
			if (AutoUpdateViewportSize) {
				MogreImage.SizeChanged += new SizeChangedEventHandler(MogreImage_SizeChanged);
			}
			mogreImageSource.IsDebugOverlayVisible = true;

			//-----------------------------------------------------------------------------

			TextureManager.Singleton.DefaultNumMipmaps = 6;

			root = MogreRootManager.GetSharedRoot();
			sceneMgr = mogreImageSource.SceneManager = root.CreateSceneManager(SceneType.ST_GENERIC, "sceneMgr");
			sceneMgr.AmbientLight = new ColourValue(0.8f, 0.8f, 0.8f);

			camera = sceneMgr.CreateCamera("cam");
			camera.Position = new Vector3(0.8f, 0.8f, 0.8f);
			camera.LookAt(new Vector3(-1, 1, -1));
			camera.SetAutoTracking(true, sceneMgr.RootSceneNode.CreateChildSceneNode(new Vector3(0, 0.4f, 0)));
			camera.NearClipDistance = 0.1f;
			camera.FarClipDistance = 2000;

			mogreImageSource.ViewportDefinitions = new ViewportDefinition[] { new ViewportDefinition(camera) };

			CreateThings();
			//SetupInput();
			

			mogreImageSource.Root.FrameStarted += FrameStarted;
		}

		private void InitializeImage() {
			mogreImageSource = new MogreImage();
			mogreImageSource.CreateDefaultScene = false;
			mogreImageSource.ViewportSize = PreferredMogreViewportSize;
			MogreImage.Source = mogreImageSource;
		}

		public Size PreferredMogreViewportSize {
			get {
				if (MogreImage.ActualHeight == 0 || MogreImage.ActualWidth == 0) {
					return StartupViewportSize;
				}
				return new Size(MogreImage.ActualWidth, MogreImage.ActualHeight);
			}
		}

		void MogreImage_SizeChanged(object sender, SizeChangedEventArgs e) {
			mogreImageSource.ViewportSize = PreferredMogreViewportSize;
		}

		/// <summary>
		/// create a bunch of crap
		/// </summary>
		void CreateThings() {

			Light light = mogreImageSource.SceneManager.CreateLight("sun2");
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(-0.1f, -1, 0.1f);
			light.Direction.Normalise();
			light.DiffuseColour = new ColourValue(1f, 1f, 1f);
			light.SpecularColour = new ColourValue(1f, 1f, 1f);
			light.Position = new Vector3(0, 10, 0);
			light.CastShadows = true;


			ponyNode = sceneMgr.RootSceneNode.CreateChildSceneNode();
			ponyEnt = sceneMgr.CreateEntity("BgPonyBody.mesh");
			ponyNode.AttachObject(ponyEnt);
			ponyNode.Position = new Vector3(0, 0, 0);
			ponyEnt.SetMaterialName("BgPony");

			wingsEnt = sceneMgr.CreateEntity("BgPonyWings.mesh");
			wingsEnt.SetMaterialName("BgPonyWings");

			hornEnt = sceneMgr.CreateEntity("BgPonyHorn.mesh");
			hornEnt.SetMaterialName("BgPonyHorn");

			eyesEnt = sceneMgr.CreateEntity("BgPonyEyes.mesh");
			eyesEnt.SetMaterialName("BgPonyEyes");

			foldedWingsEnt = sceneMgr.CreateEntity("BgPonyWingsFolded.mesh");
			foldedWingsEnt.SetMaterialName("BgPonyWingsFolded");

			hairEnts = new Entity[6];
			maneEnts = new Entity[6];
			tailEnts = new Entity[6];

			for (int a = 0; a < 6; a++) {
				// there is no hair#4
				if (a == 3) {
					hairEnts[3] = hairEnts[2];
					maneEnts[3] = maneEnts[2];
					tailEnts[3] = tailEnts[2];
					continue;
				}
				
				hairEnts[a] = sceneMgr.CreateEntity("BgPonyHair" + (a+1) + ".mesh");
				hairEnts[a].SetMaterialName("BgPonyHair_Double_" + (a+1));

				maneEnts[a] = sceneMgr.CreateEntity("BgPonyMane" + (a+1) + ".mesh");
				maneEnts[a].SetMaterialName("BgPonyHair_Double_" + (a+1));

				tailEnts[a] = sceneMgr.CreateEntity("BgPonyTail" + (a+1) + ".mesh");
				tailEnts[a].SetMaterialName("BgPonyHair_Double_" + (a+1));
			}

			wingsEnt.Visible = false;
			foldedWingsEnt.Visible = false;
			hornEnt.Visible = false;

			for (int a = 1; a < 6; a++) {
				if (a == 3)
					continue;

				hairEnts[a].Visible = false;
				maneEnts[a].Visible = false;
				tailEnts[a].Visible = false;
			}

			AdjustBodyColour(bodyColour);
			AdjustBodyAOColour(bodyAOColour);
			AdjustHairColour1(hairColour1);
			AdjustHairColour2(hairColour2);
			AdjustHairAOColour1(hairAOColour1);
			AdjustHairAOColour2(hairAOColour2);

			// attach stuff

			ponyEnt.AttachObjectToBone("Eyes", eyesEnt);
			ponyEnt.AttachObjectToBone("Horn", hornEnt);
			ponyEnt.AttachObjectToBone("Wings", wingsEnt);
			ponyEnt.AttachObjectToBone("Wings", foldedWingsEnt);
			for (int a = 0; a < 6; a++) {
				if (a == 3) continue;

				ponyEnt.AttachObjectToBone("Hair", hairEnts[a]);
				ponyEnt.AttachObjectToBone("Mane", maneEnts[a]);
				ponyEnt.AttachObjectToBone("Tail", tailEnts[a]);
			}

			// setup animations

			Skeleton skeleton = ponyEnt.Skeleton;
			skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_CUMULATIVE;
			// set up the blink animation state with some stuff
			blinkState = ponyEnt.GetAnimationState("Blink2");
			blinkState.Enabled = true;
			blinkState.Loop = true;
			blinkState.Weight = 1;

			blinkState.CreateBlendMask(skeleton.NumBones, 0f);
			ushort handle = skeleton.GetBone("EyeBrowTop.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowTop.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);

			animState = ponyEnt.GetAnimationState("Stand1");
			wingsState = wingsEnt.GetAnimationState("Flap1");
			maneStates = new AnimationState[6];
			tailStates = new AnimationState[6];
			for (int a = 0; a < 6; a++) {
				if (a == 3) continue;
				maneStates[a] = maneEnts[a].GetAnimationState("Stand1");
				tailStates[a] = tailEnts[a].GetAnimationState("Stand1");
				maneStates[a].Enabled = tailStates[a].Enabled = true;
				maneStates[a].Loop = tailStates[a].Loop = true;
			}
			currentAnimation = "Stand1";

			animState.Enabled = wingsState.Enabled = true;
			animState.Loop = wingsState.Loop = true;
		}

		void PlayAnimation(string animName) {
			currentAnimation = animName;
			{
				AnimationState newState = ponyEnt.GetAnimationState(animName);

				animState.Enabled = false;
				newState.Enabled = true;
				newState.Loop = true;

				animState = newState;
			}
			for (int a = 0; a < 6; a++) {
				if (a == 3) continue;

				{
					AnimationState newState;
					if (maneEnts[a].Skeleton.HasAnimation(animName))
						newState = maneEnts[a].GetAnimationState(animName);
					else
						newState = maneEnts[a].GetAnimationState("Stand1");

					maneStates[a].Enabled = false;
					newState.Enabled = true;
					newState.Loop = true;

					maneStates[a] = newState;
				}
				{
					AnimationState newState;
					if (tailEnts[a].Skeleton.HasAnimation(animName))
						newState = tailEnts[a].GetAnimationState(animName);
					else
						newState = tailEnts[a].GetAnimationState("Stand1");

					tailStates[a].Enabled = false;
					newState.Enabled = true;
					newState.Loop = true;

					tailStates[a] = newState;
				}
			}
		}
		

		/*void SetupInput() {
			ParamList pl = new ParamList();
			IntPtr windowHnd = new WindowInteropHelper(this).Handle;
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
				case KeyCode.KC_W:
					camera.Position += new Vector3(0.1f, 0, 0);
					break;
				case KeyCode.KC_S:
					camera.Position -= new Vector3(0.1f, 0, 0);
					break;
				case KeyCode.KC_A:
					camera.Position += new Vector3(0, 0, 0.1f);
					break;
				case KeyCode.KC_D:
					camera.Position -= new Vector3(0, 0, 0.1f);
					break;
				case KeyCode.KC_Q:
					camera.Position += new Vector3(0, 0.1f, 0);
					break;
				case KeyCode.KC_E:
					camera.Position -= new Vector3(0, 0.1f, 0);
					break;
			}

			return !quit;
		}
		*/

		#region Color adjusting methods
		void AdjustBodyColour(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPony");
			var ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			ps.SetNamedConstant("BodyColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

			mat = MaterialManager.Singleton.GetByName("BgPonyHorn");
			ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			ps.SetNamedConstant("BodyColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

			mat = MaterialManager.Singleton.GetByName("BgPonyWings");
			ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			ps.SetNamedConstant("BodyColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

			mat = MaterialManager.Singleton.GetByName("BgPonyWingsFolded");
			ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			ps.SetNamedConstant("BodyColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);
		}

		void AdjustBodyAOColour(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPony");
			var psAO = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			psAO.SetNamedConstant("AOColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psAO);

			var psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			psEdge.SetNamedConstant("OutlineColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);


			mat = MaterialManager.Singleton.GetByName("BgPonyHorn");
			psAO = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			psAO.SetNamedConstant("AOColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psAO);

			psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			psEdge.SetNamedConstant("OutlineColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);


			mat = MaterialManager.Singleton.GetByName("BgPonyWings");
			psAO = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			psAO.SetNamedConstant("AOColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psAO);

			psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			psEdge.SetNamedConstant("OutlineColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);


			mat = MaterialManager.Singleton.GetByName("BgPonyWingsFolded");
			psAO = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			psAO.SetNamedConstant("AOColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psAO);

			psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			psEdge.SetNamedConstant("OutlineColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);
		}

		void AdjustEyeColour1(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyEyes");
			var ps = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			ps.SetNamedConstant("TopIrisColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(ps);
		}

		void AdjustEyeColour2(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyEyes");
			var ps = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			ps.SetNamedConstant("BottomIrisColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(ps);
		}

		void AdjustEyeHighlightColour(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyEyes");
			var ps = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			ps.SetNamedConstant("HighlightColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(ps);
		}

		void AdjustHairColour1(ColourValue colour) {
			for (int a = 1; a <= 6; a++) {
				if (a == 4)
					continue;

				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyHair_Single_" + a);
				var ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				ps.SetNamedConstant("HairColour", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

				mat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + a);
				ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				ps.SetNamedConstant("HairColour1", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);
			}
		}

		void AdjustHairAOColour1(ColourValue colour) {
			for (int a = 1; a <= 6; a++) {
				if (a == 4)
					continue;

				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyHair_Single_" + a);
				var psCol = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				psCol.SetNamedConstant("AOColour", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psCol);

				var psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
				psEdge.SetNamedConstant("OutlineColour", colour);
				mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);


				mat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + a);
				psCol = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				psCol.SetNamedConstant("AOColour1", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psCol);
			}
		}

		void AdjustHairColour2(ColourValue colour) {
			for (int a = 1; a <= 6; a++) {
				if (a == 4)
					continue;

				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + a);
				var ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				ps.SetNamedConstant("HairColour2", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);
			}
		}

		void AdjustHairAOColour2(ColourValue colour) {
			for (int a = 1; a <= 6; a++) {
				if (a == 4)
					continue;

				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + a);
				var psCol = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				psCol.SetNamedConstant("AOColour2", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psCol);

				var psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
				psEdge.SetNamedConstant("OutlineColour", colour);
				mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);
			}
		}
		#endregion


		readonly Quaternion rotQuat = new Quaternion(new Degree(0.5f), Vector3.UNIT_Y);
		bool FrameStarted(FrameEvent evt) {
			if (quit)
				return false;

			//InputKeyboard.Capture();

			// rotate our pone
			if (ponyNode != null) {
				ponyNode.Rotate(new Quaternion(new Degree(16.6666f * evt.timeSinceLastFrame), Vector3.UNIT_Y));

				animState.AddTime(evt.timeSinceLastFrame);
				blinkState.AddTime(evt.timeSinceLastFrame);
				wingsState.AddTime(evt.timeSinceLastFrame);
				for (int a = 0; a < 6; a++) {
					if (a == 3) continue;
					maneStates[a].AddTime(evt.timeSinceLastFrame);
					tailStates[a].AddTime(evt.timeSinceLastFrame);
				}
			}

			return !quit;
		}

		protected override void OnClosed(EventArgs e) {
			quit = true;

			base.OnClosed(e);
		}


		#region click color changing buttons
		private void bodyColourButton_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = bodyColour.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				bodyColour = dialog.Color.ToMogre();
				AdjustBodyColour(bodyColour);

				bodyColourButton.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}

		private void bodyShadingButton_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = bodyAOColour.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				bodyAOColour = dialog.Color.ToMogre();
				AdjustBodyAOColour(bodyAOColour);

				bodyShadingButton.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}

		private void eyeColour1Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = eyeColour1.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				eyeColour1 = dialog.Color.ToMogre();
				AdjustEyeColour1(eyeColour1);

				eyeColour1Button.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}

		private void eyeColour2Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = eyeColour2.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				eyeColour2 = dialog.Color.ToMogre();
				AdjustEyeColour2(eyeColour2);

				eyeColour2Button.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}

		private void eyeHighlightButton_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = eyeHighlightColour.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				eyeHighlightColour = dialog.Color.ToMogre();
				AdjustEyeHighlightColour(eyeHighlightColour);

				eyeHighlightButton.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}

		private void hairColour1Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = hairColour1.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				hairColour1 = dialog.Color.ToMogre();
				AdjustHairColour1(hairColour1);

				hairColour1Button.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}

		private void hairAOColour1Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = hairAOColour1.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				hairAOColour1 = dialog.Color.ToMogre();
				AdjustHairAOColour1(hairAOColour1);

				hairAOColour1Button.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}

		private void hairColour2Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = hairColour2.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				hairColour2 = dialog.Color.ToMogre();
				AdjustHairColour2(hairColour2);

				hairColour2Button.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}

		private void hairAOColour2Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = hairAOColour2.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				hairAOColour2 = dialog.Color.ToMogre();
				AdjustHairAOColour2(hairAOColour2);

				hairAOColour2Button.Background = new SolidColorBrush(dialog.Color.ToWindowsMediaColor());
			}
		}
		#endregion

		#region horn and wings visibility
		private void hornCheckBox_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			hornEnt.Visible = true;
		}

		private void hornCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			hornEnt.Visible = false;
		}

		private void wingsCheckBox_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			wingsEnt.Visible = true;
		}

		private void wingsCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			wingsEnt.Visible = false;
		}

		private void foldedWingsCheckBox_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			foldedWingsEnt.Visible = true;
		}

		private void foldedWingsCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			foldedWingsEnt.Visible = false;
		}
		#endregion

		#region one or two hair colors radio buttons
		private void oneHairColourRadioButton_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			hairColour2Button.IsEnabled = false;
			hairAOColour2Button.IsEnabled = false;

			for (int a = 0; a < 6; a++) {
				// there is no hair#4
				if (a == 3)
					continue;

				hairEnts[a].SetMaterialName("BgPonyHair_Single_" + (a + 1));
				maneEnts[a].SetMaterialName("BgPonyHair_Single_" + (a + 1));
				tailEnts[a].SetMaterialName("BgPonyHair_Single_" + (a + 1));
			}
		}

		private void twoHairColoursRadioButton_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			hairColour2Button.IsEnabled = true;
			hairAOColour2Button.IsEnabled = true;

			for (int a = 0; a < 6; a++) {
				// there is no hair#4
				if (a == 3)
					continue;

				hairEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));
				maneEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));
				tailEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));
			}
		}
		#endregion

		/// <summary>
		/// change the visibility of the different hairstyles
		/// </summary>
		private void hairStyleComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0 || !AreWeDoneLoading)
				return;

			ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
			string text = (string) item.Content;

			int hairID = int.Parse("" + text[9]) - 1;

			for (int a = 0; a < 6; a++) {
				if (a == 3) continue;

				if (a == hairID) {
					hairEnts[a].Visible = true;
					maneEnts[a].Visible = true;
					tailEnts[a].Visible = true;
				}
				else {
					hairEnts[a].Visible = false;
					maneEnts[a].Visible = false;
					tailEnts[a].Visible = false;
				}
			}
			activeHairstyle = hairID;
		}

		/// <summary>
		/// Change the cutie mark
		/// </summary>
		private void cutieMarkButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "\u2588\u2588\u2588 MAKE SURE THE IMAGE IS IN THE SAME FOLDER AS THE OTHER CUTIE MARKS! \u2588\u2588\u2588";
			dialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "\\materials";
			dialog.DefaultExt = ".png";
			dialog.Filter = "PNG Images|*.png";
			dialog.Multiselect = false;

			var result = dialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK) {
				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPony");
				mat.GetTechnique(0).GetPass(1).GetTextureUnitState(1).SetTextureName(Path.GetFileName(dialog.FileName));
			}
		}

		private void animationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0 || !AreWeDoneLoading)
				return;

			ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
			string text = (string) item.Content;

			PlayAnimation(text);
		}



	}

	public static class Extensions {
		public static ColourValue ToMogre(this Color col) {
			return new ColourValue(col.R / 256f, col.G / 256f, col.B / 256f);
		}

		public static Color ToWindows(this ColourValue col) {
			return Color.FromArgb((int) (col.r * 256), (int) (col.g * 256), (int) (col.b * 256));
		}

		public static System.Windows.Media.Color ToWindowsMediaColor(this System.Drawing.Color col) {
			return System.Windows.Media.Color.FromRgb(col.R, col.G, col.B);
		}
	}
}
