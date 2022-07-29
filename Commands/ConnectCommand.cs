using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Terraria;
using Terraria.ModLoader;
using Terraria.Achievements;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Terraria.ID;

namespace Archipelago.Commands
{
	public class ConnectCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "connect";

		public override string Description
			=> "/connect username ip port";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
            Connect(input, args);
        }

        public static void Connect(string input, string[] args)
        {
            if (ArchipelagoTerraria.session != null && ArchipelagoTerraria.session.Socket.Connected)
            {
                Main.NewText("Already Connected to Archipelago Server.");
                return;
            }
            if (args.Length < 2 || args.Length > 3)
            {
                Main.NewText("Invalid Arguments: /connect username ip port");
                return;
            }
            int port = 38281;
            if (args.Length == 3)
            {
                port = Int32.Parse(args[2]);
            }
            ArchipelagoTerraria.session = ArchipelagoSessionFactory.CreateSession(args[1], port);
            // Session Event Handlers
            ArchipelagoTerraria.session.Socket.PacketReceived += ArchipelagoTerraria.OnPacketReceived;
            ArchipelagoTerraria.session.Items.ItemReceived += Managers.ItemManager.OnItemReceived;
            // This will sleep the terraria main thread, so do not call any functions like NewText during this expecting them to work
            // It will simply stall until the login result times out, and then the connected packet will be received
            // After the login result already timed out, meaning you will get no login result but be connected
            LoginResult result = ArchipelagoTerraria.session.TryConnectAndLogin("Terraria", args[0], new Version(0, 3, 3), ItemsHandlingFlags.AllItems, null, null, null);
            if (!result.Successful)
            {
                DisconnectCommand.Disconnect();
                Main.NewText("Failed Connection: " + result.ToString());
                return;
            }
            On.Terraria.Chat.ChatCommandProcessor.ProcessIncomingMessage += ArchipelagoTerraria.OnTerrariaChatMessage;
            Main.NewText("Connected to Archipelago server.");
        }
	}
}