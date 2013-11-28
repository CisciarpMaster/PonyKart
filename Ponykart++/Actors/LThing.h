#ifndef LTHING_H_INCLUDED
#define LTHING_H_INCLUDED

#include <string>
#include <Ogre.h>

namespace Ponykart
{
namespace Actors
{
// Our game object class! Pretty much everything you see in the game uses this
class LThing // TODO: Implement correctly. Find the declaration of RigidBody (Bullet ?)
{
public:
	// Getters
	unsigned getID();
	std::string getName();
	Ogre::SceneNode getRootNode();
	//RigidBody getBody();

protected: // Set-protected members
	unsigned ID; // Every lthing has an ID, though it's mostly just used to stop ogre complaining about duplicate names
	std::string Name; // This lthing's name. It's usually the same as its .thing filename.
	Ogre::SceneNode RootNode; // A scene node that all of the model components attach stuff to.
	// Physics! If we have 0 shape components, this is null; if we have 1 shape component, this is a body made from that shape;
	// if we have 2 or more shape components, this is a body made from a compound shape using all of the components' shapes
	//RigidBody Body;
};
} // Actors
} // Ponykart

#endif // LTHING_H_INCLUDED
