
namespace Lymph.Actors
{
	/// <summary> Class that all cells inherit from </summary>
	public abstract class Cell : KinematicThing
	{
		public int HP { get; set; }

		public Cell(ThingTemplate tt)
			: base(tt)
		{
		}
	}
}
