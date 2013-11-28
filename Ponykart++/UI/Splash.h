#ifndef SPLASH_H_INCLUDED
#define SPLASH_H_INCLUDED

#include <string>
#include "ExampleApplication.h" // Ogre's ready-to-run class

namespace Ponykart
{
	/// This class manages the splash screen you see when you start up the game.
	class Splash : ExampleApplication
	{
	public:
		Splash();
		void Show();
		void Increment(std::string text);

	private:
		void createScene();

	private:
		static const int maximum;
		static const char* const LoadingPicture;
	};
}

#endif // SPLASH_H_INCLUDED
