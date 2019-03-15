﻿using System;
using System.Linq;
using System.Collections.Generic;
using SFDGameScriptInterface;
using System.Text;

namespace SFDScript.BotExtended
{
    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        private const int MAX_PLAYERS = 12;

        public void OnStartup()
        {
            //System.Diagnostics.Debugger.Break();

            if (Game.IsEditorTest)
            {
                var player = Game.GetPlayers()[0];
                var modifiers = player.GetModifiers();

                //modifiers.MaxHealth = 5000;
                //modifiers.CurrentHealth = 5000;
                //modifiers.InfiniteAmmo = 1;
                //modifiers.MeleeStunImmunity = 1;

                player.SetModifiers(modifiers);
                player.GiveWeaponItem(WeaponItem.KATANA);
                player.GiveWeaponItem(WeaponItem.REVOLVER);
                player.GiveWeaponItem(WeaponItem.FLAMETHROWER);
                player.GiveWeaponItem(WeaponItem.MOLOTOVS);
            }

            try
            {
                BotHelper.Initialize();
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

        public void OnShutdown()
        {
            StoreStatistics();
        }

        private void StoreStatistics()
        {
            var storage = Game.LocalStorage;
            var players = Game.GetPlayers();
            var groupDead = true;
            var updatedBotTypes = new List<BotType>();

            foreach (var player in players)
            {
                var botType = BotHelper.GetBotType(player);
                if (botType == BotType.None || updatedBotTypes.Contains(botType)) continue;
                var botTypeKeyPrefix = BotHelper.GET_BOTTYPE_STORAGE_KEY(botType);

                var botWinCountKey = botTypeKeyPrefix + "_WIN_COUNT";
                int botOldWinCount;
                var getBotWinCountAttempt = storage.TryGetItemInt(botWinCountKey, out botOldWinCount);

                var botTotalMatchKey = botTypeKeyPrefix + "_TOTAL_MATCH";
                int botOldTotalMatch;
                var getBotTotalMatchAttempt = storage.TryGetItemInt(botTotalMatchKey, out botOldTotalMatch);

                if (getBotWinCountAttempt && getBotTotalMatchAttempt)
                {
                    if (!player.IsDead)
                        storage.SetItem(botWinCountKey, botOldWinCount + 1);
                    storage.SetItem(botTotalMatchKey, botOldTotalMatch + 1);
                }
                else
                {
                    if (!player.IsDead)
                        storage.SetItem(botWinCountKey, 1);
                    else
                        storage.SetItem(botWinCountKey, 0);
                    storage.SetItem(botTotalMatchKey, 1);
                }

                updatedBotTypes.Add(botType);
                if (!player.IsDead) groupDead = false;
            }

            var currentBotGroup = BotHelper.CurrentBotGroup;
            var currentGroupSetIndex = BotHelper.CurrentGroupSetIndex;
            var botGroupKeyPrefix = BotHelper.GET_GROUP_STORAGE_KEY(BotHelper.CurrentBotGroup, BotHelper.CurrentGroupSetIndex);

            var groupWinCountKey = botGroupKeyPrefix + "_WIN_COUNT";
            int groupOldWinCount;
            var getGroupWinCountAttempt = storage.TryGetItemInt(groupWinCountKey, out groupOldWinCount);

            var groupTotalMatchKey = botGroupKeyPrefix + "_TOTAL_MATCH";
            int groupOldTotalMatch;
            var getGroupTotalMatchAttempt = storage.TryGetItemInt(groupTotalMatchKey, out groupOldTotalMatch);

            if (getGroupWinCountAttempt && getGroupTotalMatchAttempt)
            {
                if (!groupDead)
                    storage.SetItem(groupWinCountKey, groupOldWinCount + 1);
                storage.SetItem(groupTotalMatchKey, groupOldTotalMatch + 1);
            }
            else
            {
                if (!groupDead)
                    storage.SetItem(groupWinCountKey, 1);
                else
                    storage.SetItem(groupWinCountKey, 0);
                storage.SetItem(groupTotalMatchKey, 1);
            }
        }

        #region Helper class

        public static class SharpHelper
        {
            public static Random Rnd = new Random();

            public static T StringToEnum<T>(string str)
            {
                return (T)Enum.Parse(typeof(T), str);
            }
            public static T[] GetArrayFromEnum<T>()
            {
                return (T[])Enum.GetValues(typeof(T));
            }

            public static IEnumerable<T> EnumToList<T>()
            {
                var enumArray = GetArrayFromEnum<T>();

                foreach (var enumVal in enumArray)
                {
                    yield return enumVal;
                }
            }
            public static string EnumToString<T>(T enumVal)
            {
                return Enum.GetName(typeof(T), enumVal);
            }
            public static bool TryParseEnum<T>(string str, out T result) where T : struct, IConvertible
            {
                result = default(T);

                if (!typeof(T).IsEnum)
                {
                    return false;
                }

                int index = -1;
                if (int.TryParse(str, out index))
                {
                    if (Enum.IsDefined(typeof(T), index))
                    {
                        // https://stackoverflow.com/questions/10387095/cast-int-to-generic-enum-in-c-sharp
                        result = (T)(object)index;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Enum.TryParse(str, ignoreCase: true, result: out result))
                    {
                        return false;
                    }
                }

                return true;
            }

            public static float RandomBetween(float min, float max)
            {
                return (float)Rnd.NextDouble() * (max - min) + min;
            }

            public static T GetRandomItem<T>(List<T> list)
            {
                var rndIndex = Rnd.Next(list.Count);
                return list[rndIndex];
            }

            public static T GetRandomEnumValue<T>()
            {
                var enumValues = Enum.GetValues(typeof(T));
                return (T)enumValues.GetValue(Rnd.Next(enumValues.Length));
            }

            public static string GetNamespace<T>()
            {
                return typeof(T).Namespace;
            }
        }

        public static class ScriptHelper
        {
            public static readonly Color MESSAGE_COLOR = new Color(24, 238, 200);
            public static readonly Color ERROR_COLOR = new Color(244, 77, 77);
            public static readonly Color WARNING_COLOR = new Color(249, 191, 11);

            public static readonly string EFFECT_ACS = "ACS";
            public static readonly string EFFECT_CAM_S = "CAM_S";
            public static readonly string EFFECT_SMOKE = "CFTXT";
            public static readonly string EFFECT_ELECTRIC = "Electric";
            public static readonly string EFFECT_TR_D = "TR_D";

            public static void PrintMessage(string message, Color? color = null)
            {
                if (color == null) color = MESSAGE_COLOR;
                Game.ShowChatMessage(message, (Color)color);
            }

            public static bool IsElapsed(float timeStarted, float timeToElapse)
            {
                if (Game.TotalElapsedGameTime - timeStarted >= timeToElapse)
                    return true;
                else
                    return false;
            }

            public static bool SpawnPlayerHasPlayer(IObject spawnPlayer)
            {
                // Player position y: -20 || +9
                // => -21 -> +10
                // Player position x: unchange
                foreach (var player in Game.GetPlayers())
                {
                    var playerPosition = player.GetWorldPosition();
                    var spawnPlayerPosition = spawnPlayer.GetWorldPosition();

                    if (spawnPlayerPosition.Y - 21 <= playerPosition.Y && playerPosition.Y <= spawnPlayerPosition.Y + 10
                        && spawnPlayerPosition.X == playerPosition.X)
                        return true;
                }

                return false;
            }
        }

        #endregion

        #region Bot type

        public enum BotType
        {
            None,

            // Tier1: Rooftop Retribution
            // Tier2: Canals Carnage
            AssassinMelee,
            AssassinRange,
            // Tier1: Subway Shakedown
            Agent, // Smart agent, weak weapon
            // Tier2: Piston Posse, Tower Trouble
            Agent2, // Dumb agent, strong weapon
            // Tier1: High Moon Holdout
            Bandido,
            // Tier1: Police Station Punchout, Warehouse Wreckage
            // Tier2: Bar Brawl
            // Tier3: Meatgrinder Begins
            Biker,
            BikerHulk,
            // Tier1: The Teahouse Job, Rooftop Retribution
            Bodyguard,
            Bodyguard2, // heavy bodyguard

            ClownBodyguard,
            ClownBoxer,
            ClownCowboy,
            ClownGangster,

            // Tier2: Steamship Standoff
            Cowboy,
            // Tier1: The Teahouse Job
            // Tier2: Alley Bombardment, Rocket Rider
            // Tier3: Rocket Rider 2
            Demolitionist,
            // Tier3: Holiday Hullabaloo
            Elf,
            Hacker,
            Jo,
            Fritzliebe,
            Funnyman,
            // Tier1: Heavy Hostility
            // Tier2: Trainyard Takedown, Alley Bombardment
            // Tier3: Meatgrinder Begins
            Gangster,
            // Tier1: The Teahouse Job, Heavy Hostility
            GangsterHulk,
            Incinerator,
            // Tier1: The Teahouse Job, Rooftop Retribution
            // Tier3: Unearthed
            Kingpin,
            Kriegbär,

            // Infected marauders, turn into zombie when dying
            MarauderBiker,
            MarauderCrazy,
            MarauderNaked,
            MarauderRifleman,
            MarauderRobber,
            MarauderTough,

            // Tier3: Meatgrinder Begins
            Meatgrinder,
            Mecha,
            // Tier2: Hazardous Hustle, Piston Posse
            // Tier3: Armored Unit
            MetroCop,
            MetroCop2,
            // Tier2: Plant 47 Panic
            Mutant,

            NaziLabAssistant,
            NaziMuscleSoldier,
            NaziScientist,
            NaziSoldier,
            SSOfficer,

            Ninja,

            // Tier1: Mall Mayhem
            // Tier3: Police Station Escape!
            Police,
            PoliceSWAT,

            // Tier3: Holiday Hullabaloo
            Santa,
            // Tier3: Facility Ambush
            Sniper,
            // Tier2: Facility Foray
            Soldier,
            Soldier2,
            Teddybear,

            // Tier1: Storage Showdown, Rooftops Rumble, Police Station Punchout, Alley Ambush, Warehouse Wreckage, Heavy Hostility
            Thug,
            ThugHulk,

            // Tier3: Hotel Cleanup
            Zombie,
            ZombieAgent,
            ZombieBruiser,
            ZombieChild,
            ZombieFat,
            ZombieFighter,
            ZombieFlamer,
            // Tier1: Chemical Crisis
            ZombieGangster,
            ZombieNinja,
            ZombiePolice,

            // Tier3: Unearthed
            ZombiePrussian,
            BaronVonHauptstein,

            ZombieSoldier,
            ZombieThug,
            ZombieWorker,
        }

        #endregion

        #region Bot profiles

        private static List<IProfile> GetProfiles(BotType botType)
        {
            var profiles = new List<IProfile>();

            switch (botType)
            {
                case BotType.Agent:
                case BotType.Agent2:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Agent",
                        Accesory = new IProfileClothingItem("AgentSunglasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacketBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Agent",
                        Accesory = new IProfileClothingItem("AgentSunglasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacketBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.AssassinMelee:
                case BotType.AssassinRange:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Assassin",
                        Accesory = new IProfileClothingItem("Mask", "ClothingDarkBlue", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SweaterBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Assassin",
                        Accesory = new IProfileClothingItem("Mask", "ClothingDarkBlue", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SweaterBlack_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Assassin",
                        Accesory = new IProfileClothingItem("Balaclava", "ClothingDarkBlue", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SweaterBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Assassin",
                        Accesory = new IProfileClothingItem("Balaclava", "ClothingDarkBlue", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SweaterBlack_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Assassin",
                        Accesory = new IProfileClothingItem("Balaclava", "ClothingDarkBlue", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacketBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Assassin",
                        Accesory = new IProfileClothingItem("Mask", "ClothingDarkBlue", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacketBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Bandido:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Bandido",
                        Accesory = new IProfileClothingItem("Mask", "ClothingDarkRed", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("Poncho2", "ClothingDarkYellow", "ClothingLightYellow"),
                        ChestUnder = new IProfileClothingItem("Shirt", "ClothingDarkOrange", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Sombrero", "ClothingOrange", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants", "ClothingDarkRed", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingOrange", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Bandido",
                        Accesory = new IProfileClothingItem("Scarf", "ClothingLightOrange", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("UnbuttonedShirt", "ClothingDarkOrange", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGloves", "ClothingDarkYellow", "ClothingLightGray"),
                        Head = new IProfileClothingItem("Sombrero", "ClothingLightBrown", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants", "ClothingDarkRed", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingOrange", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Bandido",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("AmmoBelt", "ClothingDarkGray", "ClothingLightGray"),
                        ChestUnder = new IProfileClothingItem("UnbuttonedShirt", "ClothingDarkOrange", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants", "ClothingDarkYellow", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt", "ClothingOrange", "ClothingYellow"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Bandido",
                        Accesory = new IProfileClothingItem("Scarf", "ClothingOrange", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("AmmoBelt_fem", "ClothingDarkGray", "ClothingLightGray"),
                        ChestUnder = new IProfileClothingItem("UnbuttonedShirt_fem", "ClothingDarkOrange", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGloves", "ClothingGray", "ClothingLightGray"),
                        Head = new IProfileClothingItem("Sombrero2", "ClothingLightOrange", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightGray", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("AmmoBeltWaist_fem", "ClothingOrange", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Bandido",
                        Accesory = new IProfileClothingItem("Cigar", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("UnbuttonedShirt", "ClothingDarkOrange", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGloves", "ClothingDarkYellow", "ClothingLightGray"),
                        Head = new IProfileClothingItem("Sombrero", "ClothingLightBrown", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants", "ClothingDarkPurple", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingOrange", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Bandido",
                        Accesory = new IProfileClothingItem("Cigar", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("AmmoBelt_fem", "ClothingDarkGray", "ClothingLightGray"),
                        ChestUnder = new IProfileClothingItem("TrainingShirt_fem", "ClothingOrange", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Sombrero2", "ClothingLightOrange", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightYellow", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingDarkYellow"),
                        Waist = new IProfileClothingItem("SatchelBelt_fem", "ClothingOrange", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Bandido",
                        Accesory = new IProfileClothingItem("Mask", "ClothingDarkRed", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("Poncho_fem", "ClothingDarkOrange", "ClothingDarkYellow"),
                        ChestUnder = new IProfileClothingItem("ShirtWithBowtie_fem", "ClothingOrange", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Sombrero", "ClothingDarkPink", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingDarkOrange", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingDarkYellow"),
                        Waist = new IProfileClothingItem("AmmoBeltWaist_fem", "ClothingOrange", "ClothingLightGray"),
                    });
                    break;
                }
                case BotType.Biker:
                case BotType.BikerHulk:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("StuddedJacket_fem", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("TornPants_fem", "ClothingDarkPurple", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin1", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("StuddedVest", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("Headband", "ClothingLightBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingDarkPurple", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin2", "ClothingLightBlue", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("VestBlack_fem", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingDarkPink", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("AviatorHat", "ClothingBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("StuddedJacket_fem", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingDarkPink", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("StuddedVest", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("Headband", "ClothingLightBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("StuddedVest", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("AviatorHat", "ClothingBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("VestBlack_fem", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("TShirt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("Headband", "ClothingLightBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("TornPants_fem", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("StuddedVest", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirtBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("Headband", "ClothingLightBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightBlue", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("StuddedVest_fem", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("AviatorHat", "ClothingBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("StuddedJacket_fem", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("MotorcycleHelmet", "ClothingDarkBlue", "ClothingLightBlue", ""),
                        Legs = new IProfileClothingItem("TornPants_fem", "ClothingDarkPurple", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("StuddedVest", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingDarkPink", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("MotorcycleHelmet", "ClothingDarkBlue", "ClothingLightBlue", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingDarkYellow", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Biker",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("StuddedJacket", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingDarkPink", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingLightBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingLightBlue", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBlue", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.Bodyguard:
                case BotType.Bodyguard2:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Bodyguard",
                        Accesory = new IProfileClothingItem("AgentSunglasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacket", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("SweaterBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ClownBodyguard:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Clown Bodyguard",
                        Accesory = new IProfileClothingItem("ClownMakeup_fem", "ClothingLightRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacket_fem", "ClothingLightCyan", "ClothingLightGray", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("HighHeels", "ClothingLightCyan", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("BucketHat", "ClothingLightCyan", "ClothingLightGray", ""),
                        Legs = null,
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingLightCyan", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Clown Bodyguard",
                        Accesory = new IProfileClothingItem("ClownMakeup_fem", "ClothingLightRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacket_fem", "ClothingLightYellow", "ClothingLightGray", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("HighHeels", "ClothingLightYellow", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("BucketHat", "ClothingLightYellow", "ClothingLightGray", ""),
                        Legs = null,
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Clown Bodyguard",
                        Accesory = new IProfileClothingItem("ClownMakeup_fem", "ClothingLightRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacket_fem", "ClothingPink", "ClothingLightGray", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("HighHeels", "ClothingPink", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("BucketHat", "ClothingPink", "ClothingLightGray", ""),
                        Legs = null,
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingPink", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Clown Bodyguard",
                        Accesory = new IProfileClothingItem("ClownMakeup_fem", "ClothingLightRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacket_fem", "ClothingLightGreen", "ClothingLightGray", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("HighHeels", "ClothingLightGreen", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("BucketHat", "ClothingLightGreen", "ClothingLightGray", ""),
                        Legs = null,
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingLightGreen", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ClownBoxer:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Clown Boxer",
                        Accesory = new IProfileClothingItem("ClownMakeup", "ClothingLightRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingDarkOrange", "ClothingOrange", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("Gloves", "ClothingRed", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("StripedPants", "ClothingLightOrange", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightYellow", ""),
                    });
                    break;
                }
                case BotType.ClownCowboy:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Clown Cowboy",
                        Accesory = new IProfileClothingItem("ClownMakeup", "ClothingLightRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("Poncho", "ClothingPurple", "ClothingGreen", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithBowtie", "ClothingLightYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("RidingBootsBlack", "ClothingLightBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Fedora2", "ClothingOrange", "ClothingPurple", ""),
                        Legs = new IProfileClothingItem("CamoPants", "ClothingLightGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingDarkGray", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.ClownGangster:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Clown Gangster",
                        Accesory = new IProfileClothingItem("ClownMakeup", "ClothingLightRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingBrown", "ClothingLightYellow", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingLightBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("StylishHat", "ClothingPurple", "ClothingLightGreen", ""),
                        Legs = new IProfileClothingItem("StripedPants", "ClothingPurple", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingBrown", "ClothingLightYellow", ""),
                    });
                    break;
                }
                case BotType.Cowboy:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Cowboy",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Vest", "ClothingBrown", "ClothingBrown", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithBowtie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("CowboyHat", "ClothingLightBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBrown", "ClothingLightYellow", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Cowboy",
                        Accesory = new IProfileClothingItem("Scarf", "ClothingLightOrange", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("UnbuttonedShirt", "ClothingDarkOrange", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGloves", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("Fedora", "ClothingLightBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingDarkRed", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingOrange", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Cowboy",
                        Accesory = new IProfileClothingItem("Scarf", "ClothingLightYellow", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("UnbuttonedShirt", "ClothingLightYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("RidingBootsBlack", "ClothingDarkOrange", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Fedora2", "ClothingBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkOrange", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Cowboy",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LumberjackShirt2", "ClothingDarkPink", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("CowboyHat", "ClothingLightBrown", "ClothingLightGreen", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBrown", "ClothingLightYellow", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Cowboy",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Vest", "ClothingBrown", "ClothingBrown", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithBowtie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("RidingBootsBlack", "ClothingBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingDarkBrown", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Cowboy",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LumberjackShirt2", "ClothingDarkRed", "ClothingDarkRed", ""),
                        Feet = new IProfileClothingItem("RidingBoots", "ClothingBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("CowboyHat", "ClothingDarkBrown", "ClothingLightBrown", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingDarkBrown", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Cowboy",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Vest", "ClothingDarkGray", "ClothingDarkGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithBowtie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("RidingBootsBlack", "ClothingBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("CowboyHat", "ClothingBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkBrown", "ClothingLightYellow", ""),
                    });
                    break;
                }
                case BotType.Demolitionist:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "The Demolitionist",
                        Accesory = new IProfileClothingItem("AgentSunglasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("GrenadeBelt", "ClothingLightGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("Gloves", "ClothingGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin5", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.Elf:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Elf",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LeatherJacket", "ClothingGreen", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("SantaHat", "ClothingGreen", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGreen", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingPink", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkGreen", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Elf",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LeatherJacket_fem", "ClothingGreen", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("SantaHat", "ClothingGreen", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingGreen", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin3", "ClothingPink", ""),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkGreen", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.Fritzliebe:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Dr. Fritzliebe",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LeatherJacket", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("FLDisguise", "ClothingLightGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Funnyman:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Funnyman",
                        Accesory = new IProfileClothingItem("ClownMakeup", "ClothingLightRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("StripedSuitJacket", "ClothingLightBlue", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithBowtie", "ClothingYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingLightYellow", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("Gloves", "ClothingLightGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("StripedPants", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Jo:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Jo",
                        Accesory = new IProfileClothingItem("Cigar", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkOrange", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("TornPants_fem", "ClothingLightBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("SmallBelt_fem", "ClothingLightBrown", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.Hacker:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Hacker",
                        Accesory = new IProfileClothingItem("Goggles", "ClothingDarkGreen", "ClothingLightCyan", ""),
                        ChestOver = new IProfileClothingItem("Jacket", "ClothingDarkGray", "ClothingLightCyan", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingOrange", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGloves", "ClothingLightGray", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("BaseballCap", "ClothingDarkGray", "ClothingLightCyan", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Hacker",
                        Accesory = new IProfileClothingItem("Vizor", "ClothingDarkGray", "ClothingLightRed", ""),
                        ChestOver = new IProfileClothingItem("Jacket", "ClothingDarkGray", "ClothingLightCyan", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingOrange", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGloves", "ClothingLightGray", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("BaseballCap", "ClothingDarkGray", "ClothingLightCyan", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Gangster:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingGray", "ClothingDarkYellow", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("BlazerWithShirt", "ClothingGray", "ClothingDarkYellow", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Flatcap", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("SuitJacket", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingGray", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin4", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("BlazerWithShirt", "ClothingGray", "ClothingDarkPink", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Flatcap", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin4", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Jacket", "ClothingGray", "ClothingGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingDarkYellow", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Fedora", "ClothingGray", "ClothingDarkPink", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingBrown", "ClothingDarkYellow", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingGray", "ClothingDarkYellow", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Flatcap", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("BlazerWithShirt", "ClothingGray", "ClothingDarkYellow", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("StylishHat", "ClothingGray", "ClothingDarkPink", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin1", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("SuitJacket", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingGray", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Fedora", "ClothingGray", "ClothingDarkPink", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin2", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Jacket", "ClothingGray", "ClothingDarkGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingDarkYellow", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Flatcap", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin1", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightYellow", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Jacket", "ClothingGray", "ClothingGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingDarkYellow", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingBrown", "ClothingDarkYellow", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("BlazerWithShirt", "ClothingGray", "ClothingDarkPink", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin2", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("BlazerWithShirt_fem", "ClothingGray", "ClothingDarkPink", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("HighHeels", "ClothingDarkPink", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Fedora", "ClothingGray", "ClothingDarkPink", ""),
                        Legs = new IProfileClothingItem("Skirt_fem", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin2", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.GangsterHulk:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster Hulk",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingBrown", "ClothingDarkYellow", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Flatcap", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster Hulk",
                        Accesory = new IProfileClothingItem("Cigar", "ClothingBrown", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingBrown", "ClothingDarkYellow", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Gangster Hulk",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingBrown", "ClothingDarkYellow", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("BucketHat", "ClothingGray", "ClothingGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightYellow", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Incinerator:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "The Incinerator",
                        Accesory = new IProfileClothingItem("GasMask", "ClothingDarkYellow", "ClothingLightOrange", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LeatherJacketBlack", "ClothingDarkYellow", "ClothingOrange", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkOrange", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("Headband", "ClothingOrange", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkYellow", "ClothingLightOrange", ""),
                    });
                    break;
                }
                case BotType.Kingpin:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Kingpin",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("StripedSuitJacket", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithBowtie", "ClothingPink", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("Gloves", "ClothingLightGray", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("TopHat", "ClothingDarkGray", "ClothingPink", ""),
                        Legs = new IProfileClothingItem("StripedPants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Kriegbär:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Kriegbär #2",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = null,
                        Skin = new IProfileClothingItem("FrankenbearSkin", "ClothingDarkGray", "ClothingLightBlue", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.MarauderBiker:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Marauder",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("JacketBlack", "ClothingDarkGray", "ClothingGray", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("MotorcycleHelmet", "ClothingLightRed", "ClothingDarkGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin5", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.MarauderCrazy:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Marauder",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Cap", "ClothingBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin5", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingBrown", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.MarauderNaked:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Marauder",
                        Accesory = new IProfileClothingItem("DogTag", "ClothingLightGray", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("Sneakers", "ClothingGray", "ClothingGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Cap", "ClothingDarkGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin5", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.MarauderRifleman:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Marauder",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Cap", "ClothingDarkGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin5", "ClothingLightRed", ""),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingBrown", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.MarauderRobber:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Marauder",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("JacketBlack", "ClothingDarkGray", "ClothingGray", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin5", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.MarauderTough:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Marauder",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("KevlarVest", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("LumberjackShirt2", "ClothingBrown", "ClothingDarkBrown", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet2", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin5", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Meatgrinder:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "The Meatgrinder",
                        Accesory = new IProfileClothingItem("GoalieMask", "ClothingLightGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("Apron", "ClothingLightPink", "ClothingLightGray"),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingDarkRed", "ClothingLightGray"),
                        Head = new IProfileClothingItem("ChefHat", "ClothingLightGray", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingGray", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingPink"),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Mecha:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Mecha Fritzliebe",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = null,
                        Skin = new IProfileClothingItem("MechSkin", "ClothingLightGray", "ClothingYellow", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.MetroCop:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "MetroCop",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("MetroLawJacket", "ClothingGray", "ClothingGray"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingGreen", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingGray", "ClothingLightGray"),
                        Head = new IProfileClothingItem("MetroLawGasMask", "ClothingGray", "ClothingLightGreen"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "MetroCop",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("MetroLawJacket", "ClothingGray", "ClothingGray"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingGreen", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingGray", "ClothingLightGray"),
                        Head = new IProfileClothingItem("MetroLawMask", "ClothingGray", "ClothingLightGreen"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightRed"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "MetroCop",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("BodyArmor", "ClothingGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingGray", "ClothingLightGray"),
                        Head = new IProfileClothingItem("MetroLawGasMask", "ClothingGray", "ClothingLightRed"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos", "Skin5", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingGray", "ClothingLightGray"),
                    });
                    break;
                }
                case BotType.MetroCop2:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "MetroCop",
                        Accesory = new IProfileClothingItem("Earpiece", "ClothingLightGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("MetroLawJacket", "ClothingGray", "ClothingGray"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingGreen", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingGray", "ClothingLightGray"),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightRed"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "MetroCop",
                        Accesory = new IProfileClothingItem("Earpiece", "ClothingLightGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("BodyArmor", "ClothingGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingGray", "ClothingLightGray"),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "MetroCop",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("MetroLawJacket", "ClothingGray", "ClothingGray"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingGreen", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingGray", "ClothingLightGray"),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightRed"),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Mutant:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Mutant",
                        Accesory = new IProfileClothingItem("RestraintMask", "ClothingLightCyan", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirtBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingLightBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Mutant",
                        Accesory = new IProfileClothingItem("GasMask", "ClothingDarkGreen", "ClothingLightGreen", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirtBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingLightBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Mutant",
                        Accesory = new IProfileClothingItem("GasMask", "ClothingDarkGreen", "ClothingLightGreen", ""),
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingLightBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Mutant",
                        Accesory = new IProfileClothingItem("RestraintMask", "ClothingCyan", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin5", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingLightBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Mutant",
                        Accesory = new IProfileClothingItem("RestraintMask", "ClothingLightGray", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingLightBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Mutant",
                        Accesory = new IProfileClothingItem("RestraintMask", "ClothingLightCyan", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirtBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingLightBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Mutant",
                        Accesory = new IProfileClothingItem("GasMask", "ClothingDarkGreen", "ClothingLightGreen", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingLightBlue", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Mutant",
                        Accesory = new IProfileClothingItem("GasMask", "ClothingDarkGreen", "ClothingLightGreen", ""),
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin5", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("CombatBelt", "ClothingLightBlue", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.NaziLabAssistant:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Nazi Lab Assistant",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.NaziMuscleSoldier:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Nazi Soldier",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt", "ClothingLightBrown", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBrown", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.NaziScientist:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Nazi Scientist",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LeatherJacket", "ClothingCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("HazmatMask", "ClothingCyan", "ClothingLightGreen", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Nazi Scientist",
                        Accesory = new IProfileClothingItem("Armband_fem", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LeatherJacket_fem", "ClothingCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack_fem", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("HazmatMask", "ClothingCyan", "ClothingLightGreen", ""),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.NaziSoldier:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Nazi Soldier",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("MetroLawJacket", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("GermanHelmet", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Nazi Soldier",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt", "ClothingLightBrown", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Cap", "ClothingBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBrown", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Nazi Soldier",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt", "ClothingLightBrown", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("GermanHelmet", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBrown", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Nazi Soldier",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("MetroLawJacket", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("SpikedHelmet", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.SSOfficer:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "SS Officer",
                        Accesory = new IProfileClothingItem("Armband", "ClothingRed", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("OfficerJacket", "ClothingDarkGray", "ClothingLightYellow", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("OfficerHat", "ClothingDarkGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Ninja:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Ninja",
                        Accesory = new IProfileClothingItem("Balaclava", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SweaterBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Ninja",
                        Accesory = new IProfileClothingItem("Balaclava", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SweaterBlack_fem", "ClothingDarkGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin3", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Ninja",
                        Accesory = new IProfileClothingItem("Mask", "ClothingDarkRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SweaterBlack_fem", "ClothingDarkGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin3", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Ninja",
                        Accesory = new IProfileClothingItem("Mask", "ClothingDarkRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SweaterBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Police:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Police Officer",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin4", "ClothingLightGray"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Police Officer",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Police Officer",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Police Officer",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin1", "ClothingLightGray"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Police Officer",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt_fem", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin3", "ClothingLightGray"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Police Officer",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt_fem", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingLightGray"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Police Officer",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt_fem", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin1", "ClothingLightGray"),
                        Waist = null,
                    });
                    break;
                }
                case BotType.PoliceSWAT:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "SWAT",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("KevlarVest_fem", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("PoliceShirt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet2", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "SWAT",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("KevlarVest", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("PoliceShirt", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet2", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Santa:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Bad Santa",
                        Accesory = new IProfileClothingItem("SantaMask", "ClothingLightGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("Coat", "ClothingRed", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("SafetyGlovesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Head = new IProfileClothingItem("SantaHat", "ClothingRed", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingRed", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingPink", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkRed", "ClothingLightYellow", ""),
                    });
                    break;
                }
                case BotType.Sniper:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Sniper",
                        Accesory = new IProfileClothingItem("Vizor", "ClothingDarkGray", "ClothingLightRed", ""),
                        ChestOver = new IProfileClothingItem("AmmoBelt", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingDarkGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("Gloves", "ClothingGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("CamoPants", "ClothingDarkGreen", "ClothingDarkGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingGray", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Sniper",
                        Accesory = new IProfileClothingItem("Vizor", "ClothingDarkGray", "ClothingLightRed", ""),
                        ChestOver = new IProfileClothingItem("AmmoBelt", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingDarkGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("Gloves", "ClothingGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("CamoPants", "ClothingDarkGreen", "ClothingDarkGray", ""),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingGray", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.Soldier:
                case BotType.Soldier2:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt", "ClothingDarkYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin4", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingDarkYellow", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt", "ClothingDarkYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingDarkYellow", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt", "ClothingDarkYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin2", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingDarkYellow", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt", "ClothingDarkYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Tattoos", "Skin1", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingDarkYellow", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt_fem", "ClothingDarkYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants_fem", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin4", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("SatchelBelt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt_fem", "ClothingDarkYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants_fem", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin3", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("SatchelBelt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt_fem", "ClothingDarkYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants_fem", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin2", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("SatchelBelt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("MilitaryShirt_fem", "ClothingDarkYellow", "ClothingLightBlue", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants_fem", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin1", "ClothingLightYellow", ""),
                        Waist = new IProfileClothingItem("SatchelBelt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.Teddybear:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Teddybear",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = null,
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = null,
                        Skin = new IProfileClothingItem("BearSkin", "Skin1", "ClothingLightGray"),
                        Waist = null,
                    });
                    break;
                }
                case BotType.Thug:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin3", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("MotorcycleHelmet", "ClothingDarkCyan", "ClothingLightYellow"),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TShirt_fem", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("MotorcycleHelmet", "ClothingDarkCyan", "ClothingLightYellow"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin3", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("StuddedVest", "ClothingBlue", "ClothingBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray"),
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingLightGreen"),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingBrown", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("StuddedVest_fem", "ClothingBlue", "ClothingBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray"),
                        Head = null,
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin3", "ClothingLightYellow"),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("StuddedVest", "ClothingBlue", "ClothingBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray"),
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos", "Skin2", "ClothingLightGreen"),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingBrown", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("StuddedVest_fem", "ClothingBlue", "ClothingBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray"),
                        Head = new IProfileClothingItem("Headband", "ClothingRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin2", "ClothingLightGreen"),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingBrown", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("VestBlack_fem", "ClothingBlue", "ClothingBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingBlue", "ClothingLightGray"),
                        Head = new IProfileClothingItem("MotorcycleHelmet", "ClothingDarkGreen", "ClothingLightYellow"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos_fem", "Skin1", "ClothingLightOrange"),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingBrown", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("StuddedVest", "ClothingBlue", "ClothingBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin1", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("VestBlack_fem", "ClothingBlue", "ClothingBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("BaseballCap", "ClothingRed", "ClothingLightRed"),
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin1", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Vest", "ClothingLightBlue", "ClothingLightBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingDarkRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingLightGray"),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("DogTag", "ClothingLightGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("VestBlack", "ClothingDarkBlue", "ClothingBlue"),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGloves", "ClothingDarkGray", "ClothingLightGray"),
                        Head = new IProfileClothingItem("WoolCap", "ClothingLightRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("TornPants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingPink"),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGloves", "ClothingBrown", "ClothingLightGray"),
                        Head = new IProfileClothingItem("Headband", "ClothingLightRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("TornPants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos", "Skin3", "ClothingLightYellow"),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = new IProfileClothingItem("VestBlack_fem", "ClothingBlue", "ClothingDarkBlue"),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingBlue", "ClothingLightGray"),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("GlovesBlack", "ClothingBlue", "ClothingLightGray"),
                        Head = new IProfileClothingItem("Headband", "ClothingLightRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("TornPants_fem", "ClothingDarkPurple", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal_fem", "Skin2", "ClothingDarkYellow"),
                        Waist = new IProfileClothingItem("Belt_fem", "ClothingDarkBlue", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Vest", "ClothingLightBlue", "ClothingLightBlue"),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingDarkBlue", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkBrown", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingRed", "ClothingLightGray"),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Tattoos", "Skin1", "ClothingDarkYellow"),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ThugHulk:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug Hulk",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin3", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug Hulk",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin2", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightGray"),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Thug Hulk",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray"),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingLightGray", "ClothingLightGray"),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray"),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingLightBlue", "ClothingLightGray"),
                        Skin = new IProfileClothingItem("Normal", "Skin1", "ClothingLightGray"),
                        Waist = new IProfileClothingItem("Belt", "ClothingGray", "ClothingLightGray"),
                    });
                    break;
                }
                case BotType.Zombie:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = null,
                        Gender = Gender.Female,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("TornPants_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie_fem", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("TornPants", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieAgent:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Agent",
                        Accesory = new IProfileClothingItem("SunGlasses", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("SuitJacketBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieBruiser:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Bruiser",
                        Accesory = new IProfileClothingItem("RestraintMask", "ClothingGray", "ClothingLightGray", ""),
                        ChestOver = new IProfileClothingItem("VestBlack", "ClothingBlue", "ClothingDarkBlue", ""),
                        ChestUnder = null,
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("TornPants", "ClothingDarkPurple", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieChild:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Child",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt_fem", "ClothingPurple", "ClothingLightGray", ""),
                        Feet = null,
                        Gender = Gender.Female,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("TornPants_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie_fem", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Child",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt", "ClothingPurple", "ClothingLightGray", ""),
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("TornPants", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieFat:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Fat Zombie",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("ShoulderHolster", "ClothingRed", "ClothingLightGray", ""),
                        ChestUnder = null,
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Shorts", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieFighter:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Dead Cop",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("Sweater", "ClothingGreen", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = new IProfileClothingItem("FingerlessGloves", "ClothingDarkGray", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkGray", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Dead Merc",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Jacket", "ClothingBrown", "ClothingLightBrown", ""),
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("AviatorHat", "ClothingBrown", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingBrown", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Dead Vigilante",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TShirt", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Cap", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Dead Spy",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("SuitJacketBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithBowtie", "ClothingLightGray", "ClothingDarkGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Dead Pilot",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("ShoulderHolster", "ClothingDarkBrown", "ClothingDarkBrown", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingLightGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("SmallBelt", "ClothingDarkGray", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Dead Driver",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Jacket", "ClothingBrown", "ClothingBrown", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingGray", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Belt", "ClothingDarkGray", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.ZombieFlamer:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Flamer",
                        Accesory = new IProfileClothingItem("Glasses", "ClothingLightYellow", "ClothingLightYellow", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("SleevelessShirtBlack", "ClothingGray", "ClothingLightGray", ""),
                        Feet = null,
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("ShortsBlack", "ClothingDarkGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieGangster:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("BlazerWithShirt", "ClothingGray", "ClothingLightBlue", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("StylishHat", "ClothingGray", "ClothingPink", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingGray", "ClothingDarkYellow", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingPink", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Flatcap", "ClothingGray", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("SuitJacket", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("StylishHat", "ClothingGray", "ClothingDarkYellow", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("SuitJacket", "ClothingGray", "ClothingLightGray", ""),
                        ChestUnder = new IProfileClothingItem("ShirtWithTie", "ClothingGray", "ClothingDarkPink", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Fedora", "ClothingGray", "ClothingDarkPink", ""),
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Gangster",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("BlazerWithShirt", "ClothingGray", "ClothingDarkPink", ""),
                        ChestUnder = null,
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingGray", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = null,
                        Legs = new IProfileClothingItem("Pants", "ClothingGray", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieNinja:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Ninja",
                        Accesory = new IProfileClothingItem("Mask", "ClothingDarkRed", "ClothingLightGray", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TrainingShirt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = new IProfileClothingItem("FingerlessGlovesBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Head = null,
                        Legs = new IProfileClothingItem("Pants_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie_fem", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("Sash_fem", "ClothingDarkRed", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.ZombiePolice:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Police",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie_fem", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Police",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("PoliceShirt", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Shoes", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("PoliceHat", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombiePrussian:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Prussian",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("LeatherJacketBlack", "ClothingCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("SpikedHelmet", "ClothingCyan", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Prussian",
                        Accesory = new IProfileClothingItem("GasMask", "ClothingCyan", "ClothingLightGreen", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("SpikedHelmet", "ClothingCyan", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.BaronVonHauptstein:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "BaronVonHauptstein", // TODO
                        Accesory = new IProfileClothingItem("GasMask", "ClothingCyan", "ClothingLightGreen", ""),
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("BootsBlack", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("SpikedHelmet", "ClothingCyan", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingDarkCyan", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieSoldier:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt_fem", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkRed", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants_fem", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Zombie_fem", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("AmmoBeltWaist_fem", "ClothingBrown", "ClothingLightGray", ""),
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Soldier",
                        Accesory = null,
                        ChestOver = null,
                        ChestUnder = new IProfileClothingItem("TornShirt", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkRed", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Helmet", "ClothingDarkYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("CamoPants", "ClothingDarkYellow", "ClothingDarkYellow", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("AmmoBeltWaist", "ClothingBrown", "ClothingLightGray", ""),
                    });
                    break;
                }
                case BotType.ZombieThug:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Thug",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Vest_fem", "ClothingLightBlue", "ClothingLightBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Female,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingDarkRed", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack_fem", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie_fem", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Thug",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Vest", "ClothingLightBlue", "ClothingLightBlue", ""),
                        ChestUnder = new IProfileClothingItem("SleevelessShirt", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("Boots", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Headband", "ClothingDarkRed", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("PantsBlack", "ClothingDarkBlue", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = null,
                    });
                    break;
                }
                case BotType.ZombieWorker:
                {
                    profiles.Add(new IProfile()
                    {
                        Name = "Zombie Worker",
                        Accesory = null,
                        ChestOver = new IProfileClothingItem("Suspenders", "ClothingOrange", "ClothingLightOrange", ""),
                        ChestUnder = new IProfileClothingItem("TornShirt", "ClothingOrange", "ClothingLightGray", ""),
                        Feet = new IProfileClothingItem("ShoesBlack", "ClothingDarkBrown", "ClothingLightGray", ""),
                        Gender = Gender.Male,
                        Hands = null,
                        Head = new IProfileClothingItem("Cap", "ClothingYellow", "ClothingLightGray", ""),
                        Legs = new IProfileClothingItem("TornPants", "ClothingOrange", "ClothingLightGray", ""),
                        Skin = new IProfileClothingItem("Zombie", "Skin1", "ClothingLightGray", ""),
                        Waist = new IProfileClothingItem("SatchelBelt", "ClothingOrange", "ClothingLightGray", ""),
                    });
                    break;
                }
            }

            return profiles;
        }

        #endregion

        #region Bot weapons

        public class WeaponSet
        {
            public WeaponSet()
            {
                Melee = WeaponItem.NONE;
                Primary = WeaponItem.NONE;
                Secondary = WeaponItem.NONE;
                Throwable = WeaponItem.NONE;
                Powerup = WeaponItem.NONE;
                UseLazer = false;
            }

            public void Equip(IPlayer player)
            {
                player.GiveWeaponItem(Melee);
                player.GiveWeaponItem(Primary);
                player.GiveWeaponItem(Secondary);
                player.GiveWeaponItem(Throwable);
                player.GiveWeaponItem(Powerup);

                if (UseLazer) player.GiveWeaponItem(WeaponItem.LAZER);
            }

            static WeaponSet()
            {
                Empty = new WeaponSet();
            }

            public static WeaponSet Empty { get; private set; }

            public WeaponItem Melee { get; set; }
            public WeaponItem Primary { get; set; }
            public WeaponItem Secondary { get; set; }
            public WeaponItem Throwable { get; set; }
            public WeaponItem Powerup { get; set; }
            public bool UseLazer { get; set; }
        }

        private static List<WeaponSet> GetWeapons(BotType botType)
        {
            var weapons = new List<WeaponSet>();

            switch (botType)
            {
                case BotType.Agent:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                        UseLazer = true,
                    });
                    break;
                }
                case BotType.Agent2:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.SHOCK_BATON,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.MAGNUM,
                        UseLazer = true,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                        Secondary = WeaponItem.UZI,
                        UseLazer = true,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                        UseLazer = true,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.DARK_SHOTGUN,
                        UseLazer = true,
                    });
                    break;
                }
                case BotType.AssassinMelee:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KATANA,
                    });
                    break;
                }
                case BotType.AssassinRange:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.UZI,
                    });
                    break;
                }
                case BotType.Bandido:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.MACHETE,
                        Secondary = WeaponItem.REVOLVER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.CARBINE,
                        Secondary = WeaponItem.REVOLVER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.SHOTGUN,
                        Secondary = WeaponItem.PISTOL,
                    });
                    break;
                }
                case BotType.Biker:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.CHAIN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    });
                    break;
                }
                case BotType.BikerHulk:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.Bodyguard:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    });
                    break;
                }
                case BotType.Bodyguard2:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    });
                    break;
                }
                case BotType.ClownBodyguard:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KATANA,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.AXE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BAT,
                    });
                    break;
                }
                case BotType.ClownBoxer:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.ClownCowboy:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    });
                    break;
                }
                case BotType.ClownGangster:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    });
                    break;
                }
                case BotType.Cowboy:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SAWED_OFF,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.MAGNUM,
                    });
                    break;
                }
                case BotType.Demolitionist:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SNIPER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.GRENADE_LAUNCHER,
                    });
                    break;
                }
                case BotType.Elf:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.CHAIN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.FLAMETHROWER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.UZI,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.FLAREGUN,
                    });
                    break;
                }
                case BotType.Fritzliebe:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.Funnyman:
                {
                    weapons.Add(WeaponSet.Empty);
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    });
                    break;
                }
                case BotType.Jo:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BOTTLE,
                        Powerup = WeaponItem.SLOWMO_10,
                    });
                    break;
                }
                case BotType.Hacker:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.Gangster:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BAT,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BOTTLE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.UZI,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SAWED_OFF,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                    });
                    break;
                }
                case BotType.GangsterHulk:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.Incinerator:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.AXE,
                        Primary = WeaponItem.FLAMETHROWER,
                        Secondary = WeaponItem.FLAREGUN,
                        Throwable = WeaponItem.MOLOTOVS,
                        Powerup = WeaponItem.SLOWMO_10,
                    });
                    break;
                }
                case BotType.Kingpin:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.MAGNUM,
                    });
                    break;
                }
                case BotType.Kriegbär:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Powerup = WeaponItem.SLOWMO_10,
                    });
                    break;
                }
                case BotType.MarauderBiker:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SMG,
                    });
                    break;
                }
                case BotType.MarauderCrazy:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    });
                    break;
                }
                case BotType.MarauderNaked:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.MACHETE,
                    });
                    break;
                }
                case BotType.MarauderRifleman:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SAWED_OFF,
                    });
                    break;
                }
                case BotType.MarauderRobber:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    });
                    break;
                }
                case BotType.MarauderTough:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    });
                    break;
                }
                case BotType.Meatgrinder:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.CHAINSAW,
                        Throwable = WeaponItem.MOLOTOVS,
                        Powerup = WeaponItem.SLOWMO_10,
                    });
                    break;
                }
                case BotType.Mecha:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.MetroCop:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.SHOCK_BATON,
                        Primary = WeaponItem.SMG,
                        UseLazer = true,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.SHOCK_BATON,
                        Primary = WeaponItem.DARK_SHOTGUN,
                        UseLazer = true,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.ASSAULT,
                        UseLazer = true,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.DARK_SHOTGUN,
                        UseLazer = true,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SMG,
                        UseLazer = true,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SHOCK_BATON,
                        UseLazer = true,
                    });
                    break;
                }
                case BotType.MetroCop2:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.SHOCK_BATON,
                        Secondary = WeaponItem.PISTOL,
                        UseLazer = true,
                    });
                    break;
                }
                case BotType.Mutant:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.NaziLabAssistant:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Powerup = WeaponItem.STRENGTHBOOST,
                    });
                    break;
                }
                case BotType.NaziMuscleSoldier:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    });
                    break;
                }
                case BotType.NaziScientist:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.CHAIR,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BOTTLE,
                    });
                    break;
                }
                case BotType.NaziSoldier:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                        Throwable = WeaponItem.GRENADES,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.MP50,
                        Throwable = WeaponItem.GRENADES,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.CARBINE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.CARBINE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.CARBINE,
                        Throwable = WeaponItem.GRENADES,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Secondary = WeaponItem.PISTOL,
                    });
                    break;
                }
                case BotType.SSOfficer:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                        Secondary = WeaponItem.PISTOL,
                    });
                    break;
                }
                case BotType.Ninja:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KATANA,
                        Powerup = WeaponItem.SLOWMO_10,
                    });
                    break;
                }
                case BotType.Police:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                        Secondary = WeaponItem.PISTOL,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                        Primary = WeaponItem.SHOTGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                        Secondary = WeaponItem.REVOLVER,
                    });
                    break;
                }
                case BotType.PoliceSWAT:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Secondary = WeaponItem.PISTOL45,
                        Throwable = WeaponItem.C4,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Secondary = WeaponItem.MACHINE_PISTOL,
                        Throwable = WeaponItem.GRENADES,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.ASSAULT,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.SMG,
                    });
                    break;
                }
                case BotType.Santa:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.M60,
                        Secondary = WeaponItem.UZI,
                    });
                    break;
                }
                case BotType.Sniper:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.SNIPER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SNIPER,
                        Secondary = WeaponItem.SILENCEDPISTOL,
                    });
                    break;
                }
                case BotType.Soldier:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.ASSAULT,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SMG,
                    });
                    break;
                }
                case BotType.Soldier2:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.GRENADE_LAUNCHER,
                    });
                    break;
                }
                case BotType.Teddybear:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Throwable = WeaponItem.GRENADES,
                        Powerup = WeaponItem.SLOWMO_10,
                    });
                    break;
                }
                case BotType.Thug:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BAT,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.HAMMER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.CHAIN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.MACHINE_PISTOL,
                    });
                    break;
                }
                case BotType.ThugHulk:
                {
                    weapons.Add(WeaponSet.Empty);
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.PIPE,
                    });
                    break;
                }
                case BotType.Zombie:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.ZombieAgent:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.SILENCEDPISTOL,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.SILENCEDUZI,
                    });
                    break;
                }
                case BotType.ZombieBruiser:
                case BotType.ZombieChild:
                case BotType.ZombieFat:
                case BotType.ZombieFlamer:
                {
                    weapons.Add(WeaponSet.Empty);
                    break;
                }
                case BotType.ZombieFighter:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Powerup = WeaponItem.SLOWMO_10,
                    });
                    break;
                }
                case BotType.ZombieGangster:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    });
                    break;
                }
                case BotType.ZombieNinja:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KATANA,
                    });
                    break;
                }
                case BotType.ZombiePolice:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.REVOLVER,
                    });
                    break;
                }
                case BotType.ZombiePrussian:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.CARBINE,
                        Throwable = WeaponItem.GRENADES,
                    });
                    break;
                }
                case BotType.BaronVonHauptstein:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Secondary = WeaponItem.REVOLVER,
                        Throwable = WeaponItem.GRENADES,
                    });
                    break;
                }
                case BotType.ZombieSoldier:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SMG,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.ASSAULT,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Throwable = WeaponItem.GRENADES,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Throwable = WeaponItem.MINES,
                    });
                    break;
                }
                case BotType.ZombieThug:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.BAT,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Throwable = WeaponItem.MOLOTOVS,
                    });
                    break;
                }
                case BotType.ZombieWorker:
                {
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.PIPE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.HAMMER,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.AXE,
                    });
                    weapons.Add(new WeaponSet()
                    {
                        Melee = WeaponItem.CHAINSAW,
                    });
                    break;
                }
            }

            return weapons;
        }

        #endregion

        #region Bot behaviors

        public enum BotAI
        {
            Debug,

            Hacker,
            Expert,
            Hard,
            Normal,
            Easy,

            MeleeExpert, // == BotAI.Hacker but with range weapons disabled
            MeleeHard, // == BotAI.Expert but with range weapons disabled
            RangeExpert, // == BotAI.Hacker but with melee weapons disabled
            RangeHard, // == BotAI.Expert but with melee weapons disabled

            Grunt,
            Hulk,

            Meatgrinder,
            Ninja,
            Sniper,
            Soldier,

            ZombieSlow,
            ZombieFast,
            ZombieHulk,
            ZombieFighter,
        }

        private static BotBehaviorSet GetBehaviorSet(BotAI botAI)
        {
            var botBehaviorSet = new BotBehaviorSet()
            {
                MeleeActions = BotMeleeActions.Default,
                MeleeActionsWhenHit = BotMeleeActions.DefaultWhenHit,
                MeleeActionsWhenEnraged = BotMeleeActions.DefaultWhenEnraged,
                MeleeActionsWhenEnragedAndHit = BotMeleeActions.DefaultWhenEnragedAndHit,
                ChaseRange = 44f,
                GuardRange = 40f,
            };

            switch (botAI)
            {
                case BotAI.Debug:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotD);
                    botBehaviorSet.RangedWeaponBurstTimeMin = 5000;
                    botBehaviorSet.RangedWeaponBurstTimeMax = 5000;
                    botBehaviorSet.RangedWeaponBurstPauseMin = 0;
                    botBehaviorSet.RangedWeaponBurstPauseMax = 0;
                    break;
                }
                case BotAI.Easy:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotD);
                    break;
                }
                case BotAI.Normal:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotC);
                    break;
                }
                case BotAI.Hard:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotB);
                    break;
                }
                case BotAI.Expert:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotA);
                    break;
                }
                case BotAI.Hacker:
                {
                    botBehaviorSet.NavigationMode = BotBehaviorNavigationMode.PathFinding;
                    botBehaviorSet.MeleeMode = BotBehaviorMeleeMode.Default;
                    botBehaviorSet.EliminateEnemies = true;
                    botBehaviorSet.SearchForItems = true;
                    botBehaviorSet.OffensiveEnrageLevel = 0.8f;
                    botBehaviorSet.NavigationRandomPausesLevel = 0.1f;
                    botBehaviorSet.DefensiveRollFireLevel = 0.95f;
                    botBehaviorSet.DefensiveAvoidProjectilesLevel = 0.7f;
                    botBehaviorSet.OffensiveClimbingLevel = 0.7f;
                    botBehaviorSet.OffensiveSprintLevel = 0.6f;
                    botBehaviorSet.OffensiveDiveLevel = 0.6f;
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0.9f;
                    botBehaviorSet.ChokePointPlayerCountThreshold = 1;
                    botBehaviorSet.ChokePointValue = 150f;
                    botBehaviorSet.MeleeWaitTimeLimitMin = 100f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 200f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.SetMeleeActionsToExpert();
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.RangedWeaponUsage = true;
                    botBehaviorSet.RangedWeaponAccuracy = 0.85f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 50f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMin = 50f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMax = 50f;
                    botBehaviorSet.RangedWeaponBurstTimeMin = 400f;
                    botBehaviorSet.RangedWeaponBurstTimeMax = 800f;
                    botBehaviorSet.RangedWeaponBurstPauseMin = 400f;
                    botBehaviorSet.RangedWeaponBurstPauseMax = 800f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 800f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.95f;
                    botBehaviorSet.RangedWeaponPrecisionAimShootDelayMin = 25f;
                    botBehaviorSet.RangedWeaponPrecisionAimShootDelayMax = 50f;
                    botBehaviorSet.RangedWeaponPrecisionBurstTimeMin = 800f;
                    botBehaviorSet.RangedWeaponPrecisionBurstTimeMax = 1600f;
                    botBehaviorSet.RangedWeaponPrecisionBurstPauseMin = 100f;
                    botBehaviorSet.RangedWeaponPrecisionBurstPauseMax = 200f;
                    break;
                }
                case BotAI.MeleeExpert:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0.9f;
                    botBehaviorSet.MeleeWaitTimeLimitMin = 600f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 800f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.MeleeWeaponUseFullRange = true;
                    break;
                }
                case BotAI.MeleeHard:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0.75f;
                    botBehaviorSet.MeleeWaitTimeLimitMin = 800f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 1000f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.MeleeWeaponUseFullRange = false;
                    break;
                }
                case BotAI.Ninja: // == BotAI.MeleeExpert + more offensive melee tactic
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0.9f;
                    botBehaviorSet.MeleeWaitTimeLimitMin = 600f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 800f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.MeleeWeaponUseFullRange = true;

                    botBehaviorSet.SearchForItems = true;
                    botBehaviorSet.SearchItems = SearchItems.Melee;
                    botBehaviorSet.OffensiveEnrageLevel = 0.5f;
                    botBehaviorSet.NavigationRandomPausesLevel = 0.1f;
                    botBehaviorSet.DefensiveRollFireLevel = 0.95f;
                    botBehaviorSet.DefensiveAvoidProjectilesLevel = 0.9f;
                    botBehaviorSet.OffensiveClimbingLevel = 0.9f;
                    botBehaviorSet.OffensiveSprintLevel = 0.9f;
                    botBehaviorSet.OffensiveDiveLevel = 0.1f; // 0.7f
                    break;
                }
                case BotAI.RangeExpert:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.RangedA);
                    botBehaviorSet.RangedWeaponAccuracy = 0.85f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 600f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 2000f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.95f;
                    break;
                }
                case BotAI.RangeHard:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.RangedA);
                    botBehaviorSet.RangedWeaponAccuracy = 0.75f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 600f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 2000f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.9f;
                    break;
                }
                case BotAI.Sniper: // == BotAI.RangeExpert + more defensive melee tactic
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.RangedA);
                    botBehaviorSet.RangedWeaponMode = BotBehaviorRangedWeaponMode.ManualAim;
                    botBehaviorSet.RangedWeaponAccuracy = 0.85f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 600f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 2000f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.95f;

                    botBehaviorSet.DefensiveRollFireLevel = 0.95f;
                    botBehaviorSet.DefensiveAvoidProjectilesLevel = 0.6f;
                    botBehaviorSet.OffensiveEnrageLevel = 0.2f;
                    botBehaviorSet.OffensiveClimbingLevel = 0f;
                    botBehaviorSet.OffensiveSprintLevel = 0f;
                    botBehaviorSet.OffensiveDiveLevel = 0f;
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0f;
                    botBehaviorSet.TeamLineUp = false;
                    break;
                }
                case BotAI.Grunt:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Grunt);

                    // Taken from PredefinedAIType.BotB, PredefinedAIType.Grunt is too slow in shooting
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 200f;
                    botBehaviorSet.RangedWeaponAimShootDelayMax = 600f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMin = 200f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMax = 600f;
                    botBehaviorSet.RangedWeaponBurstTimeMin = 400f;
                    botBehaviorSet.RangedWeaponBurstTimeMax = 800f;
                    botBehaviorSet.RangedWeaponBurstPauseMin = 400f;
                    botBehaviorSet.RangedWeaponBurstPauseMax = 800f;
                    break;
                }
                case BotAI.Hulk:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Hulk);
                    break;
                }
                case BotAI.Meatgrinder:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Meatgrinder);
                    break;
                }
                case BotAI.ZombieSlow:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.ZombieA);
                    break;
                }
                case BotAI.ZombieFast:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.ZombieB);
                    break;
                }
                case BotAI.ZombieHulk:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Hulk);
                    botBehaviorSet.AttackDeadEnemies = true;
                    botBehaviorSet.SearchForItems = false;
                    botBehaviorSet.MeleeWeaponUsage = false;
                    botBehaviorSet.RangedWeaponUsage = false;
                    botBehaviorSet.PowerupUsage = false;
                    botBehaviorSet.ChokePointValue = 32f;
                    botBehaviorSet.ChokePointPlayerCountThreshold = 5;
                    botBehaviorSet.DefensiveRollFireLevel = 0.1f;
                    botBehaviorSet.OffensiveDiveLevel = 0f;
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0f;
                    break;
                }
                case BotAI.ZombieFighter:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    botBehaviorSet.AttackDeadEnemies = true;
                    botBehaviorSet.SearchForItems = false;
                    botBehaviorSet.MeleeWeaponUsage = false;
                    botBehaviorSet.RangedWeaponUsage = false;
                    botBehaviorSet.PowerupUsage = false;
                    botBehaviorSet.ChokePointValue = 32f;
                    botBehaviorSet.ChokePointPlayerCountThreshold = 5;
                    botBehaviorSet.DefensiveRollFireLevel = 0.1f;
                    botBehaviorSet.OffensiveDiveLevel = 0f;
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0f;
                    break;
                }
                default:
                {
                    botBehaviorSet.NavigationMode = BotBehaviorNavigationMode.None;
                    botBehaviorSet.MeleeMode = BotBehaviorMeleeMode.None;
                    botBehaviorSet.EliminateEnemies = false;
                    botBehaviorSet.SearchForItems = false;
                    break;
                }
            }

            return botBehaviorSet;
        }

        #endregion

        #region Bot infos

        public class BotInfo
        {
            public BotInfo()
            {
                EquipWeaponChance = 1f;
                OnSpawn = null;
                UpdateInterval = 1000;
                OnUpdate = null;
                OnDamage = null;
                OnDeath = null;
                IsBoss = false;
                SpawnLine = "";
                SpawnLineChance = 1f;
                DeathLine = "";
                DeathLineChance = 1f;
                StartInfected = false;
            }

            public float EquipWeaponChance { get; set; } // 0-1
            public BotAI AIType { get; set; }
            public PlayerModifiers Modifiers { get; set; }
            public bool IsBoss { get; set; }
            public Action<Bot, List<Bot>> OnSpawn { get; set; }
            public int UpdateInterval { get; set; } // in ms
            public Action<Bot> OnUpdate { get; set; }
            public Action<Bot> OnDamage { get; set; }
            public Action<Bot> OnDeath { get; set; }
            public string SpawnLine { get; set; }
            public float SpawnLineChance { get; set; }
            public string DeathLine { get; set; }
            public float DeathLineChance { get; set; }
            public bool StartInfected { get; set; } // Starting as infected by zombie
        }

        private static BotInfo GetInfo(BotType botType)
        {
            var botInfo = new BotInfo();

            switch (botType)
            {
                // Agent
                case BotType.Agent:
                {
                    botInfo.AIType = BotAI.Hard;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 70,
                        CurrentHealth = 70,
                        ProjectileDamageDealtModifier = 0.9f,
                        MeleeDamageDealtModifier = 0.9f,
                        SizeModifier = 0.95f,
                    };
                    break;
                }

                // Assassin
                case BotType.AssassinMelee:
                {
                    botInfo.AIType = BotAI.MeleeHard;
                    botInfo.OnSpawn = (Bot bot, List<Bot> others) =>
                    {
                        var behavior = bot.Player.GetBotBehaviorSet();
                        behavior.OffensiveClimbingLevel = 0.9f;
                        behavior.OffensiveSprintLevel = 0.9f;
                        bot.Player.SetBotBehaviorSet(behavior);
                    };
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 70,
                        CurrentHealth = 70,
                        ProjectileDamageDealtModifier = 0.9f,
                        MeleeDamageDealtModifier = 0.95f,
                        RunSpeedModifier = 1.25f,
                        SprintSpeedModifier = 1.4f,
                        SizeModifier = 0.95f,
                    };
                    break;
                }
                case BotType.AssassinRange:
                {
                    botInfo.AIType = BotAI.RangeHard;
                    botInfo.OnSpawn = (Bot bot, List<Bot> others) =>
                    {
                        var behavior = bot.Player.GetBotBehaviorSet();
                        behavior.OffensiveClimbingLevel = 0.9f;
                        behavior.OffensiveSprintLevel = 0.9f;
                        bot.Player.SetBotBehaviorSet(behavior);
                    };
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 70,
                        CurrentHealth = 70,
                        ProjectileDamageDealtModifier = 0.9f,
                        MeleeDamageDealtModifier = 0.95f,
                        RunSpeedModifier = 1.25f,
                        SprintSpeedModifier = 1.4f,
                        SizeModifier = 0.95f,
                    };
                    break;
                }

                // Boxer
                case BotType.ClownBoxer:
                {
                    botInfo.AIType = BotAI.Hulk;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 110,
                        CurrentHealth = 110,
                        ProjectileDamageDealtModifier = 0.5f,
                        MeleeDamageDealtModifier = 1.1f,
                        MeleeForceModifier = 1.5f,
                        SizeModifier = 1.15f,
                    };
                    break;
                }

                // Cowboy (faster grunt)
                case BotType.ClownCowboy:
                case BotType.Cowboy:
                {
                    botInfo.AIType = BotAI.Grunt;
                    botInfo.EquipWeaponChance = 1f;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 70,
                        CurrentHealth = 70,
                        ProjectileDamageDealtModifier = 1.1f,
                        MeleeDamageDealtModifier = 0.85f,
                        RunSpeedModifier = 1.1f,
                        SprintSpeedModifier = 1.1f,
                        SizeModifier = 0.9f,
                    };
                    botInfo.SpawnLine = "Move 'em on, head 'em up...";
                    botInfo.SpawnLineChance = 0.05f;
                    botInfo.DeathLine = "Count 'em in, ride 'em... oof!";
                    botInfo.DeathLineChance = 0.05f;
                    break;
                }

                // Hulk
                case BotType.BikerHulk:
                case BotType.GangsterHulk:
                case BotType.ThugHulk:
                {
                    botInfo.AIType = BotAI.Hulk;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 150,
                        CurrentHealth = 150,
                        ProjectileDamageDealtModifier = 0.5f,
                        MeleeDamageDealtModifier = 1.1f,
                        MeleeForceModifier = 1.5f,
                        RunSpeedModifier = 0.75f,
                        SprintSpeedModifier = 0.75f,
                        SizeModifier = 1.15f,
                    };
                    break;
                }

                // Grunt
                case BotType.Biker:
                case BotType.NaziScientist:
                case BotType.Thug:
                {
                    botInfo.AIType = BotAI.Grunt;
                    botInfo.EquipWeaponChance = 0.5f;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 70,
                        CurrentHealth = 70,
                        ProjectileDamageDealtModifier = 0.9f,
                        MeleeDamageDealtModifier = 0.95f,
                        SizeModifier = 0.95f,
                    };
                    break;
                }

                // Grunt with weapon
                case BotType.Agent2:
                case BotType.Bandido:
                case BotType.Bodyguard:
                case BotType.Bodyguard2:
                case BotType.ClownBodyguard:
                case BotType.ClownGangster:
                case BotType.Elf:
                case BotType.Gangster:
                case BotType.MetroCop:
                case BotType.Police:
                case BotType.PoliceSWAT:
                {
                    botInfo.AIType = BotAI.Grunt;
                    botInfo.EquipWeaponChance = 1f;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 70,
                        CurrentHealth = 70,
                        ProjectileDamageDealtModifier = 0.9f,
                        MeleeDamageDealtModifier = 0.95f,
                        SizeModifier = 0.95f,
                    };
                    break;
                }

                // Marauder
                case BotType.MarauderBiker:
                case BotType.MarauderCrazy:
                case BotType.MarauderNaked:
                case BotType.MarauderRifleman:
                case BotType.MarauderRobber:
                case BotType.MarauderTough:
                {
                    botInfo.AIType = BotAI.Grunt;
                    botInfo.EquipWeaponChance = 1f;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 1000,
                        CurrentHealth = 70, // Fake blood on the face to make it look like the infected
                        ProjectileDamageDealtModifier = 0.9f,
                        MeleeDamageDealtModifier = 0.95f,
                        SizeModifier = 0.95f,
                    };
                    botInfo.StartInfected = true;
                    break;
                }

                // Sniper
                case BotType.Sniper:
                {
                    botInfo.AIType = BotAI.Sniper;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 60,
                        CurrentHealth = 60,
                        ProjectileDamageDealtModifier = 1.15f,
                        ProjectileCritChanceDealtModifier = 1.15f,
                        MeleeDamageDealtModifier = 0.85f,
                        RunSpeedModifier = 0.8f,
                        SprintSpeedModifier = 0.8f,
                        SizeModifier = 0.95f,
                    };
                    break;
                }

                // Zombie
                case BotType.Zombie:
                case BotType.ZombieAgent:
                case BotType.ZombieGangster:
                case BotType.ZombieNinja:
                case BotType.ZombiePolice:
                case BotType.ZombiePrussian:
                case BotType.ZombieSoldier:
                case BotType.ZombieThug:
                case BotType.ZombieWorker:
                {
                    botInfo.AIType = BotAI.ZombieSlow;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 60,
                        CurrentHealth = 60,
                        MeleeDamageDealtModifier = 0.75f,
                        RunSpeedModifier = 0.75f,
                        SizeModifier = 0.95f,
                    };
                    botInfo.SpawnLine = "Brainzz";
                    botInfo.SpawnLineChance = 0.1f;
                    break;
                }

                // Zombie fast
                case BotType.ZombieChild:
                {
                    botInfo.AIType = BotAI.ZombieFast;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 35,
                        CurrentHealth = 35,
                        MeleeDamageDealtModifier = 0.75f,
                        RunSpeedModifier = 1.15f,
                        SprintSpeedModifier = 1.15f,
                        SizeModifier = 0.85f,
                    };
                    botInfo.SpawnLine = "Brainzz";
                    botInfo.SpawnLineChance = 0.1f;
                    break;
                }

                // Zombie fat
                case BotType.ZombieFat:
                {
                    botInfo.AIType = BotAI.ZombieSlow;
                    botInfo.OnDeath = (Bot bot) => Game.TriggerExplosion(bot.Player.GetWorldPosition());
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 20,
                        CurrentHealth = 20,
                        MeleeDamageDealtModifier = 1.2f,
                        RunSpeedModifier = 0.5f,
                        SprintSpeedModifier = 0.5f,
                        SizeModifier = 1.25f,
                    };
                    break;
                }

                // Zombie flamer
                case BotType.ZombieFlamer:
                {
                    botInfo.AIType = BotAI.ZombieFast;
                    botInfo.OnSpawn = (Bot bot, List<Bot> others) => bot.Player.SetMaxFire();
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 35,
                        CurrentHealth = 35,
                        FireDamageTakenModifier = 0.01f,
                        MeleeDamageDealtModifier = 0.5f,
                        RunSpeedModifier = 1.15f,
                        SprintSpeedModifier = 1.15f,
                        SizeModifier = 0.95f,
                    };
                    break;
                }

                // Zombie hulk
                case BotType.ZombieBruiser:
                {
                    botInfo.AIType = BotAI.ZombieHulk;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 125,
                        CurrentHealth = 125,
                        MeleeDamageDealtModifier = 1.1f,
                        MeleeForceModifier = 1.4f,
                        RunSpeedModifier = 0.75f,
                        SprintSpeedModifier = 0.75f,
                        SizeModifier = 1.2f,
                    };
                    botInfo.SpawnLine = "Brainzz";
                    botInfo.SpawnLineChance = 0.1f;
                    break;
                }

                // --Bosses--
                case BotType.Demolitionist:
                {
                    botInfo.AIType = BotAI.RangeHard;
                    botInfo.OnSpawn = (Bot bot, List<Bot> others) =>
                    {
                        var behavior = bot.Player.GetBotBehaviorSet();
                        behavior.SearchForItems = true;
                        behavior.SearchItems = SearchItems.Primary;
                        bot.Player.SetBotBehaviorSet(behavior);
                    };
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 150,
                        CurrentHealth = 150,
                        ProjectileDamageDealtModifier = 5.0f,
                        ProjectileCritChanceDealtModifier = 5.0f,
                        MeleeDamageDealtModifier = 1.5f,
                        RunSpeedModifier = 0.5f,
                        SprintSpeedModifier = 0.5f,
                        SizeModifier = 0.95f,
                        InfiniteAmmo = 1,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Fritzliebe:
                {
                    botInfo.AIType = BotAI.Expert;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 200,
                        CurrentHealth = 200,
                        SizeModifier = 0.95f,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Funnyman:
                {
                    botInfo.AIType = BotAI.Expert;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 250,
                        CurrentHealth = 250,
                        SizeModifier = 1.05f,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Hacker:
                {
                    botInfo.AIType = BotAI.Hacker;
                    botInfo.UpdateInterval = 200;
                    botInfo.OnUpdate = (Bot bot) =>
                    {
                        if (bot.Player.IsRemoved) return;

                        var profile = bot.Player.GetProfile();
                        var currentColor = profile.Head.Color2;
                        var newColor = "";

                        switch (currentColor)
                        {
                            case "ClothingLightRed":
                                newColor = "ClothingLightOrange";
                                break;
                            case "ClothingLightOrange":
                                newColor = "ClothingLightYellow";
                                break;
                            case "ClothingLightYellow":
                                newColor = "ClothingLightGreen";
                                break;
                            case "ClothingLightGreen":
                                newColor = "ClothingLightCyan";
                                break;
                            case "ClothingLightCyan":
                                newColor = "ClothingLightBlue";
                                break;
                            case "ClothingLightBlue":
                                newColor = "ClothingLightPurple";
                                break;
                            case "ClothingLightPurple":
                                newColor = "ClothingLightRed";
                                break;
                            default:
                                newColor = "ClothingLightCyan";
                                break;
                        }
                        profile.Head.Color2 = newColor;
                        profile.ChestOver.Color2 = newColor;
                        profile.Feet.Color1 = newColor;
                        bot.Player.SetProfile(profile);
                    };
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 125,
                        CurrentHealth = 125,
                        EnergyConsumptionModifier = 0f,
                        MeleeForceModifier = 3f,
                        RunSpeedModifier = 1.1f,
                        SprintSpeedModifier = 1.1f,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Incinerator:
                {
                    botInfo.AIType = BotAI.Hard;
                    botInfo.OnSpawn = (Bot bot, List<Bot> others) =>
                    {
                        var behavior = bot.Player.GetBotBehaviorSet();
                        behavior.SearchForItems = false;
                        behavior.RangedWeaponPrecisionInterpolateTime = 0f;
                        bot.Player.SetBotBehaviorSet(behavior);
                    };
                    botInfo.OnDeath = (Bot bot) =>
                    {
                        var player = bot.Player;
                        var playerPosition = player.GetWorldPosition();

                        if (player.CurrentPrimaryWeapon.WeaponItem == WeaponItem.FLAMETHROWER)
                        {
                            Game.TriggerExplosion(playerPosition);
                            Game.SpawnFireNodes(playerPosition, 20, 5f, FireNodeType.Flamethrower);
                            Game.TriggerFireplosion(playerPosition, 60f);
                        }
                    };
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 250,
                        CurrentHealth = 250,
                        FireDamageTakenModifier = 0.25f,
                        InfiniteAmmo = 1,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Jo:
                {
                    botInfo.AIType = BotAI.MeleeExpert;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 250,
                        CurrentHealth = 250,
                        SizeModifier = 1.1f,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Kingpin:
                {
                    botInfo.AIType = BotAI.Hard;
                    botInfo.OnSpawn = (Bot bot, List<Bot> others) =>
                    {
                        var bodyguards = others.Where(Q => Q.Type == BotType.Bodyguard || Q.Type == BotType.GangsterHulk).Take(2);
                        var bodyguardMaxCount = 2;
                        var bodyguardCount = bodyguards.Count();
                        var bodyguardMissing = bodyguardMaxCount - bodyguardCount;
                        if (bodyguardCount < bodyguardMaxCount)
                            bodyguards.Concat(others.Where(Q => Q.Type == BotType.Bodyguard2).Take(bodyguardMissing));

                        foreach (var bodyguard in bodyguards)
                        {
                            bodyguard.Player.SetGuardTarget(bot.Player);
                        }
                    };
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 250,
                        CurrentHealth = 250,
                        SizeModifier = 1.05f,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Kriegbär:
                {
                    botInfo.AIType = BotAI.Expert;
                    botInfo.OnSpawn = (Bot bot, List<Bot> others) =>
                    {
                        var behavior = bot.Player.GetBotBehaviorSet();
                        behavior.RangedWeaponUsage = false;
                        behavior.SearchForItems = false;
                        behavior.GuardRange = 32;
                        behavior.ChaseRange = 32;
                        bot.Player.SetBotBehaviorSet(behavior);

                        var fritzliebe = others.Find(Q => Q.Type == BotType.Fritzliebe);
                        if (fritzliebe.Player == null) return;

                        bot.Player.SetGuardTarget(fritzliebe.Player);
                    };
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 400,
                        CurrentHealth = 400,
                        MaxEnergy = 400,
                        CurrentEnergy = 400,
                        FireDamageTakenModifier = 1.5f,
                        MeleeForceModifier = 1.75f,
                        RunSpeedModifier = 1.1f,
                        SprintSpeedModifier = 1.1f,
                        SizeModifier = 1.25f,
                    };
                    botInfo.IsBoss = true;
                    botInfo.SpawnLine = "HNNNARRRRRRRHHH!";
                    break;
                }
                case BotType.Meatgrinder:
                {
                    botInfo.AIType = BotAI.Meatgrinder;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 250,
                        CurrentHealth = 250,
                        MaxEnergy = 250,
                        CurrentEnergy = 250,
                        ProjectileDamageDealtModifier = 1.5f,
                        MeleeDamageDealtModifier = 1.5f,
                        MeleeForceModifier = 1.5f,
                        RunSpeedModifier = 1.15f,
                        SprintSpeedModifier = 1.15f,
                        SizeModifier = 1.1f,
                        InfiniteAmmo = 1,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.MetroCop2:
                {
                    botInfo.AIType = BotAI.Expert;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 110,
                        CurrentHealth = 110,
                        RunSpeedModifier = 1.1f,
                        SprintSpeedModifier = 1.1f,
                        SizeModifier = 0.95f,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Ninja:
                {
                    botInfo.AIType = BotAI.Ninja;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 200,
                        CurrentHealth = 200,
                        MeleeDamageDealtModifier = 1.2f,
                        RunSpeedModifier = 1.5f,
                        SprintSpeedModifier = 1.5f,
                        SizeModifier = 0.9f,
                        EnergyRechargeModifier = 0.85f,
                        InfiniteAmmo = 1,
                    };
                    botInfo.IsBoss = true;
                    botInfo.SpawnLine = "Tatakai...";
                    botInfo.DeathLine = "H-h-haji...";
                    break;
                }
                case BotType.Teddybear:
                {
                    botInfo.AIType = BotAI.Hulk;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 500,
                        CurrentHealth = 500,
                        MaxEnergy = 500,
                        CurrentEnergy = 500,
                        MeleeDamageDealtModifier = 1.25f,
                        MeleeForceModifier = 2.0f,
                        RunSpeedModifier = 0.9f,
                        SprintSpeedModifier = 0.9f,
                        SizeModifier = 1.25f,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
                case BotType.Santa:
                {
                    botInfo.AIType = BotAI.Hard; // ChallengeA
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 200,
                        CurrentHealth = 200,
                        ExplosionDamageTakenModifier = 0.5f,
                        MeleeForceModifier = 1.5f,
                        SizeModifier = 1.1f,
                        InfiniteAmmo = 1,
                    };
                    botInfo.IsBoss = true;
                    botInfo.SpawnLine = "Ho ho ho!";
                    botInfo.DeathLine = "Ho ohhhh...";
                    break;
                }
                case BotType.ZombieFighter:
                {
                    botInfo.AIType = BotAI.ZombieFighter;
                    botInfo.Modifiers = new PlayerModifiers()
                    {
                        MaxHealth = 200,
                        CurrentHealth = 200,
                        MeleeDamageDealtModifier = 1.05f,
                        RunSpeedModifier = 0.95f,
                        SprintSpeedModifier = 0.95f,
                        SizeModifier = 1.1f,
                    };
                    botInfo.IsBoss = true;
                    break;
                }
            }

            return botInfo;
        }

        #endregion

        #region Bot group

        public class SubGroup
        {
            public SubGroup() { }
            public SubGroup(List<BotType> types, float weight)
            {
                Types = types;
                Weight = weight;
            }
            public SubGroup(BotType type, float weight)
            {
                Types = new List<BotType>() { type };
                Weight = weight;
            }
            public SubGroup(BotType type)
            {
                Types = new List<BotType>() { type };
                Weight = 0f;
            }
            private List<BotType> types;
            public List<BotType> Types
            {
                get { return types; }
                set
                {
                    types = value;
                    HasBoss = GetInfo(types.First()).IsBoss;
                }
            }
            public float Weight { get; set; }
            public bool HasBoss { get; private set; }
            public BotType GetRandomType()
            {
                return SharpHelper.GetRandomItem(Types);
            }
        }

        public class Group
        {
            public List<SubGroup> SubGroups { get; private set; }
            public float TotalScore { get; private set; }
            public bool HasBoss { get; private set; }

            public Group(List<SubGroup> subGroups)
            {
                SubGroups = subGroups;
                HasBoss = false;

                foreach (var subGroup in subGroups)
                {
                    var hasBoss = subGroup.HasBoss;
                    if (hasBoss)
                    {
                        HasBoss = true;
                        continue;
                    }

                    TotalScore += subGroup.Weight;
                }
            }

            public void Spawn(int groupCount)
            {
                var subGroupCount = 0;
                var groupCountRemaining = groupCount;
                var mobCount = HasBoss ? groupCount - 1 : groupCount;
                var newBots = new List<Bot>();

                foreach (var subGroup in SubGroups)
                {
                    subGroupCount++;

                    if (!subGroup.HasBoss)
                    {
                        var weight = subGroup.Weight;
                        var share = weight / TotalScore;
                        var botCountRemainingThisType = Math.Round(mobCount * share);

                        while (groupCountRemaining > 0 && (botCountRemainingThisType > 0 || subGroupCount == SubGroups.Count))
                        {
                            var botType = subGroup.GetRandomType();
                            var bot = BotHelper.SpawnBot(botType);

                            newBots.Add(bot);
                            groupCountRemaining--;
                            botCountRemainingThisType--;
                        }
                    }
                    else
                    {
                        var botType = subGroup.GetRandomType();
                        var bot = BotHelper.SpawnBot(botType);

                        newBots.Add(bot);
                        groupCountRemaining--;
                    }
                }

                foreach (var bot in newBots.ToList())
                {
                    bot.OnSpawn(newBots);
                }
            }
        }

        public class GroupSet
        {
            public List<Group> Groups { get; set; }

            public GroupSet()
            {
                Groups = new List<Group>();
            }
            public bool HasBoss
            {
                get { return Groups.Where(g => g.HasBoss).Any(); }
            }

            public void AddGroup(List<SubGroup> subGroups)
            {
                Groups.Add(new Group(subGroups));
            }
        }

        private static List<BotType> CommonZombieTypes = new List<BotType>()
        {
            BotType.Zombie,
            BotType.ZombieAgent,
            BotType.ZombieFlamer,
            BotType.ZombieGangster,
            BotType.ZombieNinja,
            BotType.ZombiePolice,
            BotType.ZombieSoldier,
            BotType.ZombieThug,
            BotType.ZombieWorker,
        };
        private static List<BotType> MutatedZombieTypes = new List<BotType>()
        {
            BotType.ZombieBruiser,
            BotType.ZombieChild,
            BotType.ZombieFat,
            BotType.ZombieFlamer,
        };

        private const int BOSS_GROUP_START_INDEX = 200;
        public enum BotGroup
        {
            Assassin = 0,
            Agent,
            Bandido,
            Biker,
            Clown,
            Cowboy,
            Gangster,
            Marauder,
            MetroCop,
            Police,
            PoliceSWAT,
            Sniper,
            Thug,
            Zombie,
            ZombieHard,

            Boss_Demolitionist = BOSS_GROUP_START_INDEX,
            Boss_Funnyman,
            Boss_Jo,
            Boss_Hacker,
            Boss_Incinerator,
            Boss_Kingpin,
            Boss_MadScientist,
            Boss_Meatgrinder,
            Boss_MetroCop,
            Boss_Ninja,
            Boss_Santa,
            Boss_Teddybear,
            Boss_Zombie,
        }

        private static GroupSet GetGroupSet(BotGroup botGroup)
        {
            var groupSet = new GroupSet();

            switch (botGroup)
            {
                case BotGroup.Assassin:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.AssassinMelee, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.AssassinRange, 1f),
                    });
                    break;
                }
                case BotGroup.Agent:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Agent, 1f),
                    });
                    break;
                }
                case BotGroup.Bandido:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Bandido, 1f),
                    });
                    break;
                }
                case BotGroup.Biker:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Biker, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Biker, 0.5f),
                        new SubGroup(BotType.Thug, 0.5f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Biker, 0.6f),
                        new SubGroup(BotType.BikerHulk, 0.4f),
                    });
                    break;
                }
                case BotGroup.Clown:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ClownCowboy, 0.5f),
                        new SubGroup(BotType.ClownGangster, 0.25f),
                        new SubGroup(BotType.ClownBoxer, 0.25f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ClownCowboy, 0.6f),
                        new SubGroup(BotType.ClownGangster, 0.4f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ClownBoxer, 0.7f),
                        new SubGroup(BotType.ClownGangster, 0.3f),
                    });
                    break;
                }
                case BotGroup.Cowboy:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Cowboy, 1f),
                    });
                    break;
                }
                case BotGroup.Gangster:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Gangster, 0.8f),
                        new SubGroup(BotType.GangsterHulk, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Gangster, 0.7f),
                        new SubGroup(BotType.ThugHulk, 0.3f),
                    });
                    break;
                }
                case BotGroup.Marauder:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(new List<BotType>()
                        {
                            BotType.MarauderBiker,
                            BotType.MarauderCrazy,
                            BotType.MarauderNaked,
                            BotType.MarauderRifleman,
                            BotType.MarauderRobber,
                            BotType.MarauderTough,
                        }, 1f),
                    });
                    break;
                }
                case BotGroup.MetroCop:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop, 0.7f),
                        new SubGroup(BotType.Agent2, 0.3f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop, 0.5f),
                        new SubGroup(BotType.Agent2, 0.5f),
                    });
                    break;
                }
                case BotGroup.Police:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Police, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Police, 0.7f),
                        new SubGroup(BotType.PoliceSWAT, 0.3f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.PoliceSWAT, 0.8f),
                        new SubGroup(BotType.Police, 0.2f),
                    });
                    break;
                }
                case BotGroup.PoliceSWAT:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.PoliceSWAT, 1f),
                    });
                    break;
                }
                case BotGroup.Sniper:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Sniper, 1f),
                    });
                    break;
                }
                case BotGroup.Thug:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Thug, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Thug, 0.5f),
                        new SubGroup(BotType.Biker, 0.5f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Thug, 0.6f),
                        new SubGroup(BotType.ThugHulk, 0.4f),
                    });
                    break;
                }
                case BotGroup.Zombie:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Zombie, 0.4f),
                        new SubGroup(CommonZombieTypes, 0.6f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.8f),
                        new SubGroup(BotType.ZombieBruiser, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.6f),
                        new SubGroup(BotType.ZombieBruiser, 0.4f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.8f),
                        new SubGroup(BotType.ZombieChild, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.6f),
                        new SubGroup(BotType.ZombieChild, 0.4f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.8f),
                        new SubGroup(BotType.ZombieFat, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.6f),
                        new SubGroup(BotType.ZombieFat, 0.4f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.8f),
                        new SubGroup(BotType.ZombieFlamer, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.6f),
                        new SubGroup(BotType.ZombieFlamer, 0.4f),
                    });
                    break;
                }
                case BotGroup.ZombieHard:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(MutatedZombieTypes, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.2f),
                        new SubGroup(MutatedZombieTypes, 0.8f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.4f),
                        new SubGroup(MutatedZombieTypes, 0.6f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.7f),
                        new SubGroup(MutatedZombieTypes, 0.3f),
                    });
                    break;
                }

                case BotGroup.Boss_Demolitionist:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Demolitionist),
                    });
                    break;
                }
                case BotGroup.Boss_Funnyman:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Funnyman),
                        new SubGroup(BotType.ClownBodyguard, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Funnyman),
                        new SubGroup(new List<BotType>()
                        {
                            BotType.ClownBoxer,
                            BotType.ClownCowboy,
                            BotType.ClownGangster,
                        }, 1f),
                    });
                    break;
                }
                case BotGroup.Boss_Jo:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Jo),
                        new SubGroup(BotType.Biker, 0.6f),
                        new SubGroup(BotType.BikerHulk, 0.4f),
                    });
                    break;
                }
                case BotGroup.Boss_Hacker:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Hacker),
                        new SubGroup(BotType.Hacker),
                    });
                    break;
                }
                case BotGroup.Boss_Incinerator:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Incinerator),
                    });
                    break;
                }
                case BotGroup.Boss_Kingpin:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Kingpin),
                        new SubGroup(BotType.Bodyguard, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Kingpin),
                        new SubGroup(BotType.GangsterHulk, 0.55f),
                        new SubGroup(BotType.Bodyguard2, 0.45f),
                    });
                    break;
                }
                case BotGroup.Boss_MadScientist:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Kriegbär),
                        new SubGroup(BotType.Fritzliebe),
                    });
                    break;
                }
                case BotGroup.Boss_Meatgrinder:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Meatgrinder),
                    });
                    break;
                }
                case BotGroup.Boss_MetroCop:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop2),
                        new SubGroup(BotType.MetroCop, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop2),
                        new SubGroup(BotType.MetroCop, 0.7f),
                        new SubGroup(BotType.Agent2, 0.3f),
                    });
                    break;
                }
                case BotGroup.Boss_Ninja:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Ninja),
                    });
                    break;
                }
                case BotGroup.Boss_Santa:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Santa),
                        new SubGroup(BotType.Elf, 1f),
                    });
                    break;
                }
                case BotGroup.Boss_Teddybear:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Teddybear),
                    });
                    break;
                }
                case BotGroup.Boss_Zombie:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ZombieFighter),
                        new SubGroup(CommonZombieTypes, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ZombieFighter),
                        new SubGroup(CommonZombieTypes, 0.7f),
                        new SubGroup(MutatedZombieTypes, 0.3f),
                    });
                    break;
                }
            }

            return groupSet;
        }

        #endregion

        public class PlayerSpawner
        {
            public Vector2 Position { get; set; }
            public bool HasSpawned { get; set; }
        }

        public class Bot
        {
            private static Color dialogueColor = new Color(128, 32, 32);
            public IPlayer Player { get; private set; }
            public BotType Type { get; private set; }
            public BotInfo Info { get; private set; }

            public Bot()
            {
                Player = null;
                Type = BotType.None;
                Info = null;
            }

            public Bot(IPlayer player, BotType type, BotInfo info)
            {
                Player = player;
                Type = type;
                Info = info;

                SaySpawnLine();
            }

            public void SaySpawnLine()
            {
                var spawnLine = Info.SpawnLine;
                var spawnLineChance = Info.SpawnLineChance;

                if (!string.IsNullOrWhiteSpace(spawnLine) && SharpHelper.RandomBetween(0f, 1f) < spawnLineChance)
                    Game.CreateDialogue(spawnLine, dialogueColor, Player, duration: 3000f);
            }

            public void SayDeathLine()
            {
                var deathLine = Info.DeathLine;
                var deathLineChance = Info.DeathLineChance;

                if (!string.IsNullOrWhiteSpace(deathLine) && SharpHelper.RandomBetween(0f, 1f) < deathLineChance)
                    Game.CreateDialogue(deathLine, dialogueColor, Player, duration: 3000f);
            }

            private int lastUpdateElapsed = 0;
            public void OnUpdate(float elapsed)
            {
                lastUpdateElapsed += (int)elapsed;

                if (lastUpdateElapsed >= Info.UpdateInterval && Info.OnUpdate != null)
                {
                    Info.OnUpdate(this);
                    lastUpdateElapsed = 0;
                }
            }

            // Dont call OnSpawn in constructor. Wait until the bot list is initialized fully
            public void OnSpawn(List<Bot> bots)
            {
                if (Info.OnSpawn != null)
                    Info.OnSpawn(this, bots);
            }

            public void OnDamage()
            {
                if (Info.OnDamage != null)
                    Info.OnDamage(this);
            }

            public void OnDeath()
            {
                if (Info.OnDeath != null)
                    Info.OnDeath(this);
            }
        }

        public static class BotExendedCommand
        {
            private static IScriptStorage m_storage = Game.LocalStorage;
            //private static IScriptStorage m_storage = Game.GetSharedStorage("BOTEXTENDED");

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

                        case "sp":
                        case "spawn":
                            SpawnNewBot(arguments);
                            break;

                        case "r":
                        case "random":
                            RandomGroup(arguments);
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
                ScriptHelper.PrintMessage("/<botextended|be> [listgroup|lg]: List all bot groups");
                ScriptHelper.PrintMessage("/<botextended|be> [listbot|lb]: List all bot types");
                ScriptHelper.PrintMessage("/<botextended|be> [find|f|/] <query>: Find bot groups");
                ScriptHelper.PrintMessage("/<botextended|be> [settings|s]: Show current script settings");
                ScriptHelper.PrintMessage("/<botextended|be> [spawn|sp] <BotType> [Team|_] [Count]: Spawn bot");
                ScriptHelper.PrintMessage("/<botextended|be> [random|r] <0|1>: Random all groups at startup if set to 1. This option will disregard the current group list");
                ScriptHelper.PrintMessage("/<botextended|be> [group|g] <group names|indexes>: Choose a list of group by either name or index to randomly spawn on startup");
                ScriptHelper.PrintMessage("/<botextended|be> [stats|st]: List all bot types and bot groups stats");
                ScriptHelper.PrintMessage("/<botextended|be> [clearstats|cst]: Clear all bot types and bot groups stats");
                ScriptHelper.PrintMessage("For example:", ScriptHelper.ERROR_COLOR);
                ScriptHelper.PrintMessage("/botextended select metrocop >> select metrocop group");
                ScriptHelper.PrintMessage("/be s 0 2 4 >> select assassin, bandido and clown group");
            }

            private static IEnumerable<string> GetGroupNames()
            {
                var groups = SharpHelper.GetArrayFromEnum<BotGroup>();

                foreach(var group in groups)
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
                if (m_storage.TryGetItemBool(BotHelper.RANDOM_GROUP, out randomGroup))
                {
                    ScriptHelper.PrintMessage("-Random groups: " + randomGroup, ScriptHelper.WARNING_COLOR);
                }
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

            private static void RandomGroup(IEnumerable<string> arguments)
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
        }

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

            public static readonly string STORAGE_PREFIX = "BOT_EXTENDED_NH";
            public static readonly string BOT_GROUPS = STORAGE_PREFIX + "BOT_EXTENDED_NH_BOT_GROUPS";
            public static readonly string RANDOM_GROUP = STORAGE_PREFIX + "BOT_EXTENDED_NH_RANDOM_GROUP";
            public static readonly string VERSION = STORAGE_PREFIX + "BOT_EXTENDED_NH_VERSION";
            public static Func<BotType, string> GET_BOTTYPE_STORAGE_KEY = (botType) => "BOT_EXTENDED_NH_"
                + SharpHelper.EnumToString(botType).ToUpperInvariant();
            public static Func<BotGroup, int, string> GET_GROUP_STORAGE_KEY = (botGroup, groupIndex) => "BOT_EXTENDED_NH_"
                + SharpHelper.EnumToString(botGroup).ToUpperInvariant()
                + "_" + groupIndex;

            private static bool RANDOM_GROUP_DEFAULT_VALUE = true;
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
            private static List<Bot> m_updateBots = new List<Bot>();

            public static void Initialize()
            {
                m_playerSpawners = GetEmptyPlayerSpawners();
                m_playerDamageEvent = Events.PlayerDamageCallback.Start(OnPlayerDamage);
                m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);
                m_updateEvent = Events.UpdateCallback.Start(OnUpdate);
                m_userMessageEvent = Events.UserMessageCallback.Start(BotExendedCommand.OnUserMessage);

                bool randomGroup;
                if (!Game.LocalStorage.TryGetItemBool(RANDOM_GROUP, out randomGroup))
                {
                    randomGroup = RANDOM_GROUP_DEFAULT_VALUE;
                }

                var playerCount = Game.GetPlayers().Length;
                var botCount = MAX_PLAYERS - playerCount;
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
                    //SpawnGroup(BotGroup.Assassin, botSpawnCount);
                    //SpawnBot(BotType.MarauderBiker);
                }
            }

            private static void SpawnRandomGroup(int botCount, List<BotGroup> botGroups)
            {
                List<BotGroup> filteredBotGroups = null;
                if (botCount < 3) // Too few for a group, spawn boss instead
                {
                    filteredBotGroups = botGroups.Select(g => g).Where(g => (int)g >= BOSS_GROUP_START_INDEX).ToList();
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

                foreach (var bot in m_updateBots) bot.OnUpdate(elapsed);

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
                Game.PlayEffect(ScriptHelper.EFFECT_TR_D, player.GetWorldPosition()); // TODO: remove

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

                if (IsHitByZombieOrTheInfected(player))
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

                    if (zombie.IsMeleeAttacking || zombie.IsJumpAttacking || zombie.IsJumpKicking || zombie.IsKicking)
                    {
                        if (!target.IsRemoved
                        && (target.GetTeam() != zombie.GetTeam()) // Zombie is always in the same team so no need to check if Independent
                        && target.IsInputEnabled
                        && 25 >= Math.Abs(zombie.GetWorldPosition().X - target.GetWorldPosition().X)
                        && 25 >= Math.Abs(zombie.GetWorldPosition().Y - target.GetWorldPosition().Y))
                        {
                            return true;
                        }
                    }
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

            private static IPlayer SpawnPlayer(BotInfo botInfo = null, WeaponSet weaponSet = null, bool ignoreFullSpawner = false)
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

                if (weaponSet != null)
                {
                    player.GiveWeaponItem(weaponSet.Melee);
                    player.GiveWeaponItem(weaponSet.Primary);
                    player.GiveWeaponItem(weaponSet.Secondary);
                    player.GiveWeaponItem(weaponSet.Throwable);
                    player.GiveWeaponItem(weaponSet.Powerup);

                    if (weaponSet.UseLazer)
                        player.GiveWeaponItem(WeaponItem.LAZER);
                }

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

                if (equipWeapons)
                {
                    if (SharpHelper.RandomBetween(0f, 1f) < info.EquipWeaponChance)
                        weaponSet = SharpHelper.GetRandomItem(GetWeapons(botType));
                }

                if (player == null) player = SpawnPlayer(info, weaponSet, ignoreFullSpawner);
                if (player == null) return null;
                if (string.IsNullOrEmpty(player.CustomID))
                    player.CustomID = Guid.NewGuid().ToString("N");

                if (setProfile)
                {
                    var profile = SharpHelper.GetRandomItem(GetProfiles(botType));
                    player.SetProfile(profile);
                    player.SetBotName(profile.Name);
                }

                player.SetModifiers(info.Modifiers);
                player.SetBotBehaviorSet(GetBehaviorSet(info.AIType));
                player.SetBotBehaviorActive(true);
                player.SetTeam(team);

                if (info.StartInfected)
                    m_infectedPlayers.Add(player.CustomID, player);

                var bot = new Bot(player, botType, info);
                m_bots.Add(player.CustomID, bot);

                if (info.OnUpdate != null)
                    m_updateBots.Add(bot);

                return bot;
            }
        }
    }
}

// TODO:
// Add draw weapon first

// fritzliebe
// Kriegbär
// mecha

// Commands:
// botextended groupInterval

// Fix a bug where the group is registered as win if you exit the server instead of continue map

// Group
// mecha/fritzliebe -> play electric effect when low on health
// Add bulletproof and meleeproof superfighters
// Multiple spawn|dead lines?
