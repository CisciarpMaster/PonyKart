using System;
using System.Linq;
using System.Collections.Generic;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Properties;
using Mogre;


namespace Ponykart.Items
{

    class ItemManager
    {
        //This const governs the frequency of itemdrops.
        //On average, an item should drop every n seconds.
        const int ITEMFREQ = 5;
        public List<Item> activeItems = new List<Item>();
        public List<ItemBox> boxes = new List<ItemBox>();
        Random rand = new Random();
        public ItemManager() {
			Launch.Log("[Loading] Creating ItemManager...");
			//LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
            Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
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
                        activeItems.Add(spawnedItem);
                    }break;
                case "SmartApple":
                    {
                        spawnedItem = new SmartApple(user);
                        activeItems.Add(spawnedItem);
                    } break;
                default:
                    {

                    }break;
            }

            //if(spawnedItem!= null)
            return spawnedItem;
        }
        public void RequestBox(Vector3 pos)
        {
            ItemBox box;
            box = new ItemBox(pos, "SmartApple");
            boxes.Add(box);
        }
        void EveryTenth(object o)
        {
            if (rand.Next(ITEMFREQ * 10) == 1)
            {
                Vector3 spawnpos = LKernel.GetG<PlayerManager>().MainPlayer.NodePosition;
                spawnpos.y += 2;
                RequestBox(spawnpos);
            }
        }
        void OnLevelUnload(LevelChangedEventArgs eventArgs)
        {
            if (eventArgs.OldLevel.Type == LevelType.Race)
            {
                //Clean up any remaining items
                activeItems.Clear();
                boxes.Clear();
            }
        }
    }


}
