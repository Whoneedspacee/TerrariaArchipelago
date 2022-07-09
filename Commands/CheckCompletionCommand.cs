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
	public class CheckCompletionCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "checkcompletion";

		public override string Description
			=> "/checkcompletion amount";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if(args.Length == 0)
            {
				args = new string[] { "5" };
            }
			int amount;
			bool isNumber = Int32.TryParse(args[0], out amount);
			if(!isNumber)
            {
				Main.NewText("Please insert a number for the amount.");
            }
			Main.NewText("Achievements Not Yet Completed: ");
			foreach(Achievement achievement in AchievementManager.achievements.Keys)
            {
				if(amount <= 0)
                {
					return;
                }
				if (AchievementManager.achievements_completed.Contains(achievement))
				{
					continue;
				}
				Main.NewText(achievement.FriendlyName + " Is Not Completed.");
				amount--;
            }
		}
	}
}