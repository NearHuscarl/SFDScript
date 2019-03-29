﻿using SFDGameScriptInterface;
using SFDScript.BotExtended.Bots;
using SFDScript.BotExtended.Group;
using SFDScript.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using static SFDScript.BotExtended.GameScript;
using static SFDScript.Library.Mocks.MockObjects;

namespace SFDScript.BotExtended
{
    public static class Command
    {
        private static IScriptStorage m_storage = Game.LocalStorage;

        public static void OnUserMessage(UserMessageCallbackArgs args)
        {
            try
            {
                if (!args.User.IsHost || !args.IsCommand || (args.Command != "BOTEXTENDED" && args.Command != "BE"))
                {
                    return;
                }

                var message = args.CommandArguments.ToLowerInvariant();
                var words = message.Split(' ');
                var command = words.FirstOrDefault();
                var arguments = words.Skip(1);

                switch (command)
                {
                    case "?":
                    case "h":
                    case "help":
                        PrintHelp();
                        break;

                    case "v":
                    case "version":
                        PrintVersion();
                        break;

                    case "lg":
                    case "listgroup":
                        ListBotGroup();
                        break;

                    case "lb":
                    case "listbot":
                        ListBotType();
                        break;

                    case "/":
                    case "f":
                    case "find":
                        FindGroup(arguments);
                        break;

                    case "s":
                    case "setting":
                        ShowCurrentSettings();
                        break;

                    case "bc":
                    case "botcount":
                        SetBotCount(arguments);
                        break;

                    case "sp":
                    case "spawn":
                        SpawnNewBot(arguments);
                        break;

                    case "r":
                    case "random":
                        SetRandomGroup(arguments);
                        break;

                    case "g":
                    case "group":
                        SelectGroup(arguments);
                        break;

                    case "st":
                    case "stats":
                        PrintStatistics();
                        break;

                    case "cst":
                    case "clearstats":
                        ClearStatistics();
                        break;

                    case "ka":
                        KillAll(); // For debugging purpose only
                        break;
                    case "gm":
                        ToggleGodMode();
                        break;

                    default:
                        ScriptHelper.PrintMessage("Invalid command", ScriptHelper.ERROR_COLOR);
                        break;
                }
            }
            catch (Exception e)
            {
                var stackTrace = e.StackTrace;
                var lines = stackTrace.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var thisNamespace = SharpHelper.GetNamespace<Bot>();

                foreach (var line in lines)
                {
                    if (line.Contains(thisNamespace))
                    {
                        Game.RunCommand("/msg [BotExtended script]: " + line);
                        Game.RunCommand("/msg [BotExtended script]: " + e.Message);
                        break;
                    }
                }
            }
        }

        private static void PrintHelp()
        {
            ScriptHelper.PrintMessage("--Botextended help--", ScriptHelper.ERROR_COLOR);
            ScriptHelper.PrintMessage("/<botextended|be> [help|h|?]: Print this help");
            ScriptHelper.PrintMessage("/<botextended|be> [version|v]: Print the current version");
            ScriptHelper.PrintMessage("/<botextended|be> [listgroup|lg]: List all bot groups");
            ScriptHelper.PrintMessage("/<botextended|be> [listbot|lb]: List all bot types");
            ScriptHelper.PrintMessage("/<botextended|be> [find|f|/] <query>: Find bot groups");
            ScriptHelper.PrintMessage("/<botextended|be> [settings|s]: Show current script settings");
            ScriptHelper.PrintMessage("/<botextended|be> [spawn|sp] <BotType> [Team|_] [Count]: Spawn bot");
            ScriptHelper.PrintMessage("/<botextended|be> [botcount|bc] [1-10]: Set maximum bot count");
            ScriptHelper.PrintMessage("/<botextended|be> [random|r] <0|1>: Random all groups at startup if set to 1. This option will disregard the current group list");
            ScriptHelper.PrintMessage("/<botextended|be> [group|g] <group names|indexes>: Choose a list of group by either name or index to randomly spawn on startup");
            ScriptHelper.PrintMessage("/<botextended|be> [stats|st]: List all bot types and bot groups stats");
            ScriptHelper.PrintMessage("/<botextended|be> [clearstats|cst]: Clear all bot types and bot groups stats");
            ScriptHelper.PrintMessage("For example:", ScriptHelper.ERROR_COLOR);
            ScriptHelper.PrintMessage("/botextended select metrocop >> select metrocop group");
            ScriptHelper.PrintMessage("/be s 0 2 4 >> select assassin, bandido and clown group");
        }

