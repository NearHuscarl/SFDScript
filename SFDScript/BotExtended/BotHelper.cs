using SFDGameScriptInterface;
using SFDScript.BotExtended.Bots;
using SFDScript.BotExtended.Group;
using SFDScript.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using static SFDScript.Library.Mocks.MockObjects;
using static SFDScript.BotExtended.GameScript;

namespace SFDScript.BotExtended
{
    public static class BotHelper
    {
        private class PlayerCorpse
        {
            public IPlayer Body { get; set; }
            public float DeathTime { get; set; }
            public bool IsTurningIntoZombie { get; private set; }
            public bool TurnIntoZombie()
            {
                if (Body.IsRemoved || Body.IsBurnedCorpse) return false;

                var player = Game.CreatePlayer(Body.GetWorldPosition());
                var zombie = SpawnBot(GetZombieType(Body), player, equipWeapons: false, setProfile: false);
                var zombieBody = zombie.Player;

                var modifiers = Body.GetModifiers();
                if (IsTheInfected(GetBotType(Body))) // Marauder has fake MaxHealth to have blood effect on the face
                    modifiers.CurrentHealth = modifiers.MaxHealth = 75;
                else
                    modifiers.CurrentHealth = modifiers.MaxHealth * 0.75f;
                zombieBody.SetModifiers(modifiers);

                var profile = Body.GetProfile();
                zombieBody.SetProfile(ToZombieProfile(profile));
                zombieBody.SetBotName(Body.Name);

                Body.Remove();
                Body = zombieBody;
                Body.SetBotBehaivorActive(false);
                Body.AddCommand(new PlayerCommand(PlayerCommandType.StartCrouch));
                IsTurningIntoZombie = true;
                return true;
            }
            public bool IsZombie { get; private set; }

            public PlayerCorpse(IPlayer player)
            {
                Body = player;
                IsTurningIntoZombie = false;
                IsZombie = false;
                DeathTime = Game.TotalElapsedGameTime;
            }

            private bool isKneeling;
            private float kneelingTime;
            public void UpdateTurningIntoZombieAnimation()
            {
                if (!isKneeling)
                {
                    kneelingTime = Game.TotalElapsedGameTime;
                    isKneeling = true;
                }
                else
                {
                    if (ScriptHelper.IsElapsed(kneelingTime, 700))
                    {
                        Body.AddCommand(new PlayerCommand(PlayerCommandType.StopCrouch));
                        Body.SetBotBehaivorActive(true);
                        IsZombie = true;
                    }
                }
            }
        }

        public static readonly string CURRENT_VERSION = "0.0";
        public static readonly string BOT_GROUPS = "BOT_EXTENDED_NH_BOT_GROUPS";
        public static readonly string RANDOM_GROUP = "BOT_EXTENDED_NH_RANDOM_GROUP";
        public static readonly string BOT_COUNT = "BOT_EXTENDED_NH_BOT_COUNT";
        public static readonly string VERSION = "BOT_EXTENDED_NH_VERSION";
        public static Func<BotType, string> GET_BOTTYPE_STORAGE_KEY = (botType) => "BOT_EXTENDED_NH_"
            + SharpHelper.EnumToString(botType).ToUpperInvariant();
        public static Func<BotGroup, int, string> GET_GROUP_STORAGE_KEY = (botGroup, groupIndex) => "BOT_EXTENDED_NH_"
            + SharpHelper.EnumToString(botGroup).ToUpperInvariant()
            + "_" + groupIndex;

        public static readonly bool RANDOM_GROUP_DEFAULT_VALUE = true;
        public static readonly int MAX_BOT_COUNT_DEFAULT_VALUE = 5;
        public static BotGroup CurrentBotGroup { get; private set; }
        public static int CurrentGroupSetIndex { get; private set; }

        private static Events.PlayerDamageCallback m_playerDamageEvent = null;
        private static Events.PlayerDeathCallback m_playerDeathEvent = null;
        private static Events.UpdateCallback m_updateEvent = null;
        private static Events.UserMessageCallback m_userMessageEvent = null;

