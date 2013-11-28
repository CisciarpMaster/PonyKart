#ifndef LAUNCH_H_INCLUDED
#define LAUNCH_H_INCLUDED

#include <string>

namespace Ponykart
{
class Launch // C#-static class : All member variables/functions should be static.
{
public:
	Launch() = delete;
	static void Main();
	static void InitializeOgre();
	static void Log(std::string message);

public:
	static bool Quit;
};
}

#endif // LAUNCH_H_INCLUDED
