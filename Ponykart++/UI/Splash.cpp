#include <Ogre.h>
#include "UI/Splash.h"
#include "Core/Options.h"
#include "Kernel/LKernel.h"

using namespace Ogre;
using namespace Ponykart;
using namespace LKernel;
using Ponykart::Core::Options;

// Define the static const members
int Splash::current;
const int Splash::maximum = 19;

Splash::Splash()
{
	current=0;

	log("[Loading] Loading splash screen ressources...");
	ConfigFile configFile;
	configFile.load("media/config/splash.res");
	ConfigFile::SectionIterator seci = configFile.getSectionIterator();
	while (seci.hasMoreElements())
	{
		String secName = seci.peekNextKey();
		ConfigFile::SettingsMultiMap *settings = seci.getNext();
		for (auto i = settings->begin(); i != settings->end(); ++i)
			ResourceGroupManager::getSingleton().addResourceLocation(i->second, i->first, secName);
	}
	ResourceGroupManager::getSingleton().initialiseAllResourceGroups();

	createScene();
	updateGUI();
}

Splash::~Splash()
{
	delete overlay;
	delete progressBG;
	delete progressFG;
	delete progressText;
}

void Splash::updateGUI()
{
	root->renderOneFrame();
	WindowEventUtilities::messagePump();
}

void Splash::increment(std::string text)
{
	log("[Loading] " + text);

	current++;
	progressFG->setDimensions(((float)current)/maximum,1);
	progressText->setCaption(text);
	root->renderOneFrame();
	WindowEventUtilities::messagePump();
}

void Splash::createScene()
{
	log("[Loading] Creating the splash screen...");
	// Loading picture
	overlayManager = &OverlayManager::getSingleton();
	overlay = overlayManager->create("LoadingOverlay");
	((MaterialPtr)MaterialManager::getSingleton().create("LoadingMaterial","splash"))->getTechnique(0)->getPass(0)->createTextureUnitState("loading.png");
	OverlayContainer* panel = static_cast<OverlayContainer*>(overlayManager->createOverlayElement("Panel","LoadingPanel"));
	panel->setMaterialName("LoadingMaterial");
	panel->setDimensions(1,0.96);

	// Progess bar
	OverlayContainer* progressBar = static_cast<OverlayContainer*>(overlayManager->createOverlayElement("Panel","ProgressBar"));
	progressBar->setDimensions(1,0.04);
	progressBar->setPosition(0,0.96);
	MaterialPtr progressBGMat = MaterialManager::getSingleton().create("progressBG","splash");
	progressBGMat->getTechnique(0)->getPass(0)->createTextureUnitState()->setColourOperationEx(LBX_SOURCE1,LBS_MANUAL,LBS_CURRENT,ColourValue(0.9, 0.9, 0.9));;
	MaterialPtr progressFGMat = MaterialManager::getSingleton().create("progressFG","splash");
	progressFGMat->getTechnique(0)->getPass(0)->createTextureUnitState()->setColourOperationEx(LBX_SOURCE1,LBS_MANUAL,LBS_CURRENT,ColourValue(0.23, 0.88, 0.5));;
	progressBG = static_cast<OverlayContainer*>(overlayManager->createOverlayElement("Panel","ProgressBarBG"));
	progressBG->setMaterialName("progressBG");
	progressBG->setDimensions(1,1);
	progressFG = static_cast<OverlayContainer*>(overlayManager->createOverlayElement("Panel","ProgressBarFG"));
	progressFG->setMaterialName("progressFG");
	progressFG->setDimensions(0,1);

	// Loading text
	FontManager::getSingleton().getByName("BlueHighway")->load(); // HACK: Workaround for bug #324 in Ogre
	progressText = static_cast<TextAreaOverlayElement*>(overlayManager->createOverlayElement("TextArea", "text"));
	progressText->setMetricsMode(GuiMetricsMode::GMM_RELATIVE);
	progressText->setLeft(0.01);
	progressText->setTop(0.01);
	progressText->setHorizontalAlignment(GHA_LEFT);
	progressText->setVerticalAlignment(GVA_TOP);
	progressText->setFontName("BlueHighway");
	progressText->setCharHeight(0.03);
	progressText->setColour(Ogre::ColourValue(0,0,0));

	progressBar->addChild(progressBG);
	progressBar->addChild(progressFG);
	progressBar->addChild(progressText);
	overlay->add2D(panel);
	overlay->add2D(progressBar);
	overlay->show();
}
