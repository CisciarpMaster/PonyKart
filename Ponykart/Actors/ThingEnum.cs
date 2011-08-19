namespace Ponykart.Actors {
	/// <summary>
	/// Enumerator for Things. Every Thing that can be instantiated needs to have an enum definition here.
	/// This is used in the Spawner. It's a hell of a lot easier to use this than use reflection to find
	/// all subclasses of Thing, match them via string, and then create one.
	/// </summary>
	public enum SpawnThingEnum {
		Kart,
		Obstacle,
		ZergShip
	}
}