        private static void PrintVersion()
        {
            ScriptHelper.PrintMessage("--Botextended version--", ScriptHelper.ERROR_COLOR);
            ScriptHelper.PrintMessage("v" + BotHelper.CURRENT_VERSION);
        }

        private static IEnumerable<string> GetGroupNames()
        {
            var groups = SharpHelper.GetArrayFromEnum<BotGroup>();

            foreach (var group in groups)
            {
                yield return ((int)group).ToString() + ": " + SharpHelper.EnumToString(group);
            }
        }

        private static void ListBotGroup()
        {
            ScriptHelper.PrintMessage("--Botextended list group--", ScriptHelper.ERROR_COLOR);

            foreach (var groupName in GetGroupNames())
            {
                ScriptHelper.PrintMessage(groupName, ScriptHelper.WARNING_COLOR);
            }
        }

        private static void ListBotType()
        {
            ScriptHelper.PrintMessage("--Botextended list bot type--", ScriptHelper.ERROR_COLOR);

            foreach (var botType in SharpHelper.EnumToList<BotType>())
            {
                ScriptHelper.PrintMessage((int)botType + ": " + SharpHelper.EnumToString(botType), ScriptHelper.WARNING_COLOR);
            }
        }

        private static void FindGroup(IEnumerable<string> arguments)
        {
            var query = arguments.FirstOrDefault();
            if (query == null) return;

            ScriptHelper.PrintMessage("--Botextended find--", ScriptHelper.ERROR_COLOR);

            foreach (var groupName in GetGroupNames())
            {
                var name = groupName.ToLowerInvariant();
                if (name.Contains(query))
                    ScriptHelper.PrintMessage(groupName, ScriptHelper.WARNING_COLOR);
            }
        }

        private static void ShowCurrentSettings()
        {
            ScriptHelper.PrintMessage("--Botextended settings--", ScriptHelper.ERROR_COLOR);

            string[] groups = null;
            if (m_storage.TryGetItemStringArr(BotHelper.BOT_GROUPS, out groups))
            {
                ScriptHelper.PrintMessage("-Current groups", ScriptHelper.WARNING_COLOR);
                for (var i = 0; i < groups.Length; i++)
                {
                    var botGroup = SharpHelper.StringToEnum<BotGroup>(groups[i]);
                    var index = (int)botGroup;
                    ScriptHelper.PrintMessage(index + ": " + groups[i]);
                }
            }

            bool randomGroup;
            if (!m_storage.TryGetItemBool(BotHelper.RANDOM_GROUP, out randomGroup))
            {
                randomGroup = BotHelper.RANDOM_GROUP_DEFAULT_VALUE;
            }
            ScriptHelper.PrintMessage("-Random groups: " + randomGroup, ScriptHelper.WARNING_COLOR);

            int botCount;
            if (!m_storage.TryGetItemInt(BotHelper.BOT_COUNT, out botCount))
            {
                botCount = BotHelper.MAX_BOT_COUNT_DEFAULT_VALUE;
            }
            ScriptHelper.PrintMessage("-Max bot count: " + botCount, ScriptHelper.WARNING_COLOR);
        }

