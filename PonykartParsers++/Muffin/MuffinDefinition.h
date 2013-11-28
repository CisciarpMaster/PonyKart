#ifndef MUFFINDEFINITION_H_INCLUDED
#define MUFFINDEFINITION_H_INCLUDED

#include <string>
#include <vector>
#include "TokenHolder.h"
//#include "Muffin/ThingBlock.h"

class ThingBlock;

namespace PonykartParsers
{
// Represents a .muffin file
class MuffinDefinition : TokenHolder // TODO: Implement this class and TokenHolder properly
{
public:
	void SetUpDictionaries() override;
	void Finish() override;
	// Getters
	std::string getName();
	std::vector<ThingBlock> getThingBlocks();
	std::vector<std::string> getExtraFiles();
private: // Set-private public members
	std::string Name;
	std::vector<ThingBlock> ThingBlocks;
	std::vector<std::string> ExtraFiles; // Other .muffin files this one should load.
};
} // PonykartParsers

#endif // MUFFINDEFINITION_H_INCLUDED
