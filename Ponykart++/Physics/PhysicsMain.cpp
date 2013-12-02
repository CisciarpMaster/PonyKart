#include <Ogre.h>
#include "Physics/PhysicsMain.h"
#include "Kernel/LKernel.h"
#include "Levels/LevelManager.h"

using namespace Ogre;
using namespace Ponykart::Physics;
using namespace Ponykart::LKernel;
using namespace Ponykart::Levels;

PhysicsMain::PhysicsMain()
{
	log("[Loading] Creating PhysicsMain...");
	LevelManager::OnLevelUnload.push_back(OnLevelUnload);
}

void PhysicsMain::OnLevelUnload(LevelChangedEventArgs eventArgs)
{
	LKernel::GetG<Root>()->removeFrameListener(FrameEnded);
	// TODO: FrameEnded must inherit from Ogre::FrameListener

	lock (world)
	{
		if (!world.IsDisposed)
		{
			for (int a = 0; a < world.CollisionObjectArray.Count; a++)
			{
				var obj = world.CollisionObjectArray[a];
				if (obj != null && !obj.IsDisposed)
				{
					world.RemoveCollisionObject(obj);
					obj.Dispose();
				}
			}

			broadphase.Dispose();
			solver.Dispose();
			dcc.Dispose();
			dispatcher.Dispose();

			world.Dispose();
		}
	}
}
