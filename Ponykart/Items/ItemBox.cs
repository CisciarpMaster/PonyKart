using System;
using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Properties;
using Ponykart.Players;
using Ponykart.Physics;
using PonykartParsers;
using Ponykart.Sound;
using Ponykart.UI;

namespace Ponykart.Items
{
    class ItemBox : LDisposable
    {
        private LThing box;
        private GameUIManager ui = LKernel.GetG<GameUIManager>();
        public Item contents;
        protected string itemName;
        public ItemBox(Vector3 spawnpos, string itemName)
        {
            Init(spawnpos, itemName);
        }
        protected void Init(Vector3 spawnpos, string _itemName)
        {
            LKernel.GetG<CollisionReporter>().AddEvent(PonykartCollisionGroups.Karts, PonykartCollisionGroups.Default, OnCol);
            contents = LKernel.GetG<ItemManager>().SpawnItem(null, itemName);
            itemName = _itemName;
            this.box = LKernel.GetG<Spawner>().Spawn("Barrel", spawnpos);
        }

        void OnCol(CollisionReportInfo info)
        {
            if (info.FirstGroup == PonykartCollisionGroups.Karts)
            {
                foreach (Player p in LKernel.GetG<PlayerManager>().Players)
                {
                    if (info.FirstObject.GetHashCode() == p.Kart.Body.GetHashCode())
                    {
                        p.hasItem = true;
                        ui.SetItemLevel(1);
                        ui.SetItemImage(itemName);
                        p.heldItem = itemName;
                        LKernel.GetG<SoundMain>().Play3D("Item Get.wav", p.NodePosition, false);                       
                        //DummyItem dummy = new DummyItem(itemName, p);
                        //LKernel.GetG<CollisionReporter>().RemoveEvent(PonykartCollisionGroups.Karts, PonykartCollisionGroups.Default, OnCol);
                        Dispose(true);
                    }
                }
                
            }
        }
        
        public new void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            this.box.Dispose();
            if (disposing)
            {
                //this.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
