#ifndef OPTIONS_H_INCLUDED
#define OPTIONS_H_INCLUDED

#include <string>
#include <unordered_map>
#include "Core/OptionsEnums.h"

namespace Ponykart
{
namespace Core
{
class Options // C#-static class : All member variables/functions should be static
{
public:
	 static void Initialize(); // Creates the folder and file if they don't exist, and either prints some data to it (if it doesn't exist) or reads from it (if it does)
	 static void Save(); // Writes out the current settings to our options file
	 static std::string Get(const std::string keyName); // Gets an option.
	 static bool GetBool(const std::string keyName); // Gets an option as a boolean.
	 static void GetWindowDimensions(unsigned* width, unsigned* height); // Gets the dimensions of the render window

private:
	explicit Options();
	static void SetupDictionaries(); // set up our dictionary with some default stuff in it

private:
	static std::unordered_map<std::string, std::string> dict;
	static std::unordered_map<std::string, std::string> defaults;
public:
	static ModelDetailOption ModelDetail;
	static ShadowDetailOption ShadowDetail;
};
} // Core
} // Ponykart

#endif // OPTIONS_H_INCLUDED
