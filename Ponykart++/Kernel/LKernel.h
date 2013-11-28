#ifndef LKERNEL_H_INCLUDED
#define LKERNEL_H_INCLUDED

#include <unordered_map>
#include <typeinfo>

#include <OgreTextAreaOverlayElement.h>
#include <OgreFontManager.h>

#include "UI/Splash.h"

namespace Ponykart
{
	class LKernel // C#-static class : All member variables/functions should be static
	{
	public:
		static void Initialize();
		static void LoadInitialObjects(Splash splash);
		static void* AddGlobalObject(void* object, std::string typeName);
		template<typename T> static T* AddGlobalObject(T* object);

	private:
		LKernel();

	private:
		static std::unordered_map<std::string,void*> GlobalObjects; // Contain one object of a given type.
		static std::unordered_map<std::string,void*> LevelObjects;
	};
}

#endif // LKERNEL_H_INCLUDED
