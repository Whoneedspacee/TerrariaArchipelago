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

namespace Archipelago.Managers
{
    public static class AchievementManager
    {

        public static Dictionary<Achievement, List<AchievementCondition>> achievements = new Dictionary<Achievement, List<AchievementCondition>>();
        public static List<Achievement> achievements_completed = new List<Achievement>();
        public static List<AchievementCondition> achievement_conditions_completed = new List<AchievementCondition>();
        public static Dictionary<Achievement, int> achievements_by_id = new Dictionary<Achievement, int>();

        public static void Load()
        {
            On.Terraria.Achievements.Achievement.AddCondition += OnAddCondition;
            On.Terraria.Achievements.AchievementCondition.Complete += OnConditionComplete;
            CreateAchievements();
        }

        public static void Unload()
        {
            On.Terraria.Achievements.Achievement.AddCondition -= OnAddCondition;
            On.Terraria.Achievements.AchievementCondition.Complete -= OnConditionComplete;
        }

        public static void OnAddCondition(On.Terraria.Achievements.Achievement.orig_AddCondition orig,
            Achievement self, AchievementCondition condition)
        {
            orig(self, condition);
            List<AchievementCondition> conditions;
            bool gotten = achievements.TryGetValue(self, out conditions);
            if (!gotten)
            {
                conditions = new List<AchievementCondition>();
            }
            conditions.Add(condition);
            achievements[self] = conditions;
            ArchipelagoTerraria.instance.Logger.Info(self.Name + " Achievement Loaded with condition " + condition.Name);
        }

        public static void OnConditionComplete(On.Terraria.Achievements.AchievementCondition.orig_Complete orig,
            AchievementCondition self)
        {
            orig(self);
            if(!achievement_conditions_completed.Contains(self))
            {
                achievement_conditions_completed.Add(self);
            }
            // Not exactly efficient but shouldn't matter too much
            foreach (Achievement achievement in achievements.Keys)
            {
                if(achievements_completed.Contains(achievement))
                {
                    continue;
                }
                List<AchievementCondition> conditions = achievements[achievement];
                bool complete = true;
                foreach (AchievementCondition condition in conditions)
                {
                    if (!achievement_conditions_completed.Contains(condition))
                    {
                        complete = false;
                        break;
                    }
                }
                if(!complete)
                {
                    continue;
                }
                achievements_completed.Add(achievement);
            }
            CompleteLocationChecks();
        }

        public static void CompleteLocationChecks()
        {
            if(ArchipelagoTerraria.session == null || !ArchipelagoTerraria.session.Socket.Connected)
            {
                return;
            }
            foreach(Achievement achievement in achievements_completed)
            {
                // Let the Archipelago Server handle printing
                // Main.NewText(achievement.FriendlyName + " Was Completed.");
                // Game Completion Check
                if (achievements_by_id[achievement] == 0)
                {
                    // Goal State
                    ArchipelagoTerraria.session.Socket.SendPacket(new StatusUpdatePacket { Status = ArchipelagoClientState.ClientGoal });
                    return;
                }
                ArchipelagoTerraria.session.Locations.CompleteLocationChecks(achievements_by_id[achievement]);
            }
        }

