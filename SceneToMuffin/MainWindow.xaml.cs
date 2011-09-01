using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SceneToThing;

namespace SceneToMuffin {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static bool AreWeDoneLoading = false;
		DotSceneLoader DSL;
		/// <summary>
		/// The data that goes in the table
		/// </summary>
		ICollection<NodeData> Data;
		CultureInfo culture = CultureInfo.InvariantCulture;
		/// <summary>
		/// Original filename we got when we imported the .scene - this is reused when we export
		/// </summary>
		string originalFilename;

		public MainWindow() {
			AreWeDoneLoading = false;

			// create the window and everything
			InitializeComponent();

			// set the window's icon to our resource
			MemoryStream iconStream = new MemoryStream();
			Properties.Resources.Icon_1.Save(iconStream);
			iconStream.Seek(0, SeekOrigin.Begin);
			this.Icon = BitmapFrame.Create(iconStream);

			// okay, done initialising
			AreWeDoneLoading = true;
		}

		/// <summary>
		/// Opens up a .scene, duh
		/// </summary>
		private void openButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = ".scene";
			dlg.Filter = "Ogre DotScene files|*.scene";

			bool? result = dlg.ShowDialog();

			if (result == true) {
				originalFilename = dlg.FileName;
				OpenSceneFile(originalFilename);
			}
		}

		/// <summary>
		/// Takes the stuff from the .scene loader and puts them into our NodeData classes
		/// </summary>
		private void OpenSceneFile(string filename) {
			DSL = new DotSceneLoader();
			DSL.ParseDotScene(filename);

			Data = new Collection<NodeData>();

			foreach (Node node in DSL.Nodes) {
				NodeData data = new NodeData {
					Name = node.Name,
					// basically, if our object's name has a # in it, it's treated as a thing. Otherwise it isn't.
					UsesThing = node.Name.Contains("#"),
					ThingFile = node.Name.Contains("#") ? node.Name.Substring(0, node.Name.IndexOf("#")) : "",
					// have to split up all of these since the table doesn't know what to do with a Vector3 etc
					PosX = node.Position.x,
					PosY = node.Position.y,
					PosZ = node.Position.z,
					OrientX = node.Orientation.x,
					OrientY = node.Orientation.y,
					OrientZ = node.Orientation.z,
					OrientW = node.Orientation.w,
					ScaleX = node.Dimensions.x,
					ScaleY = node.Dimensions.y,
					ScaleZ = node.Dimensions.z,
					// only using these for re-exporting the .scene with the things removed
					Mesh = node.Entity != null ? node.Entity.Mesh : null,
					Material = node.Entity != null ? node.Entity.Material : null,
					Static = node.Entity != null ? node.Entity.Static : false,
					CastShadows = node.Entity != null ? node.Entity.CastShadows : false,
					ReceiveShadows = node.Entity != null ? node.Entity.ReceiveShadows : false,
				};
				Data.Add(data);
			}
			dataGrid.ItemsSource = Data;
		}

		/// <summary>
		/// Exports out a .muffin, as well as a .scene with the things removed if the checkbox is, well, checked
		/// </summary>
		private void exportButton_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".muffin";
			dlg.Filter = "Lymph Muffin files|*.muffin";
			dlg.FileName = originalFilename.Substring(originalFilename.LastIndexOf("/") + 1).Replace(".scene", ".muffin");

			bool? result = dlg.ShowDialog();

			if (result == true) {
				// write the .muffin
				string filename = dlg.FileName;

				var orderedData = Data.OrderBy(nd => nd.Name);

				using (var stream = File.Create(filename)) {
					using (var writer = new StreamWriter(stream)) {
						// write out these two first
						writer.WriteLine("Type = " + levelTypeBox.Text);

						// only use the ones that use the .thing, of course
						foreach (NodeData data in orderedData.Where(d => d.UsesThing)) {
							// .thing file
							writer.WriteLine(data.ThingFile + " {");
							// name and position are required
							writer.WriteLine("\tName = \"" + data.Name + "\"");
							writer.WriteLine("\tPosition = " + f(data.PosX) + ", " + f(data.PosY) + ", " + f(data.PosZ));
							// orientation isn't required
							if (data.OrientX != 0 || data.OrientY != 0 || data.OrientZ != 0 || data.OrientW != 1)
								writer.WriteLine("\tOrientation = " + f(data.OrientX) + ", " + f(data.OrientY) + ", " + f(data.OrientZ) + ", " + f(data.OrientW));
							// neither is scale
							if (data.ScaleX != 1 || data.ScaleY != 1 || data.ScaleZ != 1)
								writer.WriteLine("\tScale = " + f(data.ScaleX) + ", " + f(data.ScaleY) + ", " + f(data.ScaleZ));
							// don't forget this bit!
							writer.WriteLine("}");
						}
					}
				}

				if (reexportCheckBox.IsChecked == true) {
					// and then we need to remake the .scene file with the .things taken out
					filename = filename.Replace(".muffin", ".scene");

					using (var stream = File.Create(filename)) {
						using (var writer = new StreamWriter(stream)) {
							// some required scene stuff
							writer.WriteLine(
@"<scene formatVersion=""1.0"" upAxis=""y"" unitsPerMeter=""39.3701"" minOgreVersion=""1.7"" author=""Ponykart's scene to muffin converter"">
	<environment>
	</environment>
	<nodes>");
							// only use the ones that aren't going in the muffin file
							foreach (NodeData data in orderedData.Where(d => !d.UsesThing)) {
								writer.WriteLine("\t\t<node name=\"" + data.Name + "\">");
								writer.WriteLine("\t\t\t<position x=\"" + f(data.PosX) + "\" y=\"" + f(data.PosY) + "\" z=\"" + f(data.PosZ) + "\" />");
								writer.WriteLine("\t\t\t<scale x=\"" + f(data.ScaleX) + "\" y=\"" + f(data.ScaleY) + "\" z=\"" + f(data.ScaleZ) + "\" />");
								writer.WriteLine("\t\t\t<rotation qx=\"" + f(data.OrientX) + "\" qy=\"" + f(data.OrientY) + "\" qz=\"" + f(data.OrientZ)
									+ "\" qw=\"" + f(data.OrientW) + "\" />");
								writer.WriteLine("\t\t\t<entity name=\"" + data.Name + "\" castShadows=\"" + b(data.CastShadows) + "\" receiveShadows=\""
									+ data.ReceiveShadows + "\" meshFile=\"" + data.Mesh + "\" static=\"" + b(data.Static) + "\">");
								// material
								writer.WriteLine("\t\t\t\t<subentities>");
								writer.WriteLine("\t\t\t\t\t<subentity index=\"0\" materialName=\"" + data.Material + "\" />");
								// this stuff
								writer.WriteLine(
@"				</subentities>
			</entity>
		</node>");
							}
							// last bit
							writer.WriteLine("\t</nodes>");
							writer.WriteLine("</scene>");
						}
					}
				}
				MessageBox.Show("Export successful!", filename, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		/// <summary>
		/// should help stop printing decimals as commas since that's dumb, it's stupid, and I hate it
		/// </summary>
		/// <param name="cookie"></param>
		/// <returns></returns>
		string f(float cookie) {
			return cookie.ToString(culture);
		}

		string b(bool squishyMarshmallowButthole) {
			return squishyMarshmallowButthole.ToString().ToLower(culture);
		}
	}
}
