#include <Ogre.h>
#include "Splash.h"
#include "Core/Launch.h"

using namespace Ponykart;
using namespace Ogre;

// Define the static const members
const int Splash::maximum = 19;
const char* const Splash::LoadingPicture = "loading.png"; // In folder/group gui

Splash::Splash()
{

}

void Splash::Show()
{
	go();
}

void Increment(std::string text)
{
	// TODO: Increment the progress bar if we're not already at 100%
	// TODO: Update the loading text label
	Launch::Log("[Loading] " + text);
}

void Splash::createScene()
{
	// Loading picture
	OverlayManager& overlayManager = Ogre::OverlayManager::getSingleton();
	Overlay* overlay = overlayManager.create("LoadingOverlay");
	((MaterialPtr)MaterialManager::getSingleton().create("LoadingMaterial","gui"))->getTechnique(0)->getPass(0)->createTextureUnitState(LoadingPicture);
	OverlayContainer* panel = static_cast<OverlayContainer*>(overlayManager.createOverlayElement("Panel","LoadingPanel"));
	panel->setMaterialName("LoadingMaterial");
	overlay->add2D(panel);
	overlay->show();
}
