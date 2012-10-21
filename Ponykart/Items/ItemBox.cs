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

namespace Ponykart.Items
{
    class ItemBox : LDisposable
    {
        public LThing box { get; protected set; }
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
            box = LKernel.GetG<Spawner>().Spawn("ItemBox", spawnpos);
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
                        p.heldItem = itemName;
                        Dispose(true);
                    }
                }
            }
        }

        public void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                box.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
