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
            ItemManager.PostAddRecipes();
        }

        public static void OnWorldLoaded(On.Terraria.WorldGen.Hooks.orig_WorldLoaded orig)
        {
            orig();
            AchievementManager.Load();
            ItemManager.Load();
        }

        // Disconnect from the server when the player saves and quits
        public static void OnSaveAndQuit(On.Terraria.WorldGen.orig_SaveAndQuit orig, Action callback)
        {
            orig(callback);
            AchievementManager.Unload();
            ItemManager.Unload();
            DisconnectFromServer();
        }

        public static void ConnectToServer(string input, string[] args)
        {
            if(args.Length != 3)
            {
                Main.NewText("Invalid Arguments.");
                return;
            }
            if (session != null && session.Socket.Connected)
            {
                Main.NewText("Already Connected to Archipelago Server.");
                return;
            }
            int port = Int32.Parse(args[2]);
            session = ArchipelagoSessionFactory.CreateSession(args[1], port);
            LoginResult result = session.TryConnectAndLogin("Terraria", args[0], new Version(0, 3, 2), ItemsHandlingFlags.NoItems, null, null, null);
            if (!result.Successful)
            {
                Main.NewText(result.ToString());
                return;
            }
            On.Terraria.Chat.ChatCommandProcessor.ProcessIncomingMessage += OnTerrariaChatMessage;
            session.Socket.PacketReceived += OnPacketReceived;
            AchievementManager.CompleteLocationChecks();
            Main.NewText("Connected to Archipelago server.");
        }

        public static void DisconnectFromServer()
        {
            if (session == null || !session.Socket.Connected)
            {
                Main.NewText("Not Currently Connected to Archipelago Server.");
                return;
            }
            On.Terraria.Chat.ChatCommandProcessor.ProcessIncomingMessage -= OnTerrariaChatMessage;
            session.Socket.PacketReceived -= OnPacketReceived;
            session.Socket.Disconnect();
            Main.NewText("Disconnected from the Archipelago server.");
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
                if(!received.Text.Contains("Space:"))
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
                ItemPrintJsonPacket received = (ItemPrintJsonPacket)packet;
                int playerid = Int32.Parse(received.Data[0].Text);
                long itemid = Int64.Parse(received.Data[2].Text);
                long locationid = Int64.Parse(received.Data[4].Text);
                Main.NewText(session.Players.GetPlayerAlias(playerid) + received.Data[1].Text +
                    session.Items.GetItemName(itemid) + received.Data[3].Text + 
                    session.Locations.GetLocationNameFromId(locationid) + received.Data[5].Text);
                if(session.ConnectionInfo.Slot != received.ReceivingPlayer)
                {
                    return;
                }
                int archipelago_item_id = received.Item.Item;
                ItemManager.ArchipelagoItemReceived(archipelago_item_id);
                return;
            }
            Main.NewText("Recieved Unchecked packet type: " + type);
        }
    }
}