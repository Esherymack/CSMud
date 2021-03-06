﻿using System;
using System.Collections.Generic;
using System.Reflection;
using CSMud.Client;
using CSMud.Entity;

namespace CSMud.Utils
{
    public static class CommandUtils
    {
         // Utility func for getting the user's current room ID
        public static int GetCurrentRoomId(User sender, MapBuild WorldMap)
        {
            return WorldMap.Rooms.FindIndex(a => a.Id == sender.CurrRoomId);
        }
        // Utility func for HandleWhoEvent, returns whether or not a room has NPC entities.
        public static bool HasNPCs(User sender, MapBuild WorldMap)
        {
            return WorldMap.Rooms[GetCurrentRoomId(sender, WorldMap)].NPCs.Count != 0;
        }
        // Utility func for finding if a room has Items
        public static bool HasItems(User sender, MapBuild WorldMap)
        {
            return WorldMap.Rooms[GetCurrentRoomId(sender, WorldMap)].Items.Count != 0;
        }
        // Utility func for finding if a room has Doors
        public static bool HasDoors(User sender, MapBuild WorldMap)
        {
            return WorldMap.Rooms[GetCurrentRoomId(sender, WorldMap)].Doors.Count != 0;
        }
        // Utility func for finding if a room has Containers
        public static bool HasContainers(User sender, MapBuild WorldMap)
        {
            return WorldMap.Rooms[GetCurrentRoomId(sender, WorldMap)].Containers.Count != 0;
        }
        // Utility function for string matching
        public static  bool FuzzyEquals(string a, string b)
        {
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        public static string[] splitLine(string msg)
        {
            return msg.Split(new char[] { ' ' }, 2);
        }
        // Gets the intended recipient for trades and whispers
        public static User GetRecipient(string a, List<User> Users)
        {
            lock(Users)
            {
                foreach(User user in Users)
                {
                    if(FuzzyEquals(user.Name, a))
                    {
                        return user;
                    }
                }
                return null;
            }
        }
        // Same as GetRecipient, but for NPCs. Used in combat, trades, conversation.
        public static NPC GetTarget(int room, string a, MapBuild WorldMap)
        {
            foreach (var i in WorldMap.Rooms[room].NPCs)
            {
                if (FuzzyEquals(i.Actual.Name, a))
                {
                    return i.Actual;
                }
            }
            return null;
        }
        // Helps change user stats based on item modifiers
        public static void ChangeStats(Item target, User sender)
        {
            foreach (KeyValuePair<string, int> entry in target.StatIncrease)
            {
                Type type = sender.Player.Stats.GetType();
                PropertyInfo property = type.GetProperty(entry.Key);
                if(FuzzyEquals(entry.Key, "health"))
                {
                    sender.Player.Heal(entry.Value);
                    sender.Connection.SendMessage($"Healed for {entry.Value} health. Current health rating: {sender.Player.Stats.CurrHealth}");
                    return;
                }
                if (entry.Value > 0)
                {
                    int increase = (int)property.GetValue(sender.Player.Stats) + entry.Value;
                    property.SetValue(sender.Player.Stats, increase);
                    sender.Connection.SendMessage($"Current {entry.Key} rating: {increase}");
                    return;
                }
                int decrease = (int)property.GetValue(sender.Player.Stats) - entry.Value;
                if (decrease < 0)
                {
                    decrease = 0;
                }
                property.SetValue(sender.Player.Stats, decrease);
                sender.Connection.SendMessage($"Current {entry.Key} rating: {decrease}");
            }
        }
        // The inverse of ChangeStats, for removing equipped items & handling timeouts on potions.
        public static void RemoveItemChangeStats(Item target, User sender)
        {
            foreach (KeyValuePair<string, int> entry in target.StatIncrease)
            {
                Type type = sender.Player.Stats.GetType();
                PropertyInfo property = type.GetProperty(entry.Key);
                if(entry.Value > 0)
                {
                    int decrease = (int)property.GetValue(sender.Player.Stats) - entry.Value;
                    if(decrease < 0)
                    {
                        decrease = 0;
                    }
                    property.SetValue(sender.Player.Stats, decrease);
                    sender.Connection.SendMessage($"Current {entry.Key} rating: {decrease}");
                    return;
                }
                int increase = (int)property.GetValue(sender.Player.Stats) + entry.Value;
                property.SetValue(sender.Player.Stats, increase);
                sender.Connection.SendMessage($"Current {entry.Key} rating: {increase}");
            }
        }
        // Check if player has a Item in their inventory
        public static bool OwnsItem(User sender, string item)
        {
            foreach(Item thing in sender.Inventory.Items)
            {
                if(FuzzyEquals(thing.Name, item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
