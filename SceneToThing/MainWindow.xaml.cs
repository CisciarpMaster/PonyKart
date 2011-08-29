using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace SceneToThing {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		/// <summary>
		/// the window sends out selectionChanged events when it's initialising, and we don't want to respond to those. That's what this is for
		/// </summary>
		public static bool AreWeDoneLoading = false;
		DotSceneLoader DSL;
		ICollection<Block> Blocks;
		CultureInfo culture = CultureInfo.InvariantCulture;

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

		private void openButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = ".scene";
			dlg.Filter = "Ogre DotScene files (.scene)|*.scene";

			bool? result = dlg.ShowDialog();

			if (result == true) {
				string filename = dlg.FileName;
				OpenSceneFile(filename);
			}
		}

		/// <summary>
		/// Opens and parses a .scene file and puts its components into the Blocks collection.
		/// </summary>
		void OpenSceneFile(string filename) {
			DSL = new DotSceneLoader();
			DSL.ParseDotScene(filename);

			Blocks = new Collection<Block>();

			// go through each of the nodes in the .scene and find which ones are shapes
			foreach (Node node in DSL.Nodes) {
				// shapes
				if (node.Entity.Mesh == "box.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Box;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Dimensions = node.Dimensions;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				else if (node.Entity.Mesh == "sphere.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Sphere;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Radius = node.Dimensions.x / 2f;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				else if (node.Entity.Mesh == "cylinder.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Cylinder;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Dimensions = node.Dimensions;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				else if (node.Entity.Mesh == "cone.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Cone;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Radius = node.Dimensions.x / 2f;
					shape.Height = node.Dimensions.y;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				else if (node.Entity.Mesh == "capsule.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Capsule;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Radius = node.Dimensions.x / 2f;
					shape.Height = node.Dimensions.y;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				// not shapes
				else {
					Blocks.Add(node);
				}
			}

			sceneListBox.Items.Clear();

			// add the blocks to the list box
			foreach (var block in Blocks) {
				ListBoxItem item = new ListBoxItem();
				sceneListBox.Items.Add(block);
			}

			// update the name box
			// remove the .scene from the filename
			filename = filename.Remove(filename.IndexOf(".scene"));
			// get rid of the folder stuff before the scene name
			if (filename.Contains("/"))
				nameBox.Text = filename.Substring(filename.LastIndexOf('/') + 1);
			else if (filename.Contains("\\"))
				nameBox.Text = filename.Substring(filename.LastIndexOf('\\') + 1);
			else
				nameBox.Text = filename;

			saveButton.IsEnabled = true;
			nameBox.IsEnabled = true;
			physicsBox.IsEnabled = true;
		}

		/// <summary>
		/// Export out our blocks into a .thing file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveButton_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".thing";
			dlg.Filter = "Lymph Thing files|*.thing";
			dlg.FileName = nameBox.Text + ".thing";

			bool? result = dlg.ShowDialog();

			// now we know where to put the file
			if (result == true) {
				string filename = dlg.FileName;

				using (var stream = File.Create(filename)) {
					using (var writer = new StreamWriter(stream)) {
						// write out the "overall" stuff
						writer.WriteLine("Name = \"" + nameBox.Text + "\"");
						writer.WriteLine("Physics = " + physicsBox.Text);
						if (physicsBox.Text != "None")
							writer.WriteLine("Mass = " + massBox.Text);

						foreach (Block block in Blocks) {
							// write out our model blocks
							Node node = block as Node;
							if (node != null) {
								writer.WriteLine("Model {");
								writer.WriteLine("\tName = \"" + node.Name + "\"");
								writer.WriteLine("\tMesh = \"" + node.Entity.Mesh + "\"");
								writer.WriteLine("\tMaterial = \"" + node.Entity.Material + "\"");
								writer.WriteLine("\tPosition = " + f(node.Position.x) + ", " + f(node.Position.y) + ", " + f(node.Position.z));
								writer.WriteLine("\tOrientation = " + f(node.Orientation.x) + ", " + f(node.Orientation.y) + ", " + f(node.Orientation.z) + ", " + f(node.Orientation.w));
								writer.WriteLine("\tScale = " + f(node.Dimensions.x) + ", " + f(node.Dimensions.y) + ", " + f(node.Dimensions.z));
								writer.WriteLine("\tCastsShadows = " + node.Entity.CastShadows);
								writer.WriteLine("}");
								continue;
							}

							// write out our shape blocks
							Shape shape = block as Shape;
							if (shape != null) {
								writer.WriteLine("// " + shape.Name);
								writer.WriteLine("Shape {");
								writer.WriteLine("\tType = " + shape.Type);
								writer.WriteLine("\tPosition = " + f(shape.Position.x) + ", " + f(shape.Position.y) + ", " + f(shape.Position.z));
								writer.WriteLine("\tOrientation = " + f(shape.Orientation.x) + ", " + f(shape.Orientation.y) + ", " + f(shape.Orientation.z) + ", " + f(shape.Orientation.w));
								if (shape.Type == ShapeTypes.Box || shape.Type == ShapeTypes.Cylinder)
									writer.WriteLine("\tDimensions = " + f(shape.Dimensions.x) + ", " + f(shape.Dimensions.y) + ", " + f(shape.Dimensions.z));
								else if (shape.Type == ShapeTypes.Capsule || shape.Type == ShapeTypes.Cone) {
									writer.WriteLine("\tHeight = " + f(shape.Height));
									writer.WriteLine("\tRadius = " + f(shape.Radius));
								}
								else if (shape.Type == ShapeTypes.Sphere)
									writer.WriteLine("\tRadius = " + f(shape.Radius));
								writer.WriteLine("}");
								continue;
							}
						}
						writer.Close();
					}
					stream.Close();
				}
				MessageBox.Show("Export successful!", filename, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		string f(float cookie) {
			return cookie.ToString(culture);
		}

		/// <summary>
		/// Put all of the info from the selected block into the text boxes
		/// </summary>
		private void sceneListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			// if we have nothing selected, disable all of the boxes
			if (e.AddedItems.Count == 0) {
				positionXBox.Text = "";
				positionYBox.Text = "";
				positionZBox.Text = "";
				orientationXBox.Text = "";
				orientationYBox.Text = "";
				orientationZBox.Text = "";
				orientationWBox.Text = "";
				dimensionsXBox.Text = "";
				dimensionsYBox.Text = "";
				dimensionsZBox.Text = "";
				radiusBox.Text = "";
				heightBox.Text = "";

				typeComboBox.IsEnabled = false;
				positionXBox.IsEnabled = false;
				positionYBox.IsEnabled = false;
				positionZBox.IsEnabled = false;
				orientationXBox.IsEnabled = false;
				orientationYBox.IsEnabled = false;
				orientationZBox.IsEnabled = false;
				orientationWBox.IsEnabled = false;
				dimensionsXBox.IsEnabled = false;
				dimensionsYBox.IsEnabled = false;
				dimensionsZBox.IsEnabled = false;
				radiusBox.IsEnabled = false;
				heightBox.IsEnabled = false;
				saveChangesButton.IsEnabled = false;
				discardChangesButton.IsEnabled = false;
				return;
			}
			// otherwise enable all of them
			else {
				typeComboBox.IsEnabled = true;
				positionXBox.IsEnabled = true;
				positionYBox.IsEnabled = true;
				positionZBox.IsEnabled = true;
				orientationXBox.IsEnabled = true;
				orientationYBox.IsEnabled = true;
				orientationZBox.IsEnabled = true;
				orientationWBox.IsEnabled = true;
				dimensionsXBox.IsEnabled = true;
				dimensionsYBox.IsEnabled = true;
				dimensionsZBox.IsEnabled = true;
				radiusBox.IsEnabled = true;
				heightBox.IsEnabled = true;
				saveChangesButton.IsEnabled = true;
				discardChangesButton.IsEnabled = true;
			}

			// make sure it's actually a block
			Block item = e.AddedItems[0] as Block;
			if (item == null)
				throw new ArgumentException("Selected item was not a Block!");

			// try using the block as a node
			Node node = item as Node;
			if (node != null) {
				typeComboBox.IsEnabled = false;
				dimensionsLabel.Content = "Scale:";
				positionXBox.Text = node.Position.x + "";
				positionYBox.Text = node.Position.y + "";
				positionZBox.Text = node.Position.z + "";
				orientationXBox.Text = node.Orientation.x + "";
				orientationYBox.Text = node.Orientation.y + "";
				orientationZBox.Text = node.Orientation.z + "";
				orientationWBox.Text = node.Orientation.w + "";
				dimensionsXBox.Text = node.Dimensions.x + "";
				dimensionsYBox.Text = node.Dimensions.y + "";
				dimensionsZBox.Text = node.Dimensions.z + "";
				radiusBox.IsEnabled = false;
				heightBox.IsEnabled = false;
				return;
			}

			// otherwise try using it as a shape
			Shape shape = item as Shape;
			if (shape != null) {
				typeComboBox.SelectedIndex = (int) shape.Type;
				typeComboBox.IsEnabled = true;
				dimensionsLabel.Content = "Dimensions:";
				positionXBox.Text = shape.Position.x + "";
				positionYBox.Text = shape.Position.y + "";
				positionZBox.Text = shape.Position.z + "";
				orientationXBox.Text = shape.Orientation.x + "";
				orientationYBox.Text = shape.Orientation.y + "";
				orientationZBox.Text = shape.Orientation.z + "";
				orientationWBox.Text = shape.Orientation.w + "";

				// since all of the different shapes use different boxes, we have to disable the ones we don't want
				if (shape.Type == ShapeTypes.Box || shape.Type == ShapeTypes.Cylinder) {
					dimensionsXBox.Text = shape.Dimensions.x + "";
					dimensionsYBox.Text = shape.Dimensions.y + "";
					dimensionsZBox.Text = shape.Dimensions.z + "";
					radiusBox.Text = "";
					radiusBox.IsEnabled = false;
					heightBox.Text = "";
					heightBox.IsEnabled = false;
				}
				else if (shape.Type == ShapeTypes.Capsule || shape.Type == ShapeTypes.Cone) {
					dimensionsXBox.Text = "";
					dimensionsXBox.IsEnabled = false;
					dimensionsYBox.Text = "";
					dimensionsYBox.IsEnabled = false;
					dimensionsZBox.Text = "";
					dimensionsZBox.IsEnabled = false;
					radiusBox.Text = shape.Radius + "";
					heightBox.Text = shape.Height + "";
				}
				else if (shape.Type == ShapeTypes.Sphere) {
					dimensionsXBox.Text = "";
					dimensionsXBox.IsEnabled = false;
					dimensionsYBox.Text = "";
					dimensionsYBox.IsEnabled = false;
					dimensionsZBox.Text = "";
					dimensionsZBox.IsEnabled = false;
					radiusBox.Text = shape.Radius + "";
					heightBox.Text = "";
					heightBox.IsEnabled = false;
				}
			}
		}

		/// <summary>
		/// Take the stuff in the text boxes and update the block object
		/// </summary>
		private void saveChangesButton_Click(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			// make sure it's a block
			Block item = sceneListBox.SelectedItem as Block;
			if (item == null)
				throw new ArgumentException("Selected item was not a Block!");

			// try as a node
			Node node = item as Node;
			if (node != null) {
				try {
					node.Position = new Vector3(float.Parse(positionXBox.Text), float.Parse(positionYBox.Text), float.Parse(positionZBox.Text));
					node.Orientation = new Quaternion(float.Parse(orientationXBox.Text), float.Parse(orientationYBox.Text),
						float.Parse(orientationZBox.Text), float.Parse(orientationWBox.Text));
					node.Dimensions = new Vector3(float.Parse(dimensionsXBox.Text), float.Parse(dimensionsYBox.Text), float.Parse(dimensionsZBox.Text));
				}
				catch (Exception ex) {
					MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			// try as a shape
			Shape shape = item as Shape;
			if (shape != null) {
				try {
					// new type
					shape.Type = (ShapeTypes) typeComboBox.SelectedIndex;
					// new position
					shape.Position = new Vector3(float.Parse(positionXBox.Text), float.Parse(positionYBox.Text), float.Parse(positionZBox.Text));
					// new orientation
					shape.Orientation = new Quaternion(float.Parse(orientationXBox.Text), float.Parse(orientationYBox.Text),
						float.Parse(orientationZBox.Text), float.Parse(orientationWBox.Text));
					// new dimensions/radius/height
					if (shape.Type == ShapeTypes.Box || shape.Type == ShapeTypes.Cylinder) {
						shape.Dimensions = new Vector3(float.Parse(dimensionsXBox.Text), float.Parse(dimensionsYBox.Text), float.Parse(dimensionsZBox.Text));
					}
					else if (shape.Type == ShapeTypes.Capsule || shape.Type == ShapeTypes.Cone) {
						shape.Radius = float.Parse(radiusBox.Text);
						shape.Height = float.Parse(heightBox.Text);
					}
					else if (shape.Type == ShapeTypes.Sphere) {
						shape.Radius = float.Parse(radiusBox.Text);
					}
				}
				catch (Exception ex) {
					MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		/// <summary>
		/// Overwrites whatever's in the text boxes with whatever the block had before
		/// </summary>
		private void discardChangesButton_Click(object sender, RoutedEventArgs e) {
			List<object> emptyList = new List<object>();
			List<object> newList = new List<object>();
			newList.Add(sceneListBox.SelectedItem);
			sceneListBox_SelectionChanged(sender, new SelectionChangedEventArgs(e.RoutedEvent, emptyList, newList));
		}

		/// <summary>
		/// If we change the type, we have to enable/disable boxes and stick new data into them.
		/// We do NOT want to modify the shape object itself!
		/// </summary>
		private void typeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0 || !AreWeDoneLoading)
				return;

			// the new type
			ShapeTypes newType = (ShapeTypes) typeComboBox.SelectedIndex;
			try {
				// x is either the x scale or the radius
				float x = dimensionsXBox.Text != "" ? float.Parse(dimensionsXBox.Text) : float.Parse(radiusBox.Text);
				// y is either the y scale or the height, or if both of those don't exist, then the radius
				float y = dimensionsYBox.Text != "" ? float.Parse(dimensionsYBox.Text) : (heightBox.Text != "" ? float.Parse(heightBox.Text) : float.Parse(radiusBox.Text));
				// z is either the z scale or the radius
				float z = dimensionsZBox.Text != "" ? float.Parse(dimensionsZBox.Text) : float.Parse(radiusBox.Text);

				// update our boxes
				if (newType == ShapeTypes.Box || newType == ShapeTypes.Cylinder) {
					dimensionsXBox.IsEnabled = true;
					dimensionsXBox.Text = x + "";
					dimensionsYBox.IsEnabled = true;
					dimensionsYBox.Text = y + "";
					dimensionsZBox.IsEnabled = true;
					dimensionsZBox.Text = z + "";
					radiusBox.IsEnabled = false;
					heightBox.IsEnabled = false;
				}
				else if (newType == ShapeTypes.Capsule || newType == ShapeTypes.Cone) {
					dimensionsXBox.IsEnabled = false;
					dimensionsYBox.IsEnabled = false;
					dimensionsZBox.IsEnabled = false;
					radiusBox.IsEnabled = true;
					radiusBox.Text = x + "";
					heightBox.IsEnabled = true;
					heightBox.Text = y + "";
				}
				else if (newType == ShapeTypes.Sphere) {
					dimensionsXBox.IsEnabled = false;
					dimensionsYBox.IsEnabled = false;
					dimensionsZBox.IsEnabled = false;
					heightBox.IsEnabled = false;
					radiusBox.IsEnabled = true;
					radiusBox.Text = x + "";
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void physicsBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;

			if ((string) item.Content == "None" || (string) item.Content == "Static") {
				massBox.IsEnabled = false;
				massBox.Text = "0";
			}
			else {
				massBox.IsEnabled = true;
				massBox.Text = "1";
			}
		}
	}
}
