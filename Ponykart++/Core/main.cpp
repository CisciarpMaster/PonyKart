#include <string>
#include <Ogre.h>
#include "Core/Options.h"
#include "UI/Splash.h"
#include "Kernel/LKernel.h"

using namespace Ponykart::LKernel;
using Ponykart::Core::Options;
using Ponykart::Splash;

int main()
{
	initOgreRoot();
	try
	{
		log("[Loading] Loading configuration...");
		Options::Initialize();

		log("[Loading] Loading the render system...");
		initOgreRenderSystem();

		log("[Loading] Creating the render window...");
		initOgreRenderWindow();

		Splash splash;

		loadInitialObjects(splash);

		//startRendering();

		return EXIT_SUCCESS;
	}
	catch (std::string e) // If you can't guarantee that someone will catch your exceptions, throw a string.
	{
		log("[EXCEPTION] " + e);
	}
}
