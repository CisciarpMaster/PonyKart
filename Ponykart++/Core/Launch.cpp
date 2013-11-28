#include "Launch.h"
#include "Kernel/LKernel.h"
#include "UI/Splash.h"

int main() {Ponykart::Launch::Main();} // Entry point

using namespace Ponykart;
using namespace Ogre;
using namespace std;

void Launch::Main()
{
	// TODO: Translate if needed
	//LKernel::Initialize();
	//Options.Initialise();

	InitializeOgre();
	//StartRendering();
}

void Launch::InitializeOgre()
{
	Splash splash;
	splash.Show();

	LKernel::LoadInitialObjects(splash);
}

// Adds a message in Ogre's log file. Ogre must be initialized.
void Launch::Log(string message)
{
	LogManager::getSingleton().logMessage(message);
}
