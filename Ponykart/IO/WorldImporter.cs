using System.Globalization;
using System.IO;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.IO {
	/// <summary>
	/// Imports worlds from a file.
	/// </summary>
	public class WorldImporter {
		RuleInstance root;
		CultureInfo culture = CultureInfo.InvariantCulture;


		/// <summary>
		/// Imports a world from the given file.
		/// </summary>
		/// TODO: We'll need something to see if the save file exists or not, and if it doesn't, to load the "new world" version
		public void Parse(Level level) {

			string fileContents = "";

			// make the file path
			string filePath = Settings.Default.SaveFileLocation + level.Name + Settings.Default.SaveFileExtension;
			// if we don't have a save file for this level yet, use the "default" one
			if (!File.Exists(filePath)) {
				filePath = Settings.Default.SaveFileLocation + level.Name + Settings.Default.DefaultSaveFileExtension;

				// what if there is no default?!
				if (!File.Exists(filePath)) {
					Launch.Log("[World Importer] WARNING: Unable to find default save file for this level!");
					return;
				}
			}

			Launch.Log("[World Importer] Importing and parsing level: " + filePath);

			// read stuff
			using (var fileStream = File.Open(filePath, FileMode.Open))
			{
				using (var reader = new StreamReader(fileStream))
				{
					// for each line in the file
					while (!reader.EndOfStream)
					{
						fileContents += reader.ReadLine() + "\r\n";
					}
					reader.Close();
				}
			}

			Parser p = new Parser();
			root = p.Parse(fileContents);

			ParseFlags(level);
			ParseNumbers(level);
			ParseEntities(level);
		}

		/// <summary>
		/// Parse boolean flags from the tree
		/// </summary>
		void ParseFlags(Level level) {
			RuleInstance flagNode = root.Children[0] as RuleInstance;
			// child 3 is EOF

			// no need to check to see if there are any flags, since if there aren't, the condition will make it quit the for loop immediately
			for (int a = 2; a < flagNode.Children.Length; a++)
			{
				RuleInstance flag = flagNode.Children[a] as RuleInstance;
				string name = ((flag.Children[0] as RuleInstance).Children[0] as Token).Image;
				bool value = (flag.Children[2] as Token).Type == NodeType.Tok_KeyTrue;

				level.Flags[name] = value;
			}
		}

		/// <summary>
		/// Parse numbers from the tree - these are all interpreted as floats
		/// </summary>
		void ParseNumbers(Level level) {
			RuleInstance numNode = root.Children[1] as RuleInstance;

			for (int a = 2; a < numNode.Children.Length; a++)
			{
				RuleInstance num = numNode.Children[a] as RuleInstance;
				string name = ((num.Children[0] as RuleInstance).Children[0] as Token).Image;
				float value = float.Parse((num.Children[2] as Token).Image, culture);

				level.Numbers[name] = value;
			}
		}

		/// <summary>
		/// Parse the entities and stick them into templates
		/// </summary>
		void ParseEntities(Level level) {
			RuleInstance entNode = root.Children[2] as RuleInstance;

			// loop through the list of entities
			for (int a = 2; a < entNode.Children.Length; a++)
			{
				RuleInstance ent = entNode.Children[a] as RuleInstance;
				string type = ((ent.Children[2] as RuleInstance).Children[0] as Token).Image;
				ThingTemplate template = new ThingTemplate(type);

				// loop through the list of properties for that entity
				for (int b = 5; b < ent.Children.Length - 1; b++)
				{
					RuleInstance prop = (ent.Children[b] as RuleInstance);
					

					if (prop.Type == NodeType.Rule_EntityProperty)
						ParseEntityProperty(prop.Children[0] as RuleInstance, template);

					// the override block is the last block, but since it's optional it might not exist.
					// so we need to check for it
					else if (prop.Type == NodeType.Rule_OverridesBlock)
					{
						// does the same thing as the previous for loop, but this is inside the overrides block
						for (int c = 2; c < prop.Children.Length - 1; c++) // the -1 is because the last child is a right brace
						{
							RuleInstance oprop = (prop.Children[c] as RuleInstance).Children[0] as RuleInstance;
							ParseEntityProperty(oprop, template);
						}
					}
				}
				// and then stuff it into the dictionary
				// if it already exists in the dictionary, then we need to put it in the dictionary with its ID# after it
				string currentName = template.StringTokens["Name"];
				if (level.Templates.ContainsKey(currentName)) {
					Launch.Log("[WorldImporter] WARNING: An entity with that name already exists in this world!");
					string newName = currentName + template.ID;
					// put the new name into the template
					template.StringTokens["Name"] = newName;
					// then put the template into the dictionary using its new name
					level.Templates[newName] = template;
				} else
					level.Templates[currentName] = template;
			}
		}

		/// <summary>
		/// We pass tokens and stick them into a template's dictionary
		/// </summary>
		/// <param name="prop">The property to get the token out of</param>
		/// <param name="tt">The template we're going to stick the token into</param>
		void ParseEntityProperty(RuleInstance prop, ThingTemplate tt) {
			switch (prop.Type) {
				// if it's a string
				case NodeType.Rule_EPString:
					string name		= ((prop.Children[0] as RuleInstance).Children[0] as Token).Image;
					string svalue	= (prop.Children[2] as Token).Image;
					svalue = svalue.Substring(1, svalue.Length - 2);
					tt.StringTokens[name] = svalue;
					break;
				// if it's a number
				case NodeType.Rule_EPFloat:
						  name		= ((prop.Children[0] as RuleInstance).Children[0] as Token).Image;
					float fvalue	= float.Parse((prop.Children[2] as Token).Image, culture);
					tt.FloatTokens[name] = fvalue;
					break;
				// if it's a boolean
				case NodeType.Rule_EPBool:
						 name		= ((prop.Children[0] as RuleInstance).Children[0] as Token).Image;
					bool bvalue		= (prop.Children[2] as Token).Type == NodeType.Tok_KeyTrue;
					tt.BoolTokens[name] = bvalue;
					break;
				// if it's a tuple of numbers
				case NodeType.Rule_EPVector3D:
						  name		= ((prop.Children[0] as RuleInstance).Children[0] as Token).Image;
					float x			= float.Parse((prop.Children[2] as Token).Image, culture);
					float y			= float.Parse((prop.Children[4] as Token).Image, culture);
					float z			= float.Parse((prop.Children[6] as Token).Image, culture);
					tt.VectorTokens[name] = new Vector3(x, y, z);
					break;
			}
		}
	}
}
