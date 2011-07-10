using System.Collections.Generic;
using System.Linq;
using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.Common.Resources;
using Miyagi.UI;

namespace Lymph.UI {
	/// <summary>
	/// this just has a few methods to load resources for the UI. Everything is static so you shouldn't need to instantiate this class
	/// </summary>
	public class UIResources {
		/// <summary>
		/// Our dictionary of fonts
		/// </summary>
		public static Dictionary<string, Font> Fonts { get; private set; }
		/// <summary>
		/// Our dictionary of skins
		/// </summary>
		public static Dictionary<string, Skin> Skins { get; private set; }

		/// <summary>
		/// Creates the fonts and the skins we will use
		/// </summary>
		public static void Create(MiyagiSystem system) {
			CreateFonts(system);
			CreateSkins();
		}

		/// <summary>
		/// Creates all the fonts
		/// </summary>
		private static void CreateFonts(MiyagiSystem system) {
			var fonts = new[]
				{
					// load ttf definitions from xml file
					TrueTypeFont.CreateFromXml(Settings.Default.MiyagiResources_file_location + @"Fonts/TrueTypeFonts.xml", system)
						.Cast<Font>().ToDictionary(f => f.Name),
					// load image font definitions from xml file
					ImageFont.CreateFromXml(Settings.Default.MiyagiResources_file_location + @"Fonts/ImageFonts.xml", system)
						.Cast<Font>().ToDictionary(f => f.Name)
				};

			Fonts = fonts.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value);

			// set BlueHighway as default font
			Font.Default = Fonts["BlueHighway"];
		}

		/// <summary>
		/// Creates all the skins we will use
		/// </summary>
		private static void CreateSkins() {
			// auto create Skin

			var skins = new List<Skin>();

			skins.AddRange(Skin.CreateFromXml(Settings.Default.MiyagiResources_file_location + @"GUI/Skins.xml"));
			skins.AddRange(Skin.CreateFromXml(Settings.Default.MiyagiResources_file_location + @"Cursor/CursorSkin.xml"));

			// manually create Skins
			var logo = new Skin("Logo");
			var rect = RectangleF.FromLTRB(0, 0, 1, 1);
			var frame1 = new TextureFrame("Logo1.png", rect, 1000);
			var frame2 = new TextureFrame("Logo2.png", rect, 800);
			var frame3 = new TextureFrame("Logo3.png", rect, 600);
			var frame4 = new TextureFrame("Logo4.png", rect, 400);
			var frame5 = new TextureFrame("Logo5.png", rect, 200);

			logo.SubSkins["Logo"] = new Texture(frame1, frame2, frame3, frame4, frame5) {
				FrameAnimationMode = FrameAnimationMode.ForwardBackwardLoop
			};

			skins.Add(logo);

			// done

			Skins = skins.ToDictionary(s => s.Name);
		}

		/// <summary>
		/// Creates the cursor
		/// </summary>
		/// <param name="guiMgr">MiyagiSystem.GUIManager</param>
		public static void CreateCursor(GUIManager guiMgr) {
			guiMgr.Cursor = new Cursor(Skins["CursorSkin"], new Size(16, 16), Point.Empty);
			guiMgr.Cursor.SetHotspot(CursorMode.ResizeLeft, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.ResizeTop, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.ResizeTopLeft, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.ResizeTopRight, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.TextInput, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.BlockDrop, new Point(8, 8));
		}
	}
}