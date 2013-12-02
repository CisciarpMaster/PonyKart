#include "Levels/LevelChangedEventArgs.h"

using namespace Ponykart::Levels;

LevelChangedEventArgs::LevelChangedEventArgs(const Level& newLevel, const Level& oldLevel, const LevelChangeRequest& request)
: NewLevel(newLevel), OldLevel(oldLevel), Request(request)
{

}
