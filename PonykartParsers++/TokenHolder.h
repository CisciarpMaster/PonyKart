#ifndef TOKENHOLDER_H_INCLUDED
#define TOKENHOLDER_H_INCLUDED

#include <unordered_map>
#include <string>
#include <Ogre.h>

#include "ThingEnum.h"

namespace PonykartParsers
{
// Since both the .thing and its blocks can all have properties, they all use this abstract class to give them dictionaries and a few helpful methods
class TokenHolder // TODO: Implement properly
{
public:
	TokenHolder()=delete;
	virtual void SetUpDictionaries(); // Constructs the std::unordered_maps
	virtual void Finish();
	ThingEnum GetEnumProperty(std::string propertyName, ThingEnum* defaultValue=nullptr); // Gets an enum property from the dictionaries.
	std::string GetStringProperty(std::string propertyName, std::string defaultValue=std::string()); // Gets a string property from the dictionaries.
	float GetFloatProperty(std::string propertyName, float* defaultValue=nullptr); // Gets a float property from the dictionaries.
	bool GetBoolProperty(std::string propertyName, bool* defaultValue=nullptr); // Gets a boolean property from the dictionaries.
	Ogre::Vector3 GetVectorProperty(std::string propertyName, Ogre::Vector3* defaultValue=nullptr); // Gets a vector property from the dictionaries.
	Ogre::Quaternion GetQuatProperty(std::string propertyName, Ogre::Quaternion* defaultValue); // Gets a quaternion property from the dictionaries.
	// Getters
	std::unordered_map<std::string, ThingEnum> getEnumTokens();
	std::unordered_map<std::string, std::string> getStringTokens();
	std::unordered_map<std::string, float> getFloatTokens();
	std::unordered_map<std::string, bool> getBoolTokens();
	std::unordered_map<std::string, Ogre::Vector3> getVectorTokens();
	std::unordered_map<std::string, Ogre::Quaternion> getQuatTokens();

protected: // Set-protected public members
	std::unordered_map<std::string, ThingEnum> EnumTokens;
	std::unordered_map<std::string, std::string> StringTokens;
	std::unordered_map<std::string, float> FloatTokens;
	std::unordered_map<std::string, bool> BoolTokens;
	std::unordered_map<std::string, Ogre::Vector3> VectorTokens;
	std::unordered_map<std::string, Ogre::Quaternion> QuatTokens;

};
} // PonykartParsers

#endif // TOKENHOLDER_H_INCLUDED