        public static void CreateAchievements()
        {
            /* Straight up copied from the AchievementInitializer minus the registering
             * Reasoning for this is for locally mod stored achievements
             * As well as the fact that you cannot double register achievements so re-loading achievements in a mod is impossible
             * Since achievement loading happens before mod loading this is the only way I know of to get the achievements
             * Also tmodloader obviously disables achievements and such so we have to do some weird stuff
             * 
             * Thankfully the conditions themselves will still track pickups and such, so we just need to track
             * what conditions are completed to see if the achievement is completed since all tmodloader does is
             * immediately return the methods for conditions and achievements completing
             * 
             * On the bright side this method makes adding custom achievements easier so it's probably the way
             * I should've done it in the first place
             * 
             * This also lets us hook the ids and achievements together
             * Custom flag conditions will have to be manually re-added unfortunately
             */
            Achievement achievement1 = new Achievement("TIMBER");
            achievement1.AddCondition(ItemPickupCondition.Create((short)9, (short)619, (short)2504, (short)620, (short)2503, (short)2260, (short)621, (short)911, (short)1729));
            achievements_by_id.Add(achievement1, 22500);

            Achievement achievement2 = new Achievement("NO_HOBO");
            achievement2.AddCondition((AchievementCondition)ProgressionEventCondition.Create(8));
            achievements_by_id.Add(achievement2, 22501);

            Achievement achievement3 = new Achievement("OBTAIN_HAMMER");
            achievement3.AddCondition(ItemPickupCondition.Create((short)2775, (short)2746, (short)3505, (short)654, (short)3517, (short)7, (short)3493, (short)2780, (short)1513, (short)2516, (short)660, (short)3481, (short)657, (short)922, (short)3511, (short)2785, (short)3499, (short)3487, (short)196, (short)367, (short)104, (short)797, (short)2320, (short)787, (short)1234, (short)1262, (short)3465, (short)204, (short)217, (short)1507, (short)3524, (short)3522, (short)3525, (short)3523, (short)4317, (short)1305));
            achievements_by_id.Add(achievement3, 22502);

            Achievement achievement4 = new Achievement("OOO_SHINY");
            achievement4.AddCondition(TileDestroyedCondition.Create((ushort)7, (ushort)6, (ushort)9, (ushort)8, (ushort)166, (ushort)167, (ushort)168, (ushort)169, (ushort)22, (ushort)204, (ushort)58, (ushort)107, (ushort)108, (ushort)111, (ushort)221, (ushort)222, (ushort)223, (ushort)211));
            achievements_by_id.Add(achievement4, 22503);

            Achievement achievement5 = new Achievement("HEART_BREAKER");
            achievement5.AddCondition(TileDestroyedCondition.Create((ushort)12));
            achievements_by_id.Add(achievement5, 22504);

            Achievement achievement6 = new Achievement("THROWING_LINES");
            achievement6.AddCondition(CustomFlagCondition.Create("Use"));
            achievements_by_id.Add(achievement6, 22505);

            Achievement achievement7 = new Achievement("YOU_CAN_DO_IT");
            achievement7.AddCondition((AchievementCondition)ProgressionEventCondition.Create(1));
            achievements_by_id.Add(achievement7, 22506);

            Achievement achievement8 = new Achievement("BENCHED");
            achievement8.AddCondition(ItemCraftCondition.Create(ItemID.Sets.Workbenches));
            achievements_by_id.Add(achievement8, 22507);

            Achievement achievement9 = new Achievement("I_AM_LOOT");
            achievement9.AddCondition(CustomFlagCondition.Create("Peek"));
            achievements_by_id.Add(achievement9, 22508);

            Achievement achievement10 = new Achievement("STAR_POWER");
            achievement10.AddCondition(CustomFlagCondition.Create("Use"));
            achievements_by_id.Add(achievement10, 22509);

            Achievement achievement11 = new Achievement("HEAVY_METAL");
            achievement11.AddCondition(ItemPickupCondition.Create((short)35, (short)716));
            achievements_by_id.Add(achievement11, 22510);

            Achievement achievement12 = new Achievement("MATCHING_ATTIRE");
            achievement12.AddCondition(CustomFlagCondition.Create("Equip"));
            achievements_by_id.Add(achievement12, 22511);

            Achievement achievement13 = new Achievement("THE_CAVALRY");
            achievement13.AddCondition(CustomFlagCondition.Create("Equip"));
            achievements_by_id.Add(achievement13, 22512);

            Achievement achievement14 = new Achievement("FASHION_STATEMENT");
            achievement14.AddCondition(CustomFlagCondition.Create("Equip"));
            achievements_by_id.Add(achievement14, 22513);

            Achievement achievement15 = new Achievement("SLIPPERY_SHINOBI");
            achievement15.AddCondition(NPCKilledCondition.Create((short)50));
            achievements_by_id.Add(achievement15, 22514);

            Achievement achievement16 = new Achievement("LIKE_A_BOSS");
            achievement16.AddCondition(ItemPickupCondition.Create((short)1133, (short)1331, (short)1307, (short)267, (short)1293, (short)557, (short)544, (short)556, (short)560, (short)43, (short)70));
            achievements_by_id.Add(achievement16, 22515);

            Achievement achievement17 = new Achievement("HOLD_ON_TIGHT");
            achievement17.AddCondition(CustomFlagCondition.Create("Equip"));
            achievements_by_id.Add(achievement17, 22516);

            Achievement achievement18 = new Achievement("EYE_ON_YOU");
            achievement18.AddCondition(NPCKilledCondition.Create((short)4));
            achievements_by_id.Add(achievement18, 22517);

            Achievement achievement19 = new Achievement("SMASHING_POPPET");
            achievement19.AddCondition((AchievementCondition)ProgressionEventCondition.Create(7));
            achievements_by_id.Add(achievement19, 22518);

            // Shares a location ID
            Achievement achievement20 = new Achievement("WORM_FODDER");
            achievement20.AddCondition(NPCKilledCondition.Create((short)13, (short)14, (short)15));
            Achievement achievement21 = new Achievement("MASTERMIND");
            achievement21.AddCondition(NPCKilledCondition.Create((short)266));
            achievements_by_id.Add(achievement20, 22519);
            achievements_by_id.Add(achievement21, 22519);

            Achievement achievement22 = new Achievement("COMPLETELY_AWESOME");
            achievement22.AddCondition(ItemPickupCondition.Create((short)98));
            achievements_by_id.Add(achievement22, 22520);

            Achievement achievement23 = new Achievement("VEHICULAR_MANSLAUGHTER");
            achievement23.AddCondition(CustomFlagCondition.Create("Hit"));
            achievements_by_id.Add(achievement23, 22521);

            Achievement achievement24 = new Achievement("INTO_ORBIT");
            achievement24.AddCondition(CustomFlagCondition.Create("Reach"));
            achievements_by_id.Add(achievement24, 22522);

            Achievement achievement25 = new Achievement("WHERES_MY_HONEY");
            achievement25.AddCondition(CustomFlagCondition.Create("Reach"));
            achievements_by_id.Add(achievement25, 22523);

            Achievement achievement26 = new Achievement("STING_OPERATION");
            achievement26.AddCondition(NPCKilledCondition.Create((short)222));
            achievements_by_id.Add(achievement26, 22524);

            Achievement achievement27 = new Achievement("NOT_THE_BEES");
            achievement27.AddCondition(CustomFlagCondition.Create("Use"));
            achievements_by_id.Add(achievement27, 22525);

            Achievement achievement28 = new Achievement("BONED");
            achievement28.AddCondition(NPCKilledCondition.Create((short)35));
            achievements_by_id.Add(achievement28, 22526);

            Achievement achievement29 = new Achievement("DUNGEON_HEIST");
            achievement29.AddCondition(ItemPickupCondition.Create((short)327));
            achievement29.AddCondition((AchievementCondition)ProgressionEventCondition.Create(19));
            achievements_by_id.Add(achievement29, 22527);

            Achievement achievement30 = new Achievement("ITS_GETTING_HOT_IN_HERE");
            achievement30.AddCondition(CustomFlagCondition.Create("Reach"));
            achievements_by_id.Add(achievement30, 22528);

            Achievement achievement31 = new Achievement("ROCK_BOTTOM");
            achievement31.AddCondition(CustomFlagCondition.Create("Reach"));
            achievements_by_id.Add(achievement31, 22529);

            Achievement achievement32 = new Achievement("GOBLIN_PUNTER");
            achievement32.AddCondition((AchievementCondition)ProgressionEventCondition.Create(10));
            achievements_by_id.Add(achievement32, 22530);

            Achievement achievement33 = new Achievement("MINER_FOR_FIRE");
            achievement33.AddCondition(ItemCraftCondition.Create((short)122));
            achievements_by_id.Add(achievement33, 22531);

            Achievement achievement34 = new Achievement("STILL_HUNGRY");
            achievement34.AddCondition(NPCKilledCondition.Create((short)113, (short)114));
            achievements_by_id.Add(achievement34, 22532);

            Achievement achievement35 = new Achievement("ITS_HARD");
            achievement35.AddCondition((AchievementCondition)ProgressionEventCondition.Create(9));
            achievements_by_id.Add(achievement35, 22533);

            Achievement achievement36 = new Achievement("BEGONE_EVIL");
            achievement36.AddCondition((AchievementCondition)ProgressionEventCondition.Create(6));
            achievements_by_id.Add(achievement36, 22534);

            Achievement achievement37 = new Achievement("EXTRA_SHINY");
            achievement37.AddCondition(TileDestroyedCondition.Create((ushort)107, (ushort)108, (ushort)111, (ushort)221, (ushort)222, (ushort)223));
            achievements_by_id.Add(achievement37, 22535);

            Achievement achievement38 = new Achievement("HEAD_IN_THE_CLOUDS");
            achievement38.AddCondition(CustomFlagCondition.Create("Equip"));
            achievements_by_id.Add(achievement38, 22536);

            Achievement achievement39 = new Achievement("BULLDOZER");
            achievement39.AddCondition(CustomIntCondition.Create("Pick", 10000));
            achievement39.UseTrackerFromCondition("Pick");
            achievements_by_id.Add(achievement39, 22537);

            Achievement achievement40 = new Achievement("BLOODBATH");
            achievement40.AddCondition((AchievementCondition)ProgressionEventCondition.Create(5));
            achievements_by_id.Add(achievement40, 22538);

            Achievement achievement41 = new Achievement("JEEPERS_CREEPERS");
            achievement41.AddCondition(CustomFlagCondition.Create("Reach"));
            achievements_by_id.Add(achievement41, 22539);

            Achievement achievement42 = new Achievement("BUCKETS_OF_BOLTS");
            achievement42.AddCondition(NPCKilledCondition.Create((short)125, (short)126));
            achievement42.AddConditions(NPCKilledCondition.CreateMany((short)sbyte.MaxValue, (short)134));
            achievement42.UseConditionsCompletedTracker();
            achievements_by_id.Add(achievement42, 22540);

            Achievement achievement43 = new Achievement("DRAX_ATTAX");
            achievement43.AddCondition(ItemCraftCondition.Create((short)579, (short)990));
            achievements_by_id.Add(achievement43, 22541);

            Achievement achievement44 = new Achievement("PHOTOSYNTHESIS");
            achievement44.AddCondition(TileDestroyedCondition.Create((ushort)211));
            achievements_by_id.Add(achievement44, 22542);

            Achievement achievement45 = new Achievement("GET_A_LIFE");
            achievement45.AddCondition(CustomFlagCondition.Create("Use"));
            achievements_by_id.Add(achievement45, 22543);

            Achievement achievement46 = new Achievement("THE_GREAT_SOUTHERN_PLANTKILL");
            achievement46.AddCondition(NPCKilledCondition.Create((short)262));
            achievements_by_id.Add(achievement46, 22544);

            Achievement achievement47 = new Achievement("TEMPLE_RAIDER");
            achievement47.AddCondition((AchievementCondition)ProgressionEventCondition.Create(22));
            achievements_by_id.Add(achievement47, 22545);

            Achievement achievement48 = new Achievement("ROBBING_THE_GRAVE");
            achievement48.AddCondition(ItemPickupCondition.Create((short)1513, (short)938, (short)963, (short)977, (short)1300, (short)1254, (short)1514, (short)679, (short)759, (short)1446, (short)1445, (short)1444, (short)1183, (short)1266, (short)671, (short)3291, (short)4679));
            achievements_by_id.Add(achievement48, 22546);

            Achievement achievement49 = new Achievement("THERE_ARE_SOME_WHO_CALL_HIM");
            achievement49.AddCondition(NPCKilledCondition.Create((short)45));
            achievements_by_id.Add(achievement49, 22547);

            Achievement achievement50 = new Achievement("DECEIVER_OF_FOOLS");
            achievement50.AddCondition(NPCKilledCondition.Create((short)196));
            achievements_by_id.Add(achievement50, 22548);

            Achievement achievement51 = new Achievement("FUNKYTOWN");
            achievement51.AddCondition(CustomFlagCondition.Create("Reach"));
            achievements_by_id.Add(achievement51, 22549);

            Achievement achievement52 = new Achievement("BIG_BOOTY");
            achievement52.AddCondition((AchievementCondition)ProgressionEventCondition.Create(20));
            achievements_by_id.Add(achievement52, 22550);

            Achievement achievement53 = new Achievement("PRETTY_IN_PINK");
            achievement53.AddCondition(NPCKilledCondition.Create((short)-4));
            achievements_by_id.Add(achievement53, 22551);

            Achievement achievement54 = new Achievement("DYE_HARD");
            achievement54.AddCondition(CustomFlagCondition.Create("Equip"));
            achievements_by_id.Add(achievement54, 22552);

            Achievement achievement55 = new Achievement("FREQUENT_FLYER");
            achievement55.AddCondition(CustomFloatCondition.Create("Pay", 10000f));
            achievement55.UseTrackerFromCondition("Pay");
            achievements_by_id.Add(achievement55, 22553);

            Achievement achievement56 = new Achievement("YOU_AND_WHAT_ARMY");
            achievement56.AddCondition(CustomFlagCondition.Create("Spawn"));
            achievements_by_id.Add(achievement56, 22554);

            Achievement achievement57 = new Achievement("PRISMANCER");
            achievement57.AddCondition(ItemPickupCondition.Create((short)495));
            achievements_by_id.Add(achievement57, 22555);

            Achievement achievement58 = new Achievement("MARATHON_MEDALIST");
            achievement58.AddCondition(CustomFloatCondition.Create("Move", 1106688f));
            achievement58.UseTrackerFromCondition("Move");
            achievements_by_id.Add(achievement58, 22556);

            Achievement achievement59 = new Achievement("LIHZAHRDIAN_IDOL");
            achievement59.AddCondition(NPCKilledCondition.Create((short)245));
            achievements_by_id.Add(achievement59, 22557);

            Achievement achievement60 = new Achievement("FISH_OUT_OF_WATER");
            achievement60.AddCondition(NPCKilledCondition.Create((short)370));
            achievements_by_id.Add(achievement60, 22558);

            Achievement achievement61 = new Achievement("SWORD_OF_THE_HERO");
            achievement61.AddCondition(ItemPickupCondition.Create((short)757));
            achievements_by_id.Add(achievement61, 22559);

            Achievement achievement62 = new Achievement("TIL_DEATH");
            achievement62.AddCondition(NPCKilledCondition.Create((short)53));
            achievements_by_id.Add(achievement62, 22560);

            Achievement achievement63 = new Achievement("ARCHAEOLOGIST");
            achievement63.AddCondition(NPCKilledCondition.Create((short)52));
            achievements_by_id.Add(achievement63, 22561);

            Achievement achievement64 = new Achievement("IT_CAN_TALK");
            achievement64.AddCondition((AchievementCondition)ProgressionEventCondition.Create(18));
            achievements_by_id.Add(achievement64, 22562);

            Achievement achievement65 = new Achievement("TOPPED_OFF");
            achievement65.AddCondition(CustomFlagCondition.Create("Use"));
            achievements_by_id.Add(achievement65, 22563);

            Achievement achievement66 = new Achievement("OBSESSIVE_DEVOTION");
            achievement66.AddCondition(NPCKilledCondition.Create((short)439));
            achievements_by_id.Add(achievement66, 22564);

            // Completion Achievement
            Achievement achievement67 = new Achievement("CHAMPION_OF_TERRARIA");
            achievement67.AddCondition(NPCKilledCondition.Create((short)398));
            achievements_by_id.Add(achievement67, 0);
        }
    }
}