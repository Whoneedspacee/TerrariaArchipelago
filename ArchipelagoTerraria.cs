using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Terraria.ID;
using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json.Linq;
using Terraria.Localization;
using Terraria.Achievements;
using Archipelago.Items;
using Microsoft.Xna.Framework.Graphics;

namespace Archipelago
{
    public class ArchipelagoTerraria : Mod
    {
        public static ArchipelagoTerraria instance;
        public static ArchipelagoSession session;

        public override void Load()
        {
            instance = this;
            On.Terraria.WorldGen.Hooks.WorldLoaded += OnWorldLoaded;
            On.Terraria.WorldGen.SaveAndQuit += OnSaveAndQuit;
        }

        public override void Unload()
        {
            instance = null;
            On.Terraria.WorldGen.Hooks.WorldLoaded -= OnWorldLoaded;
            On.Terraria.WorldGen.SaveAndQuit -= OnSaveAndQuit;
        }

        public override void PostAddRecipes()
        {
            Managers.ItemManager.PostAddRecipes();
        }

        public static void OnWorldLoaded(On.Terraria.WorldGen.Hooks.orig_WorldLoaded orig)
        {
            orig();
            Managers.AchievementManager.Load();
            Managers.ItemManager.Load();
        }

        // Disconnect from the server when the player saves and quits
        public static void OnSaveAndQuit(On.Terraria.WorldGen.orig_SaveAndQuit orig, Action callback)
        {
            orig(callback);
            Managers.AchievementManager.Unload();
            Managers.ItemManager.Unload();
            Commands.DisconnectCommand.Disconnect();
        }

        // Checks for player sent messages
        public static void OnTerrariaChatMessage(On.Terraria.Chat.ChatCommandProcessor.orig_ProcessIncomingMessage orig,
            Terraria.Chat.ChatCommandProcessor self, Terraria.Chat.ChatMessage message, int clientid)
        {
            orig(self, message, clientid);
            session.Socket.SendPacket(new SayPacket() { Text = message.Text });
        }

        // Main.NewText to avoid sending the message to the Archipelago server
        public static void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            ArchipelagoPacketType type = packet.PacketType;
            if (type == ArchipelagoPacketType.Print)
            {
                PrintPacket received = (PrintPacket)packet;
                Main.NewText(received.Text);
                return;
            }
            if (type == ArchipelagoPacketType.RoomUpdate)
            {
                RoomUpdatePacket received = (RoomUpdatePacket)packet;
                return;
            }
            if (type == ArchipelagoPacketType.PrintJSON)
            {
                /*ItemPrintJsonPacket received = (ItemPrintJsonPacket)packet;
                int playerid = Int32.Parse(received.Data[0].Text);
                long itemid = Int64.Parse(received.Data[2].Text);
                long locationid = Int64.Parse(received.Data[4].Text);
                Main.NewText(session.Players.GetPlayerAlias(playerid) + received.Data[1].Text +
                    session.Items.GetItemName(itemid) + received.Data[3].Text + 
                    session.Locations.GetLocationNameFromId(locationid) + received.Data[5].Text);*/
                return;
            }
            if(type == ArchipelagoPacketType.ReceivedItems)
            {
                ReceivedItemsPacket received = (ReceivedItemsPacket)packet;
                return;
            }
            // This packet comes first when connecting, do not put any terraria main thread actions here such as NewText
            // The Connected packet will stop the main thread from sleeping, so if you put any actions here that can only be done
            // On the main thread, then the game will be slept until the login result times out
            if(type == ArchipelagoPacketType.RoomInfo)
            {
                RoomInfoPacket received = (RoomInfoPacket)packet;
                return;
            }
            if (type == ArchipelagoPacketType.Connected)
            {
                ConnectedPacket received = (ConnectedPacket)packet;
                Managers.AchievementManager.CompleteLocationChecks();
                return;
            }
            Main.NewText("Recieved Unchecked packet type: " + type);
        }
    }
}