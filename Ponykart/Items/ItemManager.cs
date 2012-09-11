using System;
using System.Linq;
using System.Collections.Generic;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Items
{
    class ItemManager
    {
        public List<Item> activeItems { get; private set; }

        public ItemManager() {
			Launch.Log("[Loading] Creating ItemManager...");
			//LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

        public Item SpawnItem(Player user, string itemName)
        {
            Item spawnedItem = null;

            //There is probably a better way to do this.
            switch(itemName)
            {
                case "BigApple":
                    {
                        spawnedItem = new BigApple(user);
                        //activeItems.Add(spawnedItem);
                    }break;
                case "SmartApple":
                    {
                        spawnedItem = new SmartApple(user);
                        //activeItems.Add(spawnedItem);
                    } break;
                default:
                    {

                    }break;
            }

            //if(spawnedItem!= null)
            return spawnedItem;
        }

        void OnLevelUnload(LevelChangedEventArgs eventArgs)
        {
            if (eventArgs.OldLevel.Type == LevelType.Race)
            {
                //Clean up any remaining items
                activeItems.Clear();
            }
        }
    }


}
