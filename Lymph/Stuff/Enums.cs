using Mogre;

namespace Lymph.Stuff {
	/// <summary>
	/// RenderQueueGroup... L-something.
	/// Enum to represent different render queue groups we stick things in so they're rendered correctly.
	/// 1 = rendered firs
	/// 100 = rendered last
	/// default is... 50 I think?
	/// </summary>
	public enum RQGL {
		/// <summary> 50 (The background that goes behind everything) </summary>
		BACKGROUND = 50,
		/// <summary> 50 (The level itself) </summary>
		LEVEL = 50,
		/// <summary> 60 (Ribbon emitters) </summary>
		RIBBONS = 50,
		/// <summary> 50 (Lymphy, enemies, etc) </summary>
		CELL_CHARACTERS = 50,
		/// <summary> 50 (The organelles inside the cells) </summary>
		ORGANELLES = 50,
		/// <summary> 50 (Antibody meshes) </summary>
		ANTIBODIES = 50,
		/// <summary> 70 (Faces) </summary>
		FACES = 70,
		/// <summary> 100 (The foreground of the level, i.handler. things that you want to obscure everything else) </summary>
		FOREGROUND = 100
	}

	/// <summary>
	/// Face style enum!
	/// </summary>
	public enum FaceStyle {
		NORMAL,
		OHNOES,
		YAY,
		SHOOT
	}

	/// <summary>
	/// Defines different types of move behaviour
	/// </summary>
	public enum MoveBehaviour {
		TOWARDS_PLAYER,
		AWAY_FROM_PLAYER,
		RANDOM,
		/// <summary>
		/// This means the Movement manager should ignore it - it does not mean that this does not move, but it usually does
		/// </summary>
		IGNORE
	}

	/// <summary>
	/// Class for managing ribbon colours, antigen colours, antibody colors, etc
	/// </summary>
	public class AntigenColour {
		public static readonly ColourValue
			_red = new ColourValue(1, 0, 0, 0.75f),
			_blue = new ColourValue(0, 0.4f, 1, 0.75f),
			_yellow = new ColourValue(1, 1, 0, 0.75f),
			_green = new ColourValue(0, 0.95f, 0, 0.75f),
			_orange = new ColourValue(1, 0.6f, 0, 0.75f),
			_magenta = new ColourValue(1, 0, 1, 0.75f),
			_purple = new ColourValue(0.65f, 0, 1, 0.75f),
			_cyan = new ColourValue(0, 1, 1, 0.75f),
			_white = new ColourValue(1, 1, 1, 0.75f),
			_black = new ColourValue(0, 0, 0, 0.75f);

		public static readonly AntigenColour
			red = new AntigenColour(_red),
			blue = new AntigenColour(_blue),
			yellow = new AntigenColour(_yellow),
			green = new AntigenColour(_green),
			orange = new AntigenColour(_orange),
			magenta = new AntigenColour(_magenta),
			purple = new AntigenColour(_purple),
			cyan = new AntigenColour(_cyan),
			white = new AntigenColour(_white),
			black = new AntigenColour(_black);

		private ColourValue colour;

		/// <summary>
		/// Constructor!
		/// </summary>
		/// <param name="c">
		/// The colour to use. Must be one of the readonlies given in this class.
		/// </param>
		public AntigenColour(ColourValue c) {
			this.colour = c;
		}

		/// <summary>
		/// Gets/Sets the internal color that this object represents
		/// </summary>
		public ColourValue Colour {
			get { return this.colour; }
			set { this.colour = value; }
		}

		/// <summary>
		/// Gets the "name" of a colour if it's one of the readonlies given in this class.
		/// Ignores any alpha value you give!
		/// </summary>
		/// <param name="val">
		/// The ColourValue you want to convert
		/// </param>
		/// <returns>
		/// A string of the name of the supplied colour.
		/// (1, 0, 0) -> "red"
		/// (0, 0.4f, 0) -> "blue"
		/// (1, 1, 0) -> "yellow"
		/// (0, 0.8f, 0) -> "green"
		/// anything else -> "unknown"
		/// </returns>
		public static string GetColourName(ColourValue val) {
			ColourValue tempColour = new ColourValue(val.r, val.g, val.b, 0.75f);
			if (tempColour == _red)
				return "red";
			else if (tempColour == _blue)
				return "blue";
			else if (tempColour == _yellow)
				return "yellow";
			else if (tempColour == _green)
				return "green";
			else if (tempColour == _orange)
				return "orange";
			else if (tempColour == _magenta)
				return "magenta";
			else if (tempColour == _purple)
				return "purple";
			else if (tempColour == _cyan)
				return "cyan";
			else if (tempColour == _white)
				return "white";
			else if (tempColour == _black)
				return "black";
			// if we reach this point then it did not match anything
			Launch.Log("[AntigenColour.GetColourName] Unknown colour: " + val.ToString());
			return "unknown";
		}
	}
}
