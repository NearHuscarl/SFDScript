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
                // Marauder has fake MaxHealth to have blood effect on the face
                if (Enum.GetName(typeof(BotType), GetExtendedBot(Body).Type).StartsWith("Marauder"))
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

        internal static string StorageKey(string key)
        {
            return Constants.STORAGE_KEY_PREFIX + key;
        }
        internal static string StorageKey(BotType botType)
        {
            return Constants.STORAGE_KEY_PREFIX + SharpHelper.EnumToString(botType).ToUpperInvariant();
        }
        internal static string StorageKey(BotGroup botGroup, int groupIndex)
        {
            return Constants.STORAGE_KEY_PREFIX + SharpHelper.EnumToString(botGroup).ToUpperInvariant() + "_" + groupIndex;
        }

        public static BotGroup CurrentBotGroup { get; private set; }
        public static int CurrentGroupSetIndex { get; private set; }

        private static Events.PlayerDamageCallback m_playerDamageEvent = null;
        private static Events.PlayerDeathCallback m_playerDeathEvent = null;
        private static Events.UpdateCallback m_updateEvent = null;
        private static Events.UserMessageCallback m_userMessageEvent = null;

        // Player corpses waiting to be transformed into zombies
        private static List<PlayerCorpse> m_infectedCorpses = new List<PlayerCorpse>();
        private static List<PlayerSpawner> m_playerSpawners;
        private static Dictionary<string, Bot> m_bots = new Dictionary<string, Bot>();

        public static void Initialize()
        {
            m_playerSpawners = GetEmptyPlayerSpawners();
            m_playerDamageEvent = Events.PlayerDamageCallback.Start(OnPlayerDamage);
            m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);
            m_updateEvent = Events.UpdateCallback.Start(OnUpdate);
            m_userMessageEvent = Events.UserMessageCallback.Start(Command.OnUserMessage);

            InitRandomSeed();

            bool randomGroup;
            if (!Storage.Get(StorageKey("RANDOM_GROUP"), out randomGroup))
            {
                randomGroup = Constants.RANDOM_GROUP_DEFAULT_VALUE;
            }

            int botCount;
            if (!Storage.Get(StorageKey("BOT_COUNT"), out botCount))
            {
                botCount = Constants.MAX_BOT_COUNT_DEFAULT_VALUE;
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
                if (!Storage.Get(StorageKey("BOT_GROUPS"), out selectedGroups))
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
                //SpawnRandomGroup(botSpawnCount, botGroups);
                //IPlayer player = null;
                SpawnGroup(BotGroup.Boss_Ninja, botSpawnCount, 1);
                //SpawnBot(BotType.Bandido);
            }
        }

        private static void InitRandomSeed()
        {
            int[] botGroupSeed;
            int inext;
            int inextp;

            var getBotGroupSeedAttempt = Storage.Get(StorageKey("BOT_GROUP_SEED"), out botGroupSeed);
            var getBotGroupInextAttempt = Storage.Get(StorageKey("BOT_GROUP_INEXT"), out inext);
            var getBotGroupInextpAttempt = Storage.Get(StorageKey("BOT_GROUP_INEXTP"), out inextp);

            if (getBotGroupSeedAttempt && getBotGroupInextAttempt && getBotGroupInextpAttempt)
            {
                RandomHelper.AddRandomGenerator("BOT_GROUP_SEED", new Rnd(botGroupSeed, inext, inextp));
            }
            else
            {
                RandomHelper.AddRandomGenerator("BOT_GROUP_SEED", new Rnd());
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

            var rndBotGroup = RandomHelper.GetItem(filteredBotGroups, "BOT_GROUP_SEED");
            var groupSet = GetGroupSet(rndBotGroup);
            var rndGroupIndex = RandomHelper.Rnd.Next(groupSet.Groups.Count);
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

            if (!CanInfectFrom(player))
            {
                IPlayer theInfected;

                if (IsHitByZombieOrTheInfected(player, out theInfected))
                {
                    var extendedBot = GetExtendedBot(player);

                    // Normal players that are not extended bots
                    if (extendedBot == Bot.None)
                    {
                        extendedBot = Wrap(player);
                    }

                    // TODO: Fix bot count double when transform to zombie from infected
                    //Game.ShowChatMessage(m_bots.Values.Count.ToString());

                    if (!extendedBot.Info.ImmuneToInfect)
                    {
                        Game.PlayEffect(Effect.CUSTOM_FLOAT_TEXT, player.GetWorldPosition(), "infected");
                        Game.ShowChatMessage(theInfected.Name + " infected player " + player.Name);
                        extendedBot.Info.ZombieStatus = ZombieStatus.Infected;
                    }
                }
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

            var bot = GetExtendedBot(player);

            if (bot != Bot.None && bot.Info.ZombieStatus == ZombieStatus.Infected)
            {
                m_infectedCorpses.Add(new PlayerCorpse(player));
            }
        }

        public static Bot GetExtendedBot(IPlayer player)
        {
            return m_bots.ContainsKey(player.CustomID) ? m_bots[player.CustomID] : Bot.None;
        }

        private static BotType GetZombieType(IPlayer player)
        {
            if (player == null)
            {
                throw new Exception("Player cannot be null");
            }
            var botType = GetExtendedBot(player).Type;

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

        private static bool IsHitByZombieOrTheInfected(IPlayer target, out IPlayer theInfected)
        {
            theInfected = null;

            foreach (var player in Game.GetPlayers())
            {
                if (player.UniqueID == target.UniqueID)
                    continue;

                if (CanInfectFrom(player))
                {
                    // The infected can only infect the non-infected via punching
                    if (player.CurrentWeaponDrawn != WeaponItemType.NONE)
                        continue;

                    if (ScriptHelper.IsHiting(player, target))
                    {
                        theInfected = player;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CanInfectFrom(IPlayer player)
        {
            var extendedBot = GetExtendedBot(player);

            return extendedBot != Bot.None
                    && extendedBot.Info.ZombieStatus != ZombieStatus.Human;
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

            var rndSpawner = RandomHelper.GetItem(emptySpawners);
            var player = Game.CreatePlayer(rndSpawner.Position);

            rndSpawner.HasSpawned = true;

            return player;
        }

        private static Bot Wrap(IPlayer player)
        {
            var bot = new Bot(player);

            if (string.IsNullOrEmpty(player.CustomID))
            {
                player.CustomID = Guid.NewGuid().ToString("N");
            }

            m_bots.Add(player.CustomID, bot);
            return bot;
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
            // player.UniqueID is unique but seems like it can change value during
            // the script lifetime. Use custom id + guid() to get the const unique id
            if (string.IsNullOrEmpty(player.CustomID))
            {
                player.CustomID = Guid.NewGuid().ToString("N");
            }

            if (equipWeapons)
            {
                if (RandomHelper.Between(0f, 1f) < info.EquipWeaponChance)
                {
                    weaponSet = RandomHelper.GetItem(GetWeapons(botType));
                }
                weaponSet.Equip(player);
            }

            if (setProfile)
            {
                var profile = RandomHelper.GetItem(GetProfiles(botType));
                player.SetProfile(profile);
                player.SetBotName(profile.Name);
            }

            player.SetModifiers(info.Modifiers);
            player.SetBotBehaviorSet(GetBehaviorSet(info.AIType, info.SearchItems));
            player.SetBotBehaviorActive(true);
            player.SetTeam(team);

            var bot = BotFactory.Create(player, botType, info);
            m_bots.Add(player.CustomID, bot);

            return bot;
        }

        public static void StoreStatistics()
        {
            var players = Game.GetPlayers();
            var groupDead = true;
            var updatedBotTypes = new List<BotType>();

            foreach (var player in players)
            {
                var botType = GetExtendedBot(player).Type;
                if (botType == BotType.None || updatedBotTypes.Contains(botType)) continue;
                var botTypeKeyPrefix = StorageKey(botType);

                var botWinCountKey = botTypeKeyPrefix + "_WIN_COUNT";
                int botOldWinCount;
                var getBotWinCountAttempt = Storage.Get(botWinCountKey, out botOldWinCount);

                var botTotalMatchKey = botTypeKeyPrefix + "_TOTAL_MATCH";
                int botOldTotalMatch;
                var getBotTotalMatchAttempt = Storage.Get(botTotalMatchKey, out botOldTotalMatch);

                if (getBotWinCountAttempt && getBotTotalMatchAttempt)
                {
                    if (!player.IsDead)
                        Storage.Set(botWinCountKey, botOldWinCount + 1);
                    Storage.Set(botTotalMatchKey, botOldTotalMatch + 1);
                }
                else
                {
                    if (!player.IsDead)
                        Storage.Set(botWinCountKey, 1);
                    else
                        Storage.Set(botWinCountKey, 0);
                    Storage.Set(botTotalMatchKey, 1);
                }

                updatedBotTypes.Add(botType);
                if (!player.IsDead) groupDead = false;
            }

            var currentBotGroup = CurrentBotGroup;
            var currentGroupSetIndex = CurrentGroupSetIndex;
            var botGroupKeyPrefix = StorageKey(CurrentBotGroup, CurrentGroupSetIndex);

            var groupWinCountKey = botGroupKeyPrefix + "_WIN_COUNT";
            int groupOldWinCount;
            var getGroupWinCountAttempt = Storage.Get(groupWinCountKey, out groupOldWinCount);

            var groupTotalMatchKey = botGroupKeyPrefix + "_TOTAL_MATCH";
            int groupOldTotalMatch;
            var getGroupTotalMatchAttempt = Storage.Get(groupTotalMatchKey, out groupOldTotalMatch);

            if (getGroupWinCountAttempt && getGroupTotalMatchAttempt)
            {
                if (!groupDead)
                    Storage.Set(groupWinCountKey, groupOldWinCount + 1);
                Storage.Set(groupTotalMatchKey, groupOldTotalMatch + 1);
            }
            else
            {
                if (!groupDead)
                    Storage.Set(groupWinCountKey, 1);
                else
                    Storage.Set(groupWinCountKey, 0);
                Storage.Set(groupTotalMatchKey, 1);
            }

            StoreRandomSeed();
        }

        private static void StoreRandomSeed()
        {
            var botGroupSeed = RandomHelper.GetRandomGenerator("BOT_GROUP_SEED").SeedArray;
            var inext = RandomHelper.GetRandomGenerator("BOT_GROUP_SEED").inext;
            var inextp = RandomHelper.GetRandomGenerator("BOT_GROUP_SEED").inextp;

            Storage.Set(StorageKey("BOT_GROUP_SEED"), botGroupSeed);
            Storage.Set(StorageKey("BOT_GROUP_INEXT"), inext);
            Storage.Set(StorageKey("BOT_GROUP_INEXTP"), inextp);
        }
    }
}
