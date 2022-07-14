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
	public class DisconnectCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "disconnect";

		public override string Description
			=> "/disconnect";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Disconnect();
		}

		public static void Disconnect()
        {
			if (ArchipelagoTerraria.session == null || !ArchipelagoTerraria.session.Socket.Connected)
			{
				Main.NewText("Not Currently Connected to Archipelago Server.");
				return;
			}
			On.Terraria.Chat.ChatCommandProcessor.ProcessIncomingMessage -= ArchipelagoTerraria.OnTerrariaChatMessage;
			ArchipelagoTerraria.session.Socket.PacketReceived -= ArchipelagoTerraria.OnPacketReceived;
			ArchipelagoTerraria.session.Items.ItemReceived -= Items.ItemManager.OnItemReceived;
			ArchipelagoTerraria.session.Socket.Disconnect();
			ArchipelagoTerraria.session = null;
			Main.NewText("Disconnected from the Archipelago server.");
		}
	}
}