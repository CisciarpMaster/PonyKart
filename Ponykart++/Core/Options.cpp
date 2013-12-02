#include <fstream>
#include "Core/Options.h"
#include "Ogre.h"

using namespace Ponykart;
using namespace Ponykart::Core;
using namespace Ogre;
using namespace std;

// Define the static members
unordered_map<string, string> Options::dict;
unordered_map<string, string> Options::defaults;
ModelDetailOption Options::ModelDetail;
ShadowDetailOption Options::ShadowDetail;

void Options::SetupDictionaries()
{
	defaults={
		{"FSAA","0"},
		{"Floating-point mode","Fastest"}, // Fastest or Accurate
		{"Full Screen","No"}, // Yes or No
		{"VSync","Yes"}, // Yes or No
		{"VSync Interval","1"}, // 1, 2, 3, or 4
		{"Video Mode","1024 x 768 @ 32-bit colour"},
		{"sRGB Gamma Conversion","No"}, // Yes or No
		{"Music","No"}, // Yes or No
		{"Sounds","Yes"},
		{"Ribbons","Yes"},
		{"ModelDetail","Medium"}, // Low, Medium or High
		{"ShadowDetail","Some"}, // None, Some or Many
		{"ShadowDistance","40"},
		{"Twh","No"},
		{"Controller", "Keyboard"},
	};
	dict = defaults; // copy it into the regular dictionary
}

void Options::Initialize()
{
	SetupDictionaries();

	constexpr char optionsPath[] = "media/config/ponykart.cfg";

	fstream file;
	file.open(optionsPath,ios::in);
	if (!file.is_open()) // create it if the file doesn't exist, and write out some defaults
	{
		file.open(optionsPath,ios::out);
		if (!file.is_open()) throw string("Cannot initialize media/config/ponykart.cfg");
		for (auto setting : defaults)
			file << setting.first << "=" << setting.second << endl;
		file.close();
		ModelDetail = ModelDetailOption::Medium;
		ShadowDetail = ShadowDetailOption::Some;
	}
	else // otherwise we just read from it
	{
		file.close(); // We're going to open it with cfile
		ConfigFile cfile;
		cfile.load(optionsPath, "=", true);
		auto sectionIterator = cfile.getSectionIterator();
		ConfigFile::SettingsMultiMap* settings=sectionIterator.getNext();
		for (auto curPair : *settings)
			dict[curPair.first] = curPair.second;
		// Convert the settings for ModelDetail and ShadowDetail (string) into enum values (int)
		string enumStr = dict["ModelDetail"];
		ModelDetail = enumStr=="High"?ModelDetailOption::High: (enumStr=="Low"?ModelDetailOption::Low: (ModelDetailOption::Medium));
		enumStr = dict["ShadowDetail"];
		ShadowDetail = enumStr=="Many"?ShadowDetailOption::Many: (enumStr=="None"?ShadowDetailOption::None: (ShadowDetailOption::Some));
	}

#ifdef DEBUG
	// since we sometimes add new options, we want to make sure the .ini file has all of them
	Save();
#endif
}

void Options::Save()
{
	constexpr char optionsPath[] = "media/config/ponykart.cfg";

	dict["ModelDetail"] = ModelDetail==ModelDetailOption::High?"High": (ModelDetail==ModelDetailOption::Low?"Low": ("Medium"));
	dict["ShadowDetail"] = ShadowDetail==ShadowDetailOption::Many?"Many": (ShadowDetail==ShadowDetailOption::None?"None": ("Some"));

	fstream file;
	file.open(optionsPath,ios::out);
	if (!file.is_open()) throw string("Cannot save media/config/ponykart.cfg");

	for (auto setting : dict)
		file << setting.first << "=" << setting.second << endl;
}

string Options::Get(const string keyName)
{
	// TODO: The string comparisons should be case-insensitive, like in the C# code
	if (keyName == "ModelDetail")
		throw new string("Options::getBool: Use the Options.ModelDetail enum instead of this method!");
	else if (keyName == "ShadowDetail")
		throw new string("Use the Options.ShadowDetail enum instead of this method!");

	return dict[keyName];
}
