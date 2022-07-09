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
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.Localization;

namespace Archipelago
{
    public static class ItemManager
    {

        public static int crafting_progression = 0;

        public static int PROGRESSIVE_CRAFTING_ITEM = 22000;
        public static int MOB_TRAP_ITEM = 22200;
        public static int BOULDER_TRAP_ITEM = 22201;
        public static int LAVA_TRAP_ITEM = 22202;
        public static int ICE_TRAP_ITEM = 22203;
        public static int GRAVITY_FLIP_TRAP_ITEM = 22204;
        public static int BLOOD_MOON_TRAP_ITEM = 22205;
        public static Dictionary<int, short> items_by_id = new Dictionary<int, short>();
        public static string[] crafting_progression_string = { "Copper" , "Iron", "Silver", "Gold", "Shadow", "Molten",
            "Cobalt", "Mythril", "Adamantite", "Hallowed", "Chlorophyte", "Lunar"};

        public static void Load()
        {
            items_by_id.Add(22101, ItemID.WoodenCrate);
            items_by_id.Add(22102, ItemID.IronCrate);
            items_by_id.Add(22103, ItemID.GoldenCrate);
            items_by_id.Add(22104, ItemID.JungleFishingCrate);
            items_by_id.Add(22105, ItemID.FloatingIslandFishingCrate);
            items_by_id.Add(22106, ItemID.CorruptFishingCrate);
            items_by_id.Add(22107, ItemID.CrimsonFishingCrate);
            items_by_id.Add(22108, ItemID.HallowedFishingCrate);
            items_by_id.Add(22109, ItemID.DungeonFishingCrate);
            items_by_id.Add(22110, ItemID.FrozenCrate);
            items_by_id.Add(22111, ItemID.OasisCrate);
            items_by_id.Add(22112, ItemID.LavaCrate);
            items_by_id.Add(22113, ItemID.OceanCrate);
        }

        public static void Unload()
        {

        }

