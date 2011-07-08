using Lymph.Stuff;

namespace Lymph.Actors
{
	///<summary> A projectile. Basically an thing with no face and a ribbon attached. </summary>
	public abstract class Projectile : DynamicThing
	{
		public AntigenColour Colour { get; set; }

		/// <summary>
		/// Ribbons should be attached to the Node
		/// </summary>
		public Projectile(ThingTemplate tt, AntigenColour c)
			: base(tt) 
		{
			Colour = c;
		}
	}
}
