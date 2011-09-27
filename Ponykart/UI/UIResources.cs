using System.Collections.Generic;
using System.Linq;
using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.Common.Resources;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Properties;

namespace Ponykart.UI {
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

			CreateFromSerialized(system);
		}

		/// <summary>
		/// Creates all the fonts
		/// </summary>
		private static void CreateFonts(MiyagiSystem system) {
			var fonts = new[]
				{
					// load ttf definitions from xml file
					TrueTypeFont.CreateFromXml(Settings.Default.MiyagiResourcesFileLocation + "Fonts/TrueTypeFonts" + Settings.Default.MiyagiXMLExtension, system)
						.Cast<Font>().ToDictionary(f => f.Name),
					// load image font definitions from xml file
					ImageFont.CreateFromXml(Settings.Default.MiyagiResourcesFileLocation + "Fonts/ImageFonts" + Settings.Default.MiyagiXMLExtension, system)
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

			skins.AddRange(Skin.CreateFromXml(Settings.Default.MiyagiResourcesFileLocation + "GUI/Skins" + Settings.Default.MiyagiXMLExtension));
			skins.AddRange(Skin.CreateFromXml(Settings.Default.MiyagiResourcesFileLocation + "GUI/PonykartSkins" + Settings.Default.MiyagiXMLExtension));
			skins.AddRange(Skin.CreateFromXml(Settings.Default.MiyagiResourcesFileLocation + "Cursor/CursorSkin" + Settings.Default.MiyagiXMLExtension));

			// done
			Skins = skins.ToDictionary(s => s.Name);
		}

		/// <summary>
		/// Loads up our GUI from an xml file
		/// </summary>
		private static void CreateFromSerialized(MiyagiSystem system) {
			system.SerializationManager.ImportFromFile(Settings.Default.MiyagiResourcesFileLocation + "serialize" + Settings.Default.MiyagiXMLExtension);

			// the XML only gives us SkinNames and FontNames, so now we have to get them to all use the correct skins/fonts
			foreach (var control in system.GUIManager.AllControls) {
				SkinnedControl sc = control as SkinnedControl;
				if (sc != null) {
					sc.Skin = Skins[sc.SkinName];
				}

				Label l = control as Label;
				if (l != null) {
					l.TextStyle.Font = Fonts[l.TextStyle.FontName];
				}
			}
		}

		/// <summary>
		/// Creates the cursor
		/// </summary>
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