        private static void SpawnNewBot(IEnumerable<string> arguments)
        {
            var query = arguments.FirstOrDefault();
            if (query == null) return;
            var argList = arguments.ToList();

            var team = PlayerTeam.Independent;
            if (arguments.Count() >= 2 && argList[1] != "_")
            {
                if (!Enum.TryParse(argList[1], ignoreCase: true, result: out team))
                    team = PlayerTeam.Independent;
            }

            var count = 1;
            if (arguments.Count() >= 3)
            {
                if (int.TryParse(argList[2], out count))
                    count = (int)MathHelper.Clamp(count, 1, 15);
                else
                    count = 1;
            }

            var botType = BotType.None;

            if (SharpHelper.TryParseEnum(query, out botType))
            {
                for (var i = 0; i < count; i++)
                {
                    BotHelper.SpawnBot(botType, player: null,
                        equipWeapons: true,
                        setProfile: true,
                        team: team,
                        ignoreFullSpawner: true);
                }

                // Dont use the string name in case it just an index
                var bot = count > 1 ? " bots" : " bot";
                ScriptHelper.PrintMessage("Spawned " + count + " " + SharpHelper.EnumToString(botType) + bot + " as " + team);
            }
            else
            {
                ScriptHelper.PrintMessage("--Botextended spawn bot--", ScriptHelper.ERROR_COLOR);
                ScriptHelper.PrintMessage("Invalid query: " + query, ScriptHelper.WARNING_COLOR);
            }
        }

        private static void SetBotCount(IEnumerable<string> arguments)
        {
            var firstArg = arguments.FirstOrDefault();
            if (firstArg == null) return;
            int value = -1;

            if (int.TryParse(firstArg, out value))
            {
                m_storage.SetItem(BotHelper.BOT_COUNT, value);
                ScriptHelper.PrintMessage("[Botextended] Update successfully");
            }
            else
                ScriptHelper.PrintMessage("[Botextended] Invalid query: " + firstArg, ScriptHelper.WARNING_COLOR);
        }

        private static void SetRandomGroup(IEnumerable<string> arguments)
        {
            var firstArg = arguments.FirstOrDefault();
            if (firstArg == null) return;
            int value = -1;

            if (firstArg != "0" && firstArg != "1")
            {
                ScriptHelper.PrintMessage("--Botextended random group--", ScriptHelper.ERROR_COLOR);
                ScriptHelper.PrintMessage("Invalid value: " + value + "Value is either 1 (true) or 0 (false): ", ScriptHelper.WARNING_COLOR);
                return;
            }

            if (int.TryParse(firstArg, out value))
            {
                if (value == 1)
                    m_storage.SetItem(BotHelper.RANDOM_GROUP, true);
                if (value == 0)
                    m_storage.SetItem(BotHelper.RANDOM_GROUP, false);
                ScriptHelper.PrintMessage("[Botextended] Update successfully");
            }
            else
                ScriptHelper.PrintMessage("[Botextended] Invalid query: " + firstArg, ScriptHelper.WARNING_COLOR);
        }

        private static void SelectGroup(IEnumerable<string> arguments)
        {
            var botGroups = new List<string>();
            BotGroup botGroup;

            foreach (var query in arguments)
            {
                if (SharpHelper.TryParseEnum(query, out botGroup))
                {
                    botGroups.Add(SharpHelper.EnumToString(botGroup));
                }
                else
                {
                    ScriptHelper.PrintMessage("--Botextended select--", ScriptHelper.ERROR_COLOR);
                    ScriptHelper.PrintMessage("Invalid query: " + query, ScriptHelper.WARNING_COLOR);
                    return;
                }
            }

            botGroups.Sort();
            m_storage.SetItem(BotHelper.BOT_GROUPS, botGroups.Distinct().ToArray());
            ScriptHelper.PrintMessage("[Botextended] Update successfully");
        }

