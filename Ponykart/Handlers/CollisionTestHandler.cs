using System;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	//[Handler(HandlerScope.Level)]
	[Obsolete]
	public class CollisionTestHandler : ILevelHandler {

		public CollisionTestHandler() {
			LKernel.Get<CollisionReporter>().AddEvent(PonykartCollisionGroups.Karts, PonykartCollisionGroups.Triggers, OnCollision);
		}

		void OnCollision(CollisionReportInfo info) {
			Console.WriteLine(info.Flags);
		}


		public void Dispose() {
			LKernel.Get<CollisionReporter>().RemoveEvent(PonykartCollisionGroups.Karts, PonykartCollisionGroups.Triggers, OnCollision);
		}
	}
}
