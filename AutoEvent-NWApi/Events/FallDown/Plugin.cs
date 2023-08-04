﻿using AutoEvent.Events.Speedrun.Features;
using MapEditorReborn.API.Features.Objects;
using MEC;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Event = AutoEvent.Interfaces.Event;

namespace AutoEvent.Events.FallDown
{
    public class Plugin : Event
    {
        public override string Name { get; set; } = Translation.FallName;
        public override string Description { get; set; } = Translation.FallDescription;
        public override string Author { get; set; } = "KoT0XleB";
        public override string MapName { get; set; } = "FallDown";
        public override string CommandName { get; set; } = "fall";
        public static SchematicObject GameMap { get; set; }
        public static TimeSpan EventTime { get; set; }

        EventHandler _eventHandler;
        public GameObject Lava { get; set; }

        public override void OnStart()
        {
            _eventHandler = new EventHandler();
            EventManager.RegisterEvents(_eventHandler);
            OnEventStarted();
        }
        public override void OnStop()
        {
            EventManager.UnregisterEvents(_eventHandler);
            _eventHandler = null;
            Timing.CallDelayed(10f, () => EventEnd());
        }

        public void OnEventStarted()
        {
            EventTime = new TimeSpan(0, 0, 0);
            GameMap = Extensions.LoadMap(MapName, new Vector3(10f, 1020f, -43.68f), Quaternion.Euler(Vector3.zero), Vector3.one);
            Extensions.PlayAudio("Puzzle.ogg", 15, true, Name);

            foreach (Player player in Player.GetPlayers())
            {
                player.SetRole(RoleTypeId.ClassD, RoleChangeReason.None);
                player.Position = RandomPosition.GetSpawnPosition(Plugin.GameMap);
            }

            Lava = GameMap.AttachedBlocks.First(x => x.name == "Lava");
            Lava.AddComponent<LavaComponent>();

            Timing.RunCoroutine(OnEventRunning(), "fall_run");
        }

        public IEnumerator<float> OnEventRunning()
        {
            for (float time = 15; time > 0; time--)
            {
                Extensions.Broadcast($"{time}", 1);
                yield return Timing.WaitForSeconds(1f);
            }

            List<GameObject> platformes = GameMap.AttachedBlocks.Where(x => x.name == "Platform").ToList();
            GameObject.Destroy(GameMap.AttachedBlocks.First(x => x.name == "Wall"));

            while (Player.GetPlayers().Count(r => r.IsAlive) > 1 && platformes.Count > 1)
            {
                var count = Player.GetPlayers().Count(r => r.IsAlive);
                var time = $"{EventTime.Minutes}:{EventTime.Seconds}";
                Extensions.Broadcast(Translation.FallBroadcast.Replace("%name%", Name).Replace("%time%", time).Replace("%count%", $"{count}"), 1);

                var platform = platformes.RandomItem();
                platformes.Remove(platform);
                GameObject.Destroy(platform);

                yield return Timing.WaitForSeconds(0.9f);
                EventTime += TimeSpan.FromSeconds(0.9f);
            }

            if (Player.GetPlayers().Count(r => r.IsAlive) == 1)
            {
                Extensions.Broadcast(Translation.FallWinner.Replace("%winner%", Player.GetPlayers().First(r => r.IsAlive).Nickname), 10);
            }
            else
            {
                Extensions.Broadcast(Translation.FallDied, 10);
            }
            
            OnStop();
            yield break;
        }

        public void EventEnd()
        {
            Extensions.CleanUpAll();
            Extensions.TeleportEnd();
            Extensions.UnLoadMap(GameMap);
            Extensions.StopAudio();
            AutoEvent.ActiveEvent = null;
        }
    }
}