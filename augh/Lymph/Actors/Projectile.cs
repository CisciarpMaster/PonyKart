
namespace Lymph.Actors
{
	///<summary> A projectile. Basically an thing with no face and a ribbon attached. </summary>
	public abstract class Projectile : DynamicThing
	{
		/// <summary>
		/// Ribbons should be attached to the Node
		/// </summary>
		public Projectile(ThingTemplate tt) : base(tt) {

		}
	}
}