        // Player corpses waiting to be transformed into zombies
        private static List<PlayerCorpse> m_infectedCorpses = new List<PlayerCorpse>();
        private static Dictionary<string, IPlayer> m_infectedPlayers = new Dictionary<string, IPlayer>();
        private static List<PlayerSpawner> m_playerSpawners;
        private static Dictionary<string, Bot> m_bots = new Dictionary<string, Bot>();

        public static void Initialize()
        {
            m_playerSpawners = GetEmptyPlayerSpawners();
            m_playerDamageEvent = Events.PlayerDamageCallback.Start(OnPlayerDamage);
            m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);
            m_updateEvent = Events.UpdateCallback.Start(OnUpdate);
            m_userMessageEvent = Events.UserMessageCallback.Start(Command.OnUserMessage);

            bool randomGroup;
            if (!Game.LocalStorage.TryGetItemBool(RANDOM_GROUP, out randomGroup))
            {
                randomGroup = RANDOM_GROUP_DEFAULT_VALUE;
            }

            int botCount;
            if (!Game.LocalStorage.TryGetItemInt(BOT_COUNT, out botCount))
            {
                botCount = MAX_BOT_COUNT_DEFAULT_VALUE;
            }

            botCount = (int)MathHelper.Clamp(botCount, 1, 10);
            var botSpawnCount = Math.Min(botCount, m_playerSpawners.Count);
            var botGroups = new List<BotGroup>();

            if (randomGroup) // Random all bot groups
            {
                botGroups = SharpHelper.GetArrayFromEnum<BotGroup>().ToList();
            }
            else // Random selected bot groups from user settings
            {
                string[] selectedGroups = null;
                if (!Game.LocalStorage.TryGetItemStringArr(BOT_GROUPS, out selectedGroups))
                {
                    ScriptHelper.PrintMessage(
                        "Error when retrieving bot groups to spawn. Default to randomize all available bot groups",
                        ScriptHelper.ERROR_COLOR);
                    botGroups = SharpHelper.GetArrayFromEnum<BotGroup>().ToList();
                }
                else
                {
                    foreach (var groupName in selectedGroups)
                        botGroups.Add(SharpHelper.StringToEnum<BotGroup>(groupName));
                }
            }

            if (!Game.IsEditorTest)
            {
                SpawnRandomGroup(botSpawnCount, botGroups);
            }
            else
            {
                //IPlayer player = null;
                SpawnGroup(BotGroup.Boss_Teddybear, botSpawnCount, 1);
                //SpawnBot(BotType.Bandido);
            }
        }

        private static void SpawnRandomGroup(int botCount, List<BotGroup> botGroups)
        {
            List<BotGroup> filteredBotGroups = null;
            if (botCount < 3) // Too few for a group, spawn boss instead
            {
                filteredBotGroups = botGroups.Select(g => g).Where(g => (int)g >= Constants.BOSS_GROUP_START_INDEX).ToList();
                if (!filteredBotGroups.Any())
                    filteredBotGroups = botGroups;
            }
            else
                filteredBotGroups = botGroups;

            var rndBotGroup = SharpHelper.GetRandomItem(filteredBotGroups);
            var groupSet = GetGroupSet(rndBotGroup);
            var rndGroupIndex = SharpHelper.Rnd.Next(groupSet.Groups.Count);
            var group = groupSet.Groups[rndGroupIndex];

            group.Spawn(botCount);
            CurrentBotGroup = rndBotGroup;
            CurrentGroupSetIndex = rndGroupIndex;
        }

        // Spawn exact group for debugging purpose. Usually you random the group before every match
        private static void SpawnGroup(BotGroup botGroup, int botCount, int groupIndex = -1)
        {
            SpawnRandomGroup(botCount, new List<BotGroup>() { botGroup });
        }

        public static void OnUpdate(float elapsed)
        {
            UpdateCorpses();

            foreach (var bot in m_bots.Values)
                bot.Update(elapsed);

            if (m_updateOnPlayerDeadNextFrame)
                OnPlayerDeathNextFrame();
        }

        // Turning corpses killed by zombie into another one after some time
        private static void UpdateCorpses()
        {
            // loop the copied list since the original one can be modified when OnPlayerDeath() is called
            foreach (var corpse in m_infectedCorpses.ToList())
            {
                if (ScriptHelper.IsElapsed(corpse.DeathTime, 5000))
                {
                    if (!corpse.IsTurningIntoZombie)
                    {
                        if (!corpse.TurnIntoZombie())
                            m_infectedCorpses.Remove(corpse);
                    }
                    else
                    {
                        if (!corpse.IsZombie)
                            corpse.UpdateTurningIntoZombieAnimation();
                        else
                            m_infectedCorpses.Remove(corpse);
                    }
                }
            }
        }

