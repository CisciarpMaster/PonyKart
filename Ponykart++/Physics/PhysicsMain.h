#ifndef PHYSICSMAIN_H_INCLUDED
#define PHYSICSMAIN_H_INCLUDED

#include "Levels/LevelChangedEventArgs.h"

namespace Ponykart
{
namespace Physics
{
	class PhysicsMain
	{
	public:
		PhysicsMain();
		static void OnLevelUnload(Levels::LevelChangedEventArgs eventArgs); // Deletes the world
	};
} // Physics
} // Ponykart


#endif // PHYSICSMAIN_H_INCLUDED
