#include <boost/thread.hpp>
#include "LKernel.h"
#include "Levels/LevelManager.h"
#include "Physics/PhysicsMain.h"

using namespace Ogre;
using namespace Ponykart;
using namespace Ponykart::Physics;
using namespace LKernel::details;

void LKernel::loadInitialObjects(Splash& splash)
{
	splash.increment("Setting up level manager...");
	Levels::LevelManager* levelManager = addGlobalObject(new Levels::LevelManager());

	// Ogre
	splash.increment("Initialising resources and resource groups...");
	initOgreResources();
	loadOgreResourceGroups();

	// Bullet
	splash.increment("Initialising Bullet physics engine...");
	try
	{
		addGlobalObject(new PhysicsMain());
		//addGlobalObject(new CollisionShapeManager());
		//addGlobalObject(new CollisionReporter());
		//addGlobalObject(new TriggerReporter());
		//addGlobalObject(new PhysicsMaterialFactory());
	}
	catch (...)
	{
		throw std::string("Bullet loading unsuccessful! Try installing the 2010 VC++ Redistributable (x86) - google it!");
	}
}