        private static void OnPlayerDamage(IPlayer player, float damage)
        {
            if (player == null) return;

            switch (player.GetTeam())
            {
                case PlayerTeam.Team4:
                    Bot enemy;
                    if (m_bots.TryGetValue(player.CustomID, out enemy))
                    {
                        enemy.OnDamage();
                    }
                    break;
            }

            if (m_infectedPlayers.ContainsKey(player.CustomID)) return;

            if (IsHitByZombieOrTheInfected(player) && !GetInfo(GetBotType(player)).ImmuneToInfect)
            {
                // Normal players that are not extended bots dont have CustomID by default
                if (string.IsNullOrEmpty(player.CustomID))
                    player.CustomID = Guid.NewGuid().ToString("N");
                m_infectedPlayers.Add(player.CustomID, player);
            }
        }

        private static bool m_updateOnPlayerDeadNextFrame = false;
        private static Bot m_deadBot = null;
        private static void OnPlayerDeathNextFrame()
        {
            // Have to wait until next frame after OnPlayerDeath event to confirm if IPlayer.IsRemoved
            // is true (player instance is removed from the map by either gibbed or exploded into pieces)
            if (!m_deadBot.Player.IsRemoved)
            {
                m_deadBot.SayDeathLine();
            }

            m_updateOnPlayerDeadNextFrame = false;
            m_deadBot = null;
        }

        private static void OnPlayerDeath(IPlayer player)
        {
            if (player == null) return;

            switch (player.GetTeam())
            {
                case PlayerTeam.Team4:
                    Bot enemy;
                    if (m_bots.TryGetValue(player.CustomID, out enemy))
                    {
                        m_deadBot = enemy;
                        m_updateOnPlayerDeadNextFrame = true;
                        enemy.OnDeath();
                    }
                    break;
            }

            if (m_infectedPlayers.ContainsKey(player.CustomID))
            {
                var infectedPlayer = m_infectedPlayers[player.CustomID];
                m_infectedCorpses.Add(new PlayerCorpse(infectedPlayer));
                m_infectedPlayers.Remove(player.CustomID);
            }
        }

        public static BotType GetBotType(IPlayer player)
        {
            return m_bots.ContainsKey(player.CustomID) ? m_bots[player.CustomID].Type : BotType.None;
        }

        private static BotType GetZombieType(IPlayer player)
        {
            if (player == null)
            {
                throw new Exception("Player cannot be null");
            }
            var botType = GetBotType(player);

            if (botType == BotType.None)
            {
                var playerAI = player.GetBotBehavior().PredefinedAI;

                switch (playerAI)
                {
                    // Expert, Hard
                    case PredefinedAIType.BotA:
                    case PredefinedAIType.BotB:
                        return BotType.ZombieFighter;

                    default: // Player is user or something else
                        return BotType.Zombie;
                }
            }
            else
            {
                var botInfo = GetInfo(botType);
                var aiType = botInfo.AIType;

                switch (aiType)
                {
                    case BotAI.Hacker:
                    case BotAI.Expert:
                    case BotAI.Hard:
                    case BotAI.MeleeHard:
                    case BotAI.MeleeExpert:
                        return BotType.ZombieFighter;

                    case BotAI.Ninja:
                        return BotType.ZombieChild;

                    case BotAI.Hulk:
                        return BotType.ZombieBruiser;
                }

                var modifiers = botInfo.Modifiers;

                if (modifiers.SprintSpeedModifier >= 1.1f)
                    return BotType.ZombieChild;

                if (modifiers.SizeModifier == 1.25f)
                    return BotType.ZombieFat;

                return BotType.Zombie;
            }
        }

        private static IProfile ToZombieProfile(IProfile profile)
        {
            switch (profile.Skin.Name)
            {
                case "Normal":
                case "Tattoos":
                    profile.Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", "");
                    break;

                case "Normal_fem":
                case "Tattoos_fem":
                    profile.Skin = new IProfileClothingItem("Zombie_fem", "Skin1", "ClothingLightGray", "");
                    break;

                case "BearSkin":
                    profile.Skin = new IProfileClothingItem("FrankenbearSkin", "ClothingDarkGray", "ClothingLightBlue", "");
                    break;
            }

            return profile;
        }