        private static void PrintStatistics()
        {
            ScriptHelper.PrintMessage("--Botextended statistics--", ScriptHelper.ERROR_COLOR);

            var botTypes = SharpHelper.EnumToList<BotType>();
            ScriptHelper.PrintMessage("-[BotType]: [WinCount] [TotalMatch] [SurvivalRate]", ScriptHelper.WARNING_COLOR);
            foreach (var botType in botTypes)
            {
                var botTypeKeyPrefix = BotHelper.GET_BOTTYPE_STORAGE_KEY(botType);
                int winCount;
                var getWinCountAttempt = m_storage.TryGetItemInt(botTypeKeyPrefix + "_WIN_COUNT", out winCount);
                int totalMatch;
                var getTotalMatchAttempt = m_storage.TryGetItemInt(botTypeKeyPrefix + "_TOTAL_MATCH", out totalMatch);

                if (getWinCountAttempt && getTotalMatchAttempt)
                {
                    var survivalRate = (float)winCount / totalMatch;
                    var survivalRateStr = survivalRate.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

                    ScriptHelper.PrintMessage(SharpHelper.EnumToString(botType) + ": "
                        + " " + winCount + " " + totalMatch + " " + survivalRateStr);
                }
            }

            var botGroups = SharpHelper.EnumToList<BotGroup>();
            ScriptHelper.PrintMessage("-[BotGroup] [Index]: [WinCount] [TotalMatch] [SurvivalRate]", ScriptHelper.WARNING_COLOR);
            foreach (var botGroup in botGroups)
            {
                var groupSet = GetGroupSet(botGroup);
                for (var i = 0; i < groupSet.Groups.Count; i++)
                {
                    var groupKeyPrefix = BotHelper.GET_GROUP_STORAGE_KEY(botGroup, i);
                    int winCount;
                    var getWinCountAttempt = m_storage.TryGetItemInt(groupKeyPrefix + "_WIN_COUNT", out winCount);
                    int totalMatch;
                    var getTotalMatchAttempt = m_storage.TryGetItemInt(groupKeyPrefix + "_TOTAL_MATCH", out totalMatch);

                    if (getWinCountAttempt && getTotalMatchAttempt)
                    {
                        var survivalRate = (float)winCount / totalMatch;
                        var survivalRateStr = survivalRate.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

                        ScriptHelper.PrintMessage(SharpHelper.EnumToString(botGroup) + " " + i + ": "
                            + " " + winCount + " " + totalMatch + " " + survivalRateStr);
                    }
                }
            }
        }

        private static void ClearStatistics()
        {
            ScriptHelper.PrintMessage("--Botextended clear statistics--", ScriptHelper.ERROR_COLOR);
            var botTypes = SharpHelper.EnumToList<BotType>();
            foreach (var botType in botTypes)
            {
                var botTypeKeyPrefix = BotHelper.GET_BOTTYPE_STORAGE_KEY(botType);

                m_storage.RemoveItem(botTypeKeyPrefix + "_WIN_COUNT");
                m_storage.RemoveItem(botTypeKeyPrefix + "_TOTAL_MATCH");
            }

            var botGroups = SharpHelper.EnumToList<BotGroup>();
            foreach (var botGroup in botGroups)
            {
                var groupSet = GetGroupSet(botGroup);
                for (var i = 0; i < groupSet.Groups.Count; i++)
                {
                    var groupKeyPrefix = BotHelper.GET_GROUP_STORAGE_KEY(botGroup, i);
                    m_storage.RemoveItem(groupKeyPrefix + "_WIN_COUNT");
                    m_storage.RemoveItem(groupKeyPrefix + "_TOTAL_MATCH");
                }
            }

            ScriptHelper.PrintMessage("[Botextended] Clear successfully");
        }

        private static void KillAll()
        {
            if (!Game.IsEditorTest) return;
            var players = Game.GetPlayers();
            foreach (var player in players)
            {
                if (player.GetUser() == null || !player.GetUser().IsHost)
                    player.Kill();
            }
        }

        private static bool m_godMode = (Game.IsEditorTest ? true : false);
        private static void ToggleGodMode()
        {
            m_godMode = !m_godMode;
            var modifiers = Game.GetPlayers()[0].GetModifiers();

            if (m_godMode)
            {
                modifiers.MaxHealth = 5000;
                modifiers.CurrentHealth = 5000;
                modifiers.InfiniteAmmo = 1;
                modifiers.MeleeStunImmunity = 1;
                ScriptHelper.PrintMessage("[Botextended] God mode - ON");
            }
            else
            {
                modifiers.MaxHealth = 100;
                modifiers.CurrentHealth = 100;
                modifiers.InfiniteAmmo = 0;
                modifiers.MeleeStunImmunity = 0;
                ScriptHelper.PrintMessage("[Botextended] God mode - OFF");
            }
            Game.GetPlayers()[0].SetModifiers(modifiers);
        }
    }
}