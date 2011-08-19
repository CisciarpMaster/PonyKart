﻿using System;
using System.Globalization;
using System.IO;
using Mogre;
using Ponykart.IO.ThingParser;
using Node = Ponykart.IO.ThingParser.Node;

namespace Ponykart.IO {
	public class ThingImporter {
		RuleInstance root;
		CultureInfo culture = CultureInfo.InvariantCulture;

		public ThingDefinition Parse(string nameOfThing) {

			string fileContents = "";

			// make the file path
			string filePath = "media/things/" + nameOfThing + ".thing";
			// if we don't have a save file for this level yet, use the "default" one
			if (!File.Exists(filePath)) {
				throw new ArgumentException("A .thing file with that name does not exist! (" + nameOfThing + ")");
			}

			Launch.Log("[ThingImporter] Importing and parsing thing: " + filePath);

			// read stuff
			using (var fileStream = File.Open(filePath, FileMode.Open)) {
				using (var reader = new StreamReader(fileStream)) {
					// for each line in the file
					while (!reader.EndOfStream) {
						fileContents += reader.ReadLine() + "\r\n";
					}
					reader.Close();
				}
			}

			Parser p = new Parser();
			root = p.Parse(fileContents);


			ThingDefinition thingDef = new ThingDefinition(nameOfThing);

			Parse(thingDef);

			thingDef.Finish();

			return thingDef;
		}

		/// <summary>
		/// Parses right from the root
		/// </summary>
		void Parse(ThingDefinition thingDef) {
			for (int a = 0; a < root.Children.Length; a++) {
				Node prop = root.Children[a];
				switch (prop.Type) {
					case NodeType.Rule_Property:
						ParseProperty(thingDef, (prop as RuleInstance).Children[0] as RuleInstance);
						break;
					case NodeType.Rule_Shape:
						ParseShape(thingDef, prop as RuleInstance);
						break;
					case NodeType.Rule_Model:
						ParseModel(thingDef, prop as RuleInstance);
						break;
				}
			}
		}

		/// <summary>
		/// Takes a property and calls the appropriate parser method depending on its type
		/// </summary>
		void ParseProperty(ThingTokenHolder holder, RuleInstance prop) {
			string propName = GetNameFromProperty(prop).ToLower(culture);
			switch (prop.Type) {
				case NodeType.Rule_StringProperty:
					holder.StringTokens[propName] = ParseStringProperty(prop);
					break;
				case NodeType.Rule_BoolProperty:
					holder.BoolTokens[propName] = ParseBoolProperty(prop);
					break;
				case NodeType.Rule_EnumProperty:
					holder.EnumTokens[propName] = ParseEnumProperty(prop);
					break;
				case NodeType.Rule_NumericProperty:
					holder.FloatTokens[propName] = ParseFloatProperty(prop);
					break;
				case NodeType.Rule_Vec3Property:
					holder.VectorTokens[propName] = ParseVectorProperty(prop);
					break;
			}
		}

		/// <summary>
		/// Gets a name out of a property. It's in its own method because it's used all the time.
		/// </summary>
		string GetNameFromProperty(RuleInstance prop) {
			RuleInstance nameRule = prop.Children[0] as RuleInstance;
			Token nameTok = nameRule.Children[0] as Token;
			return nameTok.Image;
		}

		/// <summary>
		/// Parse a string property
		/// </summary>
		string ParseStringProperty(RuleInstance prop) {
			Token valTok = prop.Children[2] as Token;
			string val = valTok.Image;

			// need to substring because we get "\"foo\"" from the file, and we don't want extra quotes
			return val.Substring(1, val.Length - 2);
		}

		// TODO
		bool ParseBoolProperty(RuleInstance prop) {


			return false;
		}

		/// <summary>
		/// Parse an enum property. The value must exist in ThingEnum, but it is not case sensitive.
		/// </summary>
		ThingEnum ParseEnumProperty(RuleInstance prop) {
			RuleInstance valRule = prop.Children[2] as RuleInstance;
			Token valTok = valRule.Children[0] as Token;
			ThingEnum te;
			if (Enum.TryParse<ThingEnum>(valTok.Image, true, out te)) 
				return te;
			else 
				throw new ApplicationException("Unable to parse Enum property!");
		}

		/// <summary>
		/// Parse a float property
		/// </summary>
		float ParseFloatProperty(RuleInstance prop) {
			Token tok = prop.Children[2] as Token;
			float f = float.Parse(tok.Image, culture);

			return f;
		}

		/// <summary>
		/// Parse a vector property, i.e. a triplet of floats separated by commas
		/// </summary>
		Vector3 ParseVectorProperty(RuleInstance prop) {
			Token tok1 = prop.Children[2] as Token;
			Token tok2 = prop.Children[4] as Token;
			Token tok3 = prop.Children[6] as Token;

			float x = float.Parse(tok1.Image, culture);
			float y = float.Parse(tok2.Image, culture);
			float z = float.Parse(tok3.Image, culture);

			return new Vector3(x, y, z);
		}

		/// <summary>
		/// Shape blocks
		/// </summary>
		void ParseShape(ThingDefinition thingDef, RuleInstance block) {
			ShapeBlock shapeBlock = new ShapeBlock(thingDef);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(shapeBlock, rule.Children[0] as RuleInstance);
			}

			thingDef.ShapeBlocks.Add(shapeBlock);
		}

		/// <summary>
		/// Model blocks
		/// </summary>
		void ParseModel(ThingDefinition thingDef, RuleInstance block) {
			ModelBlock modelBlock = new ModelBlock(thingDef);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(modelBlock, rule.Children[0] as RuleInstance);
			}

			thingDef.ModelBlocks.Add(modelBlock);
		}
	}
}