        private static bool IsHitByZombieOrTheInfected(IPlayer target)
        {
            foreach (var bot in m_bots.Values)
            {
                if (!IsZombie(bot.Type) && !IsTheInfected(bot.Type))
                    continue;
                if (IsTheInfected(bot.Type) && bot.Player.CurrentMeleeWeapon.WeaponItem != WeaponItem.NONE)
                    continue;

                var zombie = bot.Player;

                if (ScriptHelper.IsHiting(zombie, target))
                    return true;
            }
            return false;
        }

        private static bool IsZombie(BotType type)
        {
            var typeStr = Enum.GetName(typeof(BotType), type);

            return typeStr.StartsWith("Zombie")
                || typeStr == "BaronVonHauptstein";
        }

        private static bool IsTheInfected(BotType type)
        {
            var typeStr = Enum.GetName(typeof(BotType), type);

            return typeStr.StartsWith("Marauder");
        }

        private static List<PlayerSpawner> GetEmptyPlayerSpawners()
        {
            var spawnPlayers = Game.GetObjectsByName("SpawnPlayer");
            var emptyPlayerSpawners = new List<PlayerSpawner>();

            foreach (var spawnPlayer in spawnPlayers)
            {
                if (!ScriptHelper.SpawnPlayerHasPlayer(spawnPlayer))
                {
                    emptyPlayerSpawners.Add(new PlayerSpawner
                    {
                        Position = spawnPlayer.GetWorldPosition(),
                        HasSpawned = false,
                    });
                }
            }

            return emptyPlayerSpawners;
        }

        private static IPlayer SpawnPlayer(bool ignoreFullSpawner = false)
        {
            List<PlayerSpawner> emptySpawners = null;

            if (ignoreFullSpawner)
            {
                emptySpawners = m_playerSpawners;
            }
            else
            {
                emptySpawners = m_playerSpawners
                    .Select(Q => Q)
                    .Where(Q => Q.HasSpawned == false)
                    .ToList();
            }

            if (!emptySpawners.Any())
            {
                return null;
            }

            var rndSpawner = SharpHelper.GetRandomItem(emptySpawners);
            var player = Game.CreatePlayer(rndSpawner.Position);

            // player.UniqueID is unique but seems like it can change value during
            // the script lifetime. Use custom id + guid() to get the const unique id
            player.CustomID = Guid.NewGuid().ToString("N");
            rndSpawner.HasSpawned = true;

            return player;
        }

        public static Bot SpawnBot(
            BotType botType,
            IPlayer player = null,
            bool equipWeapons = true,
            bool setProfile = true,
            PlayerTeam team = PlayerTeam.Team4,
            bool ignoreFullSpawner = false)
        {
            var info = GetInfo(botType);
            var weaponSet = WeaponSet.Empty;

            if (player == null) player = SpawnPlayer(ignoreFullSpawner);
            if (player == null) return null;
            if (string.IsNullOrEmpty(player.CustomID))
                player.CustomID = Guid.NewGuid().ToString("N");

            if (equipWeapons)
            {
                if (SharpHelper.RandomBetween(0f, 1f) < info.EquipWeaponChance)
                {
                    weaponSet = SharpHelper.GetRandomItem(GetWeapons(botType));
                }
                if (!weaponSet.IsEmpty) weaponSet.Equip(player);
            }

            if (setProfile)
            {
                var profile = SharpHelper.GetRandomItem(GetProfiles(botType));
                player.SetProfile(profile);
                player.SetBotName(profile.Name);
            }

            player.SetModifiers(info.Modifiers);
            player.SetBotBehaviorSet(GetBehaviorSet(info.AIType, info.SearchItems));
            player.SetBotBehaviorActive(true);
            player.SetTeam(team);

            if (info.StartInfected)
                m_infectedPlayers.Add(player.CustomID, player);

            var bot = BotFactory.Create(player, botType, info);
            m_bots.Add(player.CustomID, bot);

            return bot;
        }
    }
}