        public static void ArchipelagoItemReceived(int archipelago_item_id)
        {
            // Crafting Progression
            if (archipelago_item_id == PROGRESSIVE_CRAFTING_ITEM)
            {
                crafting_progression++;
                Main.NewText("Crafting Progression Upgraded to Tier " + crafting_progression_string[crafting_progression - 1]);
                return;
            }
            // Traps
            if (archipelago_item_id == MOB_TRAP_ITEM)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active)
                        continue;
                    player.TeleportationPotion();
                    Main.NewText(player.name + " Was Randomly Teleported.");
                }
                Main.NewText("Random Teleportation Trap Activated.");
                return;
            }
            else if (archipelago_item_id == BOULDER_TRAP_ITEM)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active)
                        continue;
                    NPC.SpawnOnPlayer(player.whoAmI, NPCID.QueenBee);
                    Main.NewText(player.name + " Got Fucked.");
                }
                Main.NewText("Queen Bee Trap Activated.");
                return;
            }
            else if (archipelago_item_id == LAVA_TRAP_ITEM)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active)
                        continue;
                    NPC.SpawnWOF(new Vector2(0, 0));
                    Main.NewText(player.name + " Got Fucked.");
                }
                Main.NewText("WoF Trap Activated");
                return;
            }
            else if (archipelago_item_id == ICE_TRAP_ITEM)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active)
                        continue;
                    player.AddBuff(BuffID.Frozen, 5 * 60);
                    Main.NewText(player.name + " Was Ice Trapped");
                }
                Main.NewText("Ice Trap Activated");
                return;
            }
            else if (archipelago_item_id == GRAVITY_FLIP_TRAP_ITEM)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active)
                        continue;
                    player.AddBuff(BuffID.VortexDebuff, 30 * 60);
                    Main.NewText(player.name + " Was Gravity Trapped");
                }
                Main.NewText("Gravity Trap Activated");
                return;
            }
            else if (archipelago_item_id == BLOOD_MOON_TRAP_ITEM)
            {
                Main.bloodMoon = true;
                Main.NewText("Blood Moon Trap Activated");
                return;
            }
            // Generic Items
            short terraria_item_id;
            bool gotten;
            gotten = items_by_id.TryGetValue(archipelago_item_id, out terraria_item_id);
            if (!gotten)
            {
                Main.NewText("Unknown Item Received With Archipelago Id: " + archipelago_item_id);
                return;
            }
            foreach (Player player in Main.player)
            {
                if (!player.active)
                    continue;
                player.QuickSpawnItem(null, terraria_item_id, 1);
                Item item = new Item(terraria_item_id);
                Main.NewText(player.name + " Received Item: " + item.Name);
            }
            return;
        }

        public static void PostAddRecipes()
        {
            // Thanks to Exterminator on the tModLoader discord for helping with this
            Recipe.Condition copper = new(NetworkText.FromLiteral("Archipelago.Copper"), r => crafting_progression >= 1);
            Recipe.Condition iron = new(NetworkText.FromLiteral("Archipelago.Iron"), r => crafting_progression >= 2);
            Recipe.Condition silver = new(NetworkText.FromLiteral("Archipelago.Silver"), r => crafting_progression >= 3);
            Recipe.Condition gold = new(NetworkText.FromLiteral("Archipelago.Gold"), r => crafting_progression >= 4);
            Recipe.Condition shadow = new(NetworkText.FromLiteral("Archipelago.Shadow"), r => crafting_progression >= 5);
            Recipe.Condition molten = new(NetworkText.FromLiteral("Archipelago.Molten"), r => crafting_progression >= 6);
            Recipe.Condition cobalt = new(NetworkText.FromLiteral("Archipelago.Cobalt"), r => crafting_progression >= 7);
            Recipe.Condition mythril = new(NetworkText.FromLiteral("Archipelago.Mythril"), r => crafting_progression >= 8);
            Recipe.Condition adamantite = new(NetworkText.FromLiteral("Archipelago.Adamantite"), r => crafting_progression >= 9);
            Recipe.Condition hallowed = new(NetworkText.FromLiteral("Archipelago.Hallowed"), r => crafting_progression >= 10);
            Recipe.Condition chlorophyte = new(NetworkText.FromLiteral("Archipelago.Chlorophyte"), r => crafting_progression >= 11);
            Recipe.Condition lunar = new(NetworkText.FromLiteral("Archipelago.Lunar"), r => crafting_progression >= 12);

            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                var recipe = Main.recipe[i];
                if (recipe.Disabled)
                    continue;
                // Pre-Hardmode (6 Tiers)
                if (recipe.HasIngredient(ItemID.CopperBar) || recipe.HasIngredient(ItemID.TinBar))
                {
                    recipe.AddCondition(copper);
                }
                else if (recipe.HasIngredient(ItemID.IronBar) || recipe.HasIngredient(ItemID.LeadBar))
                {
                    recipe.AddCondition(iron);
                }
                else if (recipe.HasIngredient(ItemID.SilverBar) || recipe.HasIngredient(ItemID.TungstenBar))
                {
                    recipe.AddCondition(silver);
                }
                else if (recipe.HasIngredient(ItemID.GoldBar) || recipe.HasIngredient(ItemID.PlatinumBar) ||
                    recipe.HasIngredient(ItemID.FossilOre) || recipe.HasIngredient(ItemID.BeeWax))
                {
                    recipe.AddCondition(gold);
                }
                else if (recipe.HasIngredient(ItemID.DemoniteBar) || recipe.HasIngredient(ItemID.CrimtaneBar) ||
                    recipe.HasIngredient(ItemID.Obsidian) || recipe.HasIngredient(ItemID.MeteoriteBar) ||
                    recipe.HasIngredient(ItemID.Stinger) || recipe.HasIngredient(ItemID.JungleSpores) || recipe.HasIngredient(ItemID.Vine))
                {
                    recipe.AddCondition(shadow);
                }
                else if (recipe.HasIngredient(ItemID.HellstoneBar))
                {
                    recipe.AddCondition(molten);
                }
                // Hardmode (6 Tiers)
                else if (recipe.HasIngredient(ItemID.CobaltBar) || recipe.HasIngredient(ItemID.PalladiumBar) ||
                    recipe.HasIngredient(ItemID.SpiderFang))
                {
                    recipe.AddCondition(cobalt);
                }
                else if (recipe.HasIngredient(ItemID.MythrilBar) || recipe.HasIngredient(ItemID.OrichalcumBar))
                {
                    recipe.AddCondition(mythril);
                }
                else if (recipe.HasIngredient(ItemID.AdamantiteBar) || recipe.HasIngredient(ItemID.TitaniumBar))
                {
                    recipe.AddCondition(adamantite);
                }
                else if (recipe.HasIngredient(ItemID.HallowedBar))
                {
                    recipe.AddCondition(hallowed);
                }
                else if (recipe.HasIngredient(ItemID.ChlorophyteBar) || recipe.HasIngredient(ItemID.SpookyWood))
                {
                    recipe.AddCondition(chlorophyte);
                }
                else if (recipe.HasIngredient(ItemID.FragmentSolar) || recipe.HasIngredient(ItemID.FragmentNebula) ||
                    recipe.HasIngredient(ItemID.FragmentStardust) || recipe.HasIngredient(ItemID.FragmentVortex))
                {
                    recipe.AddCondition(lunar);
                }
            }
        }

    }
}