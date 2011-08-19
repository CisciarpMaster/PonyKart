using System;

namespace Ponykart.Actors {
	public interface IThingComponent : IDisposable {
		int ID { get; }
		string Name { get; }

		string ToString();
	}
}
