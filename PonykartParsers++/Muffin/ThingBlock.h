#ifndef THINGBLOCK_H_INCLUDED
#define THINGBLOCK_H_INCLUDED

#include <string>
#include <Ogre.h>

#include "MuffinDefinition.h"

namespace PonykartParsers
{
// These represent each Thing in the .muffin files
class ThingBlock : TokenHolder
{
public:
	ThingBlock(std::string thingName, MuffinDefinition owner);
	ThingBlock(std::string thingName, Ogre::Vector3 position);
	ThingBlock(std::string thingName, Ogre::Vector3 position, Ogre::Quaternion orientation);
	void Finish() override;
	// Getters
	std::string getThingName();
	MuffinDefinition getOwner();
	Ogre::Vector3 getPosition();

private: // Set-private public members
	std::string ThingName; // The name of the .thing file this corresponds with
	MuffinDefinition Owner;
	Ogre::Vector3 Position;
};
} // PonykartParsers

#endif // THINGBLOCK_H_INCLUDED
