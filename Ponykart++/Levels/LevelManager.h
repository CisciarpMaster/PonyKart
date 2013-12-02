#ifndef LEVELMANAGER_H_INCLUDED
#define LEVELMANAGER_H_INCLUDED

#include <string>
#include <functional>
#include <Ogre.h>
#include "Levels/Level.h"
#include "Levels/LevelChangeRequest.h"
#include "Levels/LevelChangedEventArgs.h"

// TODO: Events are basically function pointers. Implement them. You may need to have vectors of them for each event.
// TODO: Apparently, Mogre has an event system. Fint the equivalent in Ogre.

namespace Ponykart
{
namespace Levels
{

// TODO: This is probably how you're supposed to define LevelEvent, since it's apparently a type. Probably
using LevelEvent =  std::vector<std::function<void (LevelChangedEventArgs)>>;
using LevelProgressEvent =  std::vector<std::function<void (LevelChangedEventArgs,std::string)>>;

class LevelManager
{
public:
	explicit LevelManager();
	void RunPostInitEvents(); // runs level manager stuff that needs to run immediately after kernel setup
	void LoadLevel(LevelChangeRequest request, float delay = INITIAL_DELAY); // Loads a level! (delay: Minimum time to wait (in seconds) before we load the level, to let stuff like loading screens have a chance to render.)
	// Getters
	Level getCurrentLevel();
	bool getIsValidLevel();

private:
	void UnloadLevel(LevelChangedEventArgs eventArgs); // Unloads the current level
	void LoadLevelNow(LevelChangedEventArgs args); // Unloads the current level and loads the new level
	// TODO: Fix FrameEvent not being defined
	//bool DelayedRun_FrameStarted(FrameEvent evt, float delay, void (*action)(LevelChangedEventArgs), LevelChangedEventArgs args); // Runs something after both the specified time has passed and two frames have been rendered.
	void Detach(); // Unhook from the frame started event
	void Invoke(LevelEvent e, LevelChangedEventArgs args); // Helper
	void InvokeLevelProgress(LevelChangedEventArgs args, std::string message);

private:
	Level currentLevel;
	bool hasRunPostInitEvents;
	bool IsValidLevel;
	static const float INITIAL_DELAY; // time to wait until we run the event
	float elapsed; // used in the FrameStarted method
	bool frameOneRendered, frameTwoRendered; // keeps track of how many frames we've rendered
	// a little hacky workaround so we can still have a FrameStarted event run but with a few extra arguments
	//Ogre::FrameListener.FrameStartedHandler preUnloadFrameStartedHandler; //TODO: Fix those
	//Ogre::FrameListener.FrameStartedHandler postLoadFrameStartedHandler;
public:
	static LevelProgressEvent OnLevelLoadProgress;
	static LevelEvent OnLevelPreUnload; // Is fired a few frames before we even start unloading anything. Mostly used for stuff that still requires screen rendering, such as putting up a loading screen
	static LevelEvent OnLevelLoad; // Is fired after the .muffins have been read and the .scene file has been used, but before we start actually creating any Things
	static LevelEvent OnLevelUnload; // Is fired after the level handlers have been disposed, but before we clean out the scene manager.
	static LevelEvent OnLevelPostLoad; // Is fired a few frames after the entire level load process, including after scripts have been run.
};

} // Levels
} // Ponykart

#endif // LEVELMANAGER_H_INCLUDED
