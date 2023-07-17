﻿using MapEditorReborn.API.Features.Objects;
using System.Linq;
using UnityEngine;

namespace AutoEvent.Events.Jump.Features
{
    internal class RandomClass
    {
        public static Vector3 GetSpawnPosition(SchematicObject GameMap, bool isScientist)
        {
            if (isScientist)
            {
                return GameMap.AttachedBlocks.Where(x => x.name == "Spawnpoint").ToList().FirstOrDefault().transform.position;
            }
            else
            {
                return GameMap.AttachedBlocks.Where(x => x.name == "Spawnpoint1").ToList().FirstOrDefault().transform.position;
            }
        }
    }
}