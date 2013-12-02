#ifndef SPLASH_H_INCLUDED
#define SPLASH_H_INCLUDED

#include <string>
#include "Ogre.h"
#include "OgreTextAreaOverlayElement.h"

namespace Ponykart
{
	/// This class manages the splash screen you see when you start up the game.
	class Splash
	{
	public:
		explicit Splash();
		~Splash();
		void updateGUI(); // Render a frame and process the window's message queue
		void increment(std::string text);

	private:
		void createScene();

	private:
		Ogre::Overlay* overlay;
		Ogre::TextAreaOverlayElement* progressText;
		Ogre::OverlayContainer* progressBG;
		Ogre::OverlayContainer* progressFG;
		Ogre::OverlayManager* overlayManager;
		static int current; // Progress bar state
		static const int maximum; // Progress bar maximum
	};
}

#endif // SPLASH_H_INCLUDED
