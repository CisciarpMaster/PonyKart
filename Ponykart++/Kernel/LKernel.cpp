#include "LKernel.h"

using namespace Ponykart;

std::unordered_map<std::string,void*> LKernel::GlobalObjects = std::unordered_map<std::string,void*>{};
std::unordered_map<std::string,void*> LKernel::LevelObjects = std::unordered_map<std::string,void*>{};

void* LKernel::AddGlobalObject(void* object, std::string type)
{
	if (GlobalObjects.find(type) != GlobalObjects.end())
		throw std::string(std::string("Global object already added ") + type);

	GlobalObjects.insert(std::pair<std::string,void*>(type,object));

	return object;
}

template<typename T>  T* LKernel::AddGlobalObject(T* object)
{
	return (T*) AddGlobalObject(object, typeid(T));
}
