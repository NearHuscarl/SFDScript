using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScript.RandBuff
{

    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        private const int MAX_PLAYERS = 12;

        public static Random Rnd = new Random();

        public void OnStartup()
        {
            InitializeBots();
        }

        private void InitializeBots()
        {
            var profiles = new List<IProfile>();
            var obj = (IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("Bruiser" );
            //System.Diagnostics.Debugger.Break();
            BotHelper.Initialize();
        }

        public void OnBotSpawn(CreateTriggerArgs args)
        {
            IObjectPlayerSpawnTrigger trigger = (IObjectPlayerSpawnTrigger)args.Caller;
            IPlayer player = (IPlayer)args.CreatedObject;

            player.CustomID = trigger.CustomID;
        }

        #region Helper class

        public static class Helper
        {
            public static T GetRandomItem<T>(List<T> list)
            {
                var rndIndex = Rnd.Next(list.Count);
                return list[rndIndex];
            }

            public static bool SpawnPlayerHasPlayer(IObject spawnPlayer)
            {
                // Player position y: -8 -> -10
                // => -6 -> -12
                // Player position x: unchange
                foreach (var player in Game.GetPlayers())
                {
                    var playerPosition = player.GetWorldPosition();
                    var spawnPlayerPosition = spawnPlayer.GetWorldPosition();

                    if (spawnPlayerPosition.Y - 12 <= playerPosition.Y && playerPosition.Y <= spawnPlayerPosition.Y - 6
                        && spawnPlayerPosition.X == playerPosition.X)
                        return true;
                }

                return false;
            }
        }

        #endregion

        #region Bot type/group

        public enum BotType
        {
            Zombie,
            Mutant,
            Sniper,
            Agent,
            Soldier,
            Bodyguard,
            Kingpin,
            Demolitionist,
            Meatgrinder,
            Santa,
            Elf,
            Police,
            PoliceSWAT,
            MetroCop,
            Bandido,
            Cowboy,
            Gangster,
            GangsterHulk,
            Thug,
            ThugHulk,
            Assassin,
            Biker,
        }

        enum BotGroup
        {
            Thug,
            Clown,
            Zombie,
            Biker,
        }
        
        #endregion

        #region Bot profiles

        private static List<IProfile> ThugProfiles = new List<IProfile>()
        {
            new IProfile()
            {
                Name = "Thug1_Female",
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
            },
            new IProfile()
            {
                Name = "Thug1_Male",
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
            },
            new IProfile()
            {
                Name = "Thug2_Male",
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
            },
            new IProfile()
            {
                Name = "Thug2_Female",
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
            },
            new IProfile()
            {
                Name = "Thug3_Male",
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
            },
            new IProfile()
            {
                Name = "Thug3_Female",
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
            },
            new IProfile()
            {
                Name = "Thug3_Male_2",
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
            },
            new IProfile()
            {
                Name = "Thug4_Female",
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
            },
            new IProfile()
            {
                Name = "Thug5_Female",
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
            },
            new IProfile()
            {
                Name = "Thug6_Female",
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
            },
            new IProfile()
            {
                Name = "Thug6_Male",
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
            },
            new IProfile()
            {
                Name = "Thug7_Male",
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
            },
            new IProfile()
            {
                Name = "Thug7_Female",
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
            },
        };

        #endregion

        #region Bot infos

        public class BotInfo
        {
            public string Id { get; set; }
            public int Weight { get; set; }
            public List<WeaponItem> Weapons { get; set; }
            public float EquipWeaponChance { get; set; } // 0-1
            public PredefinedAIType AIType { get; set; }
            public PlayerModifiers Modifiers { get; set; }
        }

        // MaxHealth: 1-9999
        // EnergyConsumptionModifier: 0-100
        // ProjectileDamageDealtModifier: 0-100
        // MeleeDamageDealtModifier: 0-100
        // RunSpeedModifier: 0.5-2.0
        // SizeModifier: 0.75-1.25

        #region Thug info

        private static List<WeaponItem> ThugWeapons = new List<WeaponItem>()
        {
            WeaponItem.PIPE,
            WeaponItem.BAT,
            WeaponItem.BATON,
            WeaponItem.KNIFE,
            WeaponItem.PISTOL,
            WeaponItem.REVOLVER,
            WeaponItem.UZI,
            WeaponItem.MACHINE_PISTOL,
            WeaponItem.HAMMER,
            WeaponItem.FLAREGUN,
            WeaponItem.MOLOTOVS,
        };

        private static List<BotInfo> ThugInfo = new List<BotInfo>()
        {
            new BotInfo()
            {
                Id = "Thug",
                Weight = 35,
                Weapons = ThugWeapons,
                EquipWeaponChance = 0.5f,
                AIType = PredefinedAIType.Grunt,
                Modifiers = new PlayerModifiers()
                {
                    MaxHealth = 22,
                    EnergyConsumptionModifier = 0.0f,
                    ProjectileDamageDealtModifier = 0.25f,
                    MeleeDamageDealtModifier = 0.25f,
                    RunSpeedModifier = 0.75f,
                    SizeModifier = 0.95f,
                },
            },
            new BotInfo()
            {
                Id = "Thug2",
                Weight = 60,
                Weapons = ThugWeapons,
                EquipWeaponChance = 0.5f,
                AIType = PredefinedAIType.Grunt,
                Modifiers = new PlayerModifiers()
                {
                    MaxHealth = 44,
                    EnergyConsumptionModifier = 0.0f,
                    ProjectileDamageDealtModifier = 0.4f,
                    MeleeDamageDealtModifier = 0.4f,
                    RunSpeedModifier = 0.75f,
                    SizeModifier = 1.05f,
                },
            }
        };

        #endregion

        #endregion

        public static Dictionary<BotType, List<IProfile>> BotProfiles = new Dictionary<BotType, List<IProfile>>()
        {
            { BotType.Thug, ThugProfiles }
        };

        public static Dictionary<BotType, List<BotInfo>> BotInfos = new Dictionary<BotType, List<BotInfo>>()
        {
            { BotType.Thug, ThugInfo }
        };

        public class PlayerSpawner
        {
            public IObjectPlayerSpawnTrigger Trigger { get; set; }
            public bool HasSpawned { get; set; }
        }

        public class Bot
        {
            public IPlayer Player { get; set; }
            public BotType Type { get; set; }
            public string Id { get; set; } // == BotModifierInfo.Id
            public BotInfo Info
            {
                get { return BotInfos[Type].Where(Q => Q.Id == Id).Select(Q => Q).FirstOrDefault(); }
            }
        }

        public static class BotHelper
        {
            private static List<PlayerSpawner> playerSpawners;

            public static void Initialize()
            {
                playerSpawners = GetEmptyPlayerSpawners();

                var playerCount = Game.GetPlayers().Length;
                var botCount = MAX_PLAYERS - playerCount;
                var botSpawnCount = Math.Min(botCount, playerSpawners.Count);

                for (var i = 0; i < botSpawnCount; i++)
                {
                    Create(BotType.Thug);
                }
            }

            private static List<PlayerSpawner> GetEmptyPlayerSpawners()
            {
                var spawnPlayers = Game.GetObjectsByName("SpawnPlayer");
                var emptyPlayerSpawners = new List<PlayerSpawner>();

                foreach (var spawnPlayer in spawnPlayers)
                {
                    if (!Helper.SpawnPlayerHasPlayer(spawnPlayer))
                    {
                        var playerSpawnTrigger = (IObjectPlayerSpawnTrigger)Game.CreateObject("PlayerSpawnTrigger");

                        playerSpawnTrigger.SetWorldPosition(spawnPlayer.GetWorldPosition());
                        playerSpawnTrigger.SetScriptMethod("OnBotSpawn");
                        playerSpawnTrigger.SetActivateOnStartup(false); // Default to false, just to make it explicit

                        emptyPlayerSpawners.Add(new PlayerSpawner
                        {
                            Trigger = playerSpawnTrigger,
                            HasSpawned = false,
                        });
                    }
                }

                return emptyPlayerSpawners;
            }

            public static Bot Create(BotType botType)
            {
                switch (botType)
                {
                    case BotType.Thug:
                        return SpawnBot(botType);

                    default:
                        return SpawnBot(BotType.Thug);
                }
            }

            private static IPlayer SpawnPlayer()
            {
                var emptySpawners = playerSpawners
                    .Select((playerSpawner, index) => new { playerSpawner, index })
                    .Where(Q => Q.playerSpawner.HasSpawned == false)
                    .ToList();

                if (!emptySpawners.Any())
                    return null;

                var rndSpawner = Helper.GetRandomItem(emptySpawners);
                var spawnTrigger = rndSpawner.playerSpawner.Trigger;

                spawnTrigger.Trigger();
                playerSpawners[rndSpawner.index].HasSpawned = true;

                return spawnTrigger.GetLastCreatedPlayer();
            }

            private static Bot SpawnBot(BotType botType)
            {
                var player = SpawnPlayer();

                if (player == null) return null;

                var profile = Helper.GetRandomItem(BotProfiles[botType]);
                var info = Helper.GetRandomItem(BotInfos[botType]);

                player.SetProfile(profile);
                player.SetModifiers(info.Modifiers);
                player.SetBotBehavior(new BotBehavior()
                {
                    Active = true,
                    PredefinedAI = info.AIType,
                });
                player.SetTeam(PlayerTeam.Team4);

                return new Bot()
                {
                    Player = player,
                    Type = botType,
                    Id = info.Id,
                };
            }
        }
    }
}
