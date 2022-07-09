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
			=> "/connect username archipelago.gg port";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			ArchipelagoTerraria.ConnectToServer(input, args);
		}
	}
}