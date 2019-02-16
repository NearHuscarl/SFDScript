using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScript.MoreBot
{

    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

/*
* author: NearHuscarl
* description: Spawn a variety of bots from the campaign and challenge maps to make thing more chaotic
* mapmodes: versus
*/

        private const int MAX_PLAYERS = 12;

        private static Random Rnd = new Random();
        private const string TIER_1 = "TIER_1";
        private const string TIER_2 = "TIER_2";
        private const string TIER_3 = "TIER_3";

        public void OnStartup()
        {
            //System.Diagnostics.Debugger.Break();

            if (Game.IsEditorTest)
            {
                var player = Game.GetPlayers()[0];
                var modifiers = player.GetModifiers();

                modifiers.MaxHealth = 5000;
                modifiers.CurrentHealth = 5000;
                modifiers.InfiniteAmmo = 1;

                player.SetModifiers(modifiers);
                player.GiveWeaponItem(WeaponItem.PIPE);
                player.GiveWeaponItem(WeaponItem.REVOLVER);
                player.GiveWeaponItem(WeaponItem.SUB_MACHINEGUN);
                player.GiveWeaponItem(WeaponItem.MOLOTOVS);
            }

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
                // Player position y: -20 || +5
                // => -21 -> +6
                // Player position x: unchange
                foreach (var player in Game.GetPlayers())
                {
                    var playerPosition = player.GetWorldPosition();
                    var spawnPlayerPosition = spawnPlayer.GetWorldPosition();

                    if (spawnPlayerPosition.Y - 21 <= playerPosition.Y && playerPosition.Y <= spawnPlayerPosition.Y + 6
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
            // Tier1: Rooftop Retribution
            // Tier2: Canals Carnage
            AssassinMelee,
            AssassinRange,
            // Tier1: Subway Shakedown
            Agent,
            // Tier2: Piston Posse, Tower Trouble
            Agent2, // pair with metro cop
            // Tier1: High Moon Holdout
            Bandido,
            // Tier1: Police Station Punchout, Warehouse Wreckage
            // Tier2: Bar Brawl
            // Tier3: Meatgrinder Begins
            Biker,
            BikerHulk,
            // Tier1: The Teahouse Job, Rooftop Retribution
            Bodyguard,
            Bodyguard2,

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
            Fritzliebe,
            Funnyman,
            // Tier3: Holiday Hullabaloo
            Elf,
            // Tier1: The Teahouse Job, Rooftop Retribution
            // Tier3: Unearthed
            Kingpin,
            Kriegbär,
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

            // Tier1: Mall Mayhem
            // Tier3: Police Station Escape!
            Police,
            PoliceSWAT,

            // Tier1: Heavy Hostility
            // Tier2: Trainyard Takedown, Alley Bombardment
            // Tier3: Meatgrinder Begins
            Gangster,
            // Tier1: The Teahouse Job, Heavy Hostility
            GangsterHulk,

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

        private static List<IProfile> AgentProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> AssasinMeleeProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> AssasinRangeProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> BandidoProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> BikerProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> BodyguardProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ClownBodyguardProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ClownBoxerProfiles = new List<IProfile>()
        {
            new IProfile()
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
                },
        };
        private static List<IProfile> ClownCowboyProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ClownGangsterProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> CowboyProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> DemolitionistProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ElfProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> FritzliebeProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> FunnymanProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> GangsterProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> GangsterHulkProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> KingpinProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> KriegbärProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> MetroCopProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> MetroCop2Profiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> MeatgrinderProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> MechaProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> MutantProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> NaziLabAssistantProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> NaziMuscleSoldierProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> NaziScientistProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> NaziSoldierProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> SSOfficerProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> PoliceProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> PoliceSWATProfiles = new List<IProfile>()
        {
            new IProfile()
            {
                Name = "SWAT Officer",
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
            },
            new IProfile()
            {
                Name = "SWAT Officer",
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
            },
        };
        private static List<IProfile> SantaProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> SniperProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> SoldierProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> TeddybearProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ThugProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ThugHulkProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieAgentProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieBruiserProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieChildProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieFlamerProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieGangsterProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieNinjaProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombiePoliceProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombiePrussianProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> BaronVonHauptsteinProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieSoldierProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieThugProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
            new IProfile()
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
            },
        };
        private static List<IProfile> ZombieWorkerProfiles = new List<IProfile>()
        {
            new IProfile()
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
            },
        };


        public static Dictionary<BotType, List<IProfile>> BotProfiles = new Dictionary<BotType, List<IProfile>>()
        {
            { BotType.Agent, AgentProfiles },
            { BotType.Agent2, AgentProfiles },
            { BotType.AssassinMelee, AssasinMeleeProfiles },
            { BotType.AssassinRange, AssasinRangeProfiles },
            { BotType.Bandido, BandidoProfiles },
            { BotType.Biker, BikerProfiles },
            { BotType.BikerHulk, BikerProfiles },
            { BotType.Bodyguard, BodyguardProfiles },
            { BotType.Bodyguard2, BodyguardProfiles },
            { BotType.ClownBodyguard, ClownBodyguardProfiles },
            { BotType.ClownBoxer, ClownBoxerProfiles },
            { BotType.ClownCowboy, ClownCowboyProfiles },
            { BotType.ClownGangster, ClownGangsterProfiles },
            { BotType.Cowboy, CowboyProfiles },
            { BotType.Demolitionist, DemolitionistProfiles },
            { BotType.Elf, ElfProfiles },
            { BotType.Fritzliebe, FritzliebeProfiles },
            { BotType.Funnyman, FunnymanProfiles },
            { BotType.Gangster, GangsterProfiles },
            { BotType.GangsterHulk, GangsterHulkProfiles },
            { BotType.Kingpin, KingpinProfiles },
            { BotType.Kriegbär, KriegbärProfiles },
            { BotType.Meatgrinder, MeatgrinderProfiles },
            { BotType.Mecha, MechaProfiles },
            { BotType.MetroCop, MetroCopProfiles },
            { BotType.MetroCop2, MetroCop2Profiles },
            { BotType.Mutant, MutantProfiles },
            { BotType.NaziLabAssistant, NaziLabAssistantProfiles },
            { BotType.NaziMuscleSoldier, NaziMuscleSoldierProfiles },
            { BotType.NaziScientist, NaziScientistProfiles },
            { BotType.NaziSoldier, NaziSoldierProfiles },
            { BotType.SSOfficer, SSOfficerProfiles },
            { BotType.Police, PoliceProfiles },
            { BotType.PoliceSWAT, PoliceSWATProfiles },
            { BotType.Santa, SantaProfiles },
            { BotType.Sniper, SniperProfiles },
            { BotType.Soldier, SoldierProfiles },
            { BotType.Soldier2, SoldierProfiles },
            { BotType.Teddybear, TeddybearProfiles },
            { BotType.Thug, ThugProfiles },
            { BotType.ThugHulk, ThugHulkProfiles },
            { BotType.Zombie, ZombieProfiles },
            { BotType.ZombieAgent, ZombieAgentProfiles },
            { BotType.ZombieBruiser, ZombieBruiserProfiles },
            { BotType.ZombieChild, ZombieChildProfiles },
            { BotType.ZombieFlamer, ZombieFlamerProfiles },
            { BotType.ZombieGangster, ZombieGangsterProfiles },
            { BotType.ZombieNinja, ZombieNinjaProfiles },
            { BotType.ZombiePolice, ZombiePoliceProfiles },
            { BotType.ZombiePrussian, ZombiePrussianProfiles },
            { BotType.BaronVonHauptstein, BaronVonHauptsteinProfiles },
            { BotType.ZombieSoldier, ZombieSoldierProfiles },
            { BotType.ZombieThug, ZombieThugProfiles },
            { BotType.ZombieWorker, ZombieWorkerProfiles },
        };

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

        private static Dictionary<string, List<WeaponItem>> WeaponStock = new Dictionary<string, List<WeaponItem>>()
        {
            {
                TIER_1, new List<WeaponItem>()
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
                }
            },
            {
                TIER_2, new List<WeaponItem>()
                {
                    WeaponItem.MACHETE,
                    WeaponItem.AXE,
                    WeaponItem.CHAIN,
                    WeaponItem.SHOTGUN,
                    WeaponItem.SAWED_OFF,
                    WeaponItem.GRENADES,
                }
            },
            {
                TIER_3, new List<WeaponItem>()
                {
                    WeaponItem.SMG,
                    WeaponItem.MP50,
                    WeaponItem.ASSAULT,
                    WeaponItem.SILENCEDPISTOL,
                    WeaponItem.SILENCEDUZI,
                    WeaponItem.MINES,
                    WeaponItem.DARK_SHOTGUN,
                    WeaponItem.C4,
                    WeaponItem.TOMMYGUN,
                }
            },
        };

        private static Dictionary<BotType, List<WeaponSet>> BotWeapons = new Dictionary<BotType, List<WeaponSet>>()
        {
            {
                BotType.AssassinMelee, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KATANA,
                    },
                }
            },
            {
                BotType.AssassinRange, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.UZI,
                    },
                }
            },
            {
                BotType.Agent, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    },
                }
            },
            {
                BotType.Agent2, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.SHOCK_BATON,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.MAGNUM,
                        UseLazer = true,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                        Secondary = WeaponItem.UZI,
                        UseLazer = true,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                        UseLazer = true,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.DARK_SHOTGUN,
                        UseLazer = true,
                    },
                }
            },
            {
                BotType.Bandido, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.MACHETE,
                        Secondary = WeaponItem.REVOLVER,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.CARBINE,
                        Secondary = WeaponItem.REVOLVER,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.SHOTGUN,
                        Secondary = WeaponItem.PISTOL,
                    },
                }
            },
            {
                BotType.Biker, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.CHAIN,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    },
                }
            },
            {
                BotType.BikerHulk, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.Bodyguard, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    },
                }
            },
            {
                BotType.Bodyguard2, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    },
                }
            },
            {
                BotType.ClownBodyguard, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KATANA,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.AXE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BAT,
                    },
                }
            },
            {
                BotType.ClownBoxer, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.ClownCowboy, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    },
                }
            },
            {
                BotType.ClownGangster, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    },
                }
            },
            {
                BotType.Cowboy, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SAWED_OFF,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.MAGNUM,
                    },
                }
            },
            {
                BotType.Demolitionist, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SNIPER,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.GRENADE_LAUNCHER,
                    },
                }
            },
            {
                BotType.Fritzliebe, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.Funnyman, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                    new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    },
                }
            },
            {
                BotType.Elf, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.CHAIN,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.FLAMETHROWER,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.UZI,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.FLAREGUN,
                    },
                }
            },
            {
                BotType.Gangster, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BAT,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BOTTLE,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.UZI,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SAWED_OFF,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                    },
                }
            },
            {
                BotType.GangsterHulk, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.Kingpin, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.MAGNUM,
                    },
                }
            },
            {
                BotType.Kriegbär, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Powerup = WeaponItem.SLOWMO_10,
                    },
                }
            },
            {
                BotType.Meatgrinder, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.CHAINSAW,
                        Throwable = WeaponItem.MOLOTOVS,
                        Powerup = WeaponItem.SLOWMO_10,
                    },
                }
            },
            {
                BotType.Mecha, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.MetroCop, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.SHOCK_BATON,
                        Primary = WeaponItem.SMG,
                        UseLazer = true,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.SHOCK_BATON,
                        Primary = WeaponItem.DARK_SHOTGUN,
                        UseLazer = true,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.ASSAULT,
                        UseLazer = true,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.DARK_SHOTGUN,
                        UseLazer = true,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SMG,
                        UseLazer = true,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SHOCK_BATON,
                        UseLazer = true,
                    },
                }
            },
            {
                BotType.MetroCop2, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.SHOCK_BATON,
                        Secondary = WeaponItem.PISTOL,
                        UseLazer = true,
                    },
                }
            },
            {
                BotType.Mutant, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.NaziLabAssistant, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Powerup = WeaponItem.STRENGTHBOOST,
                    },
                }
            },
            {
                BotType.NaziMuscleSoldier, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    },
                }
            },
            {
                BotType.NaziScientist, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.CHAIR,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BOTTLE,
                    },
                }
            },
            {
                BotType.NaziSoldier, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                        Throwable = WeaponItem.GRENADES,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.MP50,
                        Throwable = WeaponItem.GRENADES,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.CARBINE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.CARBINE,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.CARBINE,
                        Throwable = WeaponItem.GRENADES,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Secondary = WeaponItem.PISTOL,
                    },
                }
            },
            {
                BotType.SSOfficer, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.MP50,
                        Secondary = WeaponItem.PISTOL,
                    },
                }
            },
            {
                BotType.Police, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                        Secondary = WeaponItem.PISTOL,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                        Primary = WeaponItem.SHOTGUN,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BATON,
                        Secondary = WeaponItem.REVOLVER,
                    },
                }
            },
            {
                BotType.PoliceSWAT, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Secondary = WeaponItem.PISTOL45,
                        Throwable = WeaponItem.C4,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Secondary = WeaponItem.MACHINE_PISTOL,
                        Throwable = WeaponItem.GRENADES,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.ASSAULT,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.SMG,
                    },
                }
            },
            {
                BotType.Santa, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.M60,
                        Secondary = WeaponItem.UZI,
                    },
                }
            },
            {
                BotType.Sniper, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Primary = WeaponItem.SNIPER,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SNIPER,
                        Secondary = WeaponItem.SILENCEDPISTOL,
                    },
                }
            },
            {
                BotType.Soldier, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.ASSAULT,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SMG,
                    },
                }
            },
            {
                BotType.Soldier2, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.GRENADE_LAUNCHER,
                    },
                }
            },
            {
                BotType.Teddybear, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Throwable = WeaponItem.GRENADES,
                        Powerup = WeaponItem.SLOWMO_10,
                    },
                }
            },
            {
                BotType.Thug, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BAT,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.HAMMER,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.CHAIN,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.MACHINE_PISTOL,
                    },
                }
            },
            {
                BotType.ThugHulk, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                    new WeaponSet()
                    {
                        Melee = WeaponItem.LEAD_PIPE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.PIPE,
                    },
                }
            },
            {
                BotType.Zombie, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.ZombieAgent, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.SILENCEDPISTOL,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.SILENCEDUZI,
                    },
                }
            },
            {
                BotType.ZombieBruiser, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.ZombieChild, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.ZombieFighter, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Powerup = WeaponItem.SLOWMO_10,
                    },
                }
            },
            {
                BotType.ZombieFlamer, new List<WeaponSet>()
                {
                    WeaponSet.Empty,
                }
            },
            {
                BotType.ZombieGangster, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.TOMMYGUN,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    },
                }
            },
            {
                BotType.ZombieNinja, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KATANA,
                    },
                }
            },
            {
                BotType.ZombiePrussian, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.REVOLVER,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.CARBINE,
                        Throwable = WeaponItem.GRENADES,
                    },
                }
            },
            {
                BotType.BaronVonHauptstein, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                        Secondary = WeaponItem.REVOLVER,
                        Throwable = WeaponItem.GRENADES,
                    },
                }
            },
            {
                BotType.ZombieSoldier, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SMG,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.ASSAULT,
                    },
                    new WeaponSet()
                    {
                        Primary = WeaponItem.SHOTGUN,
                    },
                    new WeaponSet()
                    {
                        Throwable = WeaponItem.GRENADES,
                    },
                    new WeaponSet()
                    {
                        Throwable = WeaponItem.MINES,
                    },
                }
            },
            {
                BotType.ZombieThug, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.BAT,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.KNIFE,
                    },
                    new WeaponSet()
                    {
                        Secondary = WeaponItem.PISTOL,
                    },
                    new WeaponSet()
                    {
                        Throwable = WeaponItem.MOLOTOVS,
                    },
                }
            },
            {
                BotType.ZombieWorker, new List<WeaponSet>()
                {
                    new WeaponSet()
                    {
                        Melee = WeaponItem.PIPE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.HAMMER,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.AXE,
                    },
                    new WeaponSet()
                    {
                        Melee = WeaponItem.CHAINSAW,
                    },
                }
            },
        };

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
            Soldier,
            Sniper,

            ZombieSlow,
            ZombieFast,
        }

        private static Dictionary<BotAI, BotBehaviorSet> BotBehaviors = new Dictionary<BotAI, BotBehaviorSet>()
        {
            {
                BotAI.Debug, GetBotBehaviorSet(BotAI.Debug)
            },
            {
                BotAI.Easy, GetBotBehaviorSet(BotAI.Easy)
            },
            {
                BotAI.Normal, GetBotBehaviorSet(BotAI.Normal)
            },
            {
                BotAI.Hard, GetBotBehaviorSet(BotAI.Hard)
            },
            {
                BotAI.Expert, GetBotBehaviorSet(BotAI.Expert)
            },
            {
                BotAI.MeleeExpert, GetBotBehaviorSet(BotAI.MeleeExpert)
            },
            {
                BotAI.RangeExpert, GetBotBehaviorSet(BotAI.RangeExpert)
            },
            {
                BotAI.RangeHard, GetBotBehaviorSet(BotAI.RangeHard)
            },
            {
                BotAI.Grunt, GetBotBehaviorSet(BotAI.Grunt)
            },
            {
                BotAI.Hulk, GetBotBehaviorSet(BotAI.Hulk)
            },
            {
                BotAI.Meatgrinder, GetBotBehaviorSet(BotAI.Meatgrinder)
            },
            {
                BotAI.Sniper, GetBotBehaviorSet(BotAI.Sniper)
            },
            {
                BotAI.ZombieSlow, GetBotBehaviorSet(BotAI.ZombieSlow)
            },
        };

        private static BotBehaviorSet GetBotBehaviorSet(BotAI botAI)
        {
            var botBehaviorSet = new BotBehaviorSet()
            {
                MeleeActions = BotMeleeActions.Default,
                MeleeActionsWhenHit = BotMeleeActions.DefaultWhenHit,
                MeleeActionsWhenEnraged = BotMeleeActions.DefaultWhenEnraged,
                MeleeActionsWhenEnragedAndHit = BotMeleeActions.DefaultWhenEnragedAndHit
            };

            switch (botAI)
            {
                case BotAI.Debug:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotD);
                    botBehaviorSet.RangedWeaponBurstTimeMin = 5000;
                    botBehaviorSet.RangedWeaponBurstTimeMax = 5000;
                    botBehaviorSet.RangedWeaponBurstPauseMin = 0;
                    botBehaviorSet.RangedWeaponBurstPauseMax = 0;
                    break;

                case BotAI.Easy:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotD);
                    break;

                case BotAI.Normal:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotC);
                    break;

                case BotAI.Hard:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotB);
                    break;

                case BotAI.Expert:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotA);
                    break;

                case BotAI.Hacker:
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
                    botBehaviorSet.MeleeWaitTimeLimitMin = 600f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 800f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.SetMeleeActionsToExpert();
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.RangedWeaponUsage = true;
                    botBehaviorSet.RangedWeaponAccuracy = 0.85f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 100f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMin = 100f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMax = 100f;
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

                case BotAI.MeleeExpert:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    break;

                case BotAI.RangeExpert:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.RangedA);
                    botBehaviorSet.RangedWeaponAccuracy = 0.85f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 600f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 2000f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.95f;
                    break;

                case BotAI.RangeHard:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.RangedA);
                    botBehaviorSet.RangedWeaponAccuracy = 0.75f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 600f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 2000f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.9f;
                    break;

                case BotAI.Sniper:
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


                case BotAI.Grunt:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Grunt);

                    // Taken from PredefinedAIType.BotB, PredefinedAIType.Grunt is too slow
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 200f;
                    botBehaviorSet.RangedWeaponAimShootDelayMax = 600f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMin = 200f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMax = 600f;
                    botBehaviorSet.RangedWeaponBurstTimeMin = 400f;
                    botBehaviorSet.RangedWeaponBurstTimeMax = 800f;
                    botBehaviorSet.RangedWeaponBurstPauseMin = 400f;
                    botBehaviorSet.RangedWeaponBurstPauseMax = 800f;
                    break;

                case BotAI.Hulk:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Hulk);
                    break;

                case BotAI.Meatgrinder:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Meatgrinder);
                    break;

                case BotAI.ZombieSlow:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.ZombieA);
                    break;

                case BotAI.ZombieFast:
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.ZombieB);
                    break;

                default:
                    botBehaviorSet.NavigationMode = BotBehaviorNavigationMode.None;
                    botBehaviorSet.MeleeMode = BotBehaviorMeleeMode.None;
                    botBehaviorSet.EliminateEnemies = false;
                    botBehaviorSet.SearchForItems = false;
                    break;
            }

            return botBehaviorSet;
        }

        #endregion

        #region Bot infos

        public class BotInfo
        {
            public BotInfo()
            {
                EquipWeaponChance = 1.0f;
                OnSpawn = null;
                IsBoss = false;
                InitialWeaponDrawn = WeaponItemType.NONE;
                SpawnLine = "";
                DeathLine = "";
            }

            public float EquipWeaponChance { get; set; } // 0-1
            public BotAI AIType { get; set; }
            public PlayerModifiers Modifiers { get; set; }
            public bool IsBoss { get; set; }
            public Action<Bot> OnSpawn { get; set; }
            public WeaponItemType InitialWeaponDrawn { get; set; }
            public string SpawnLine { get; set; }
            public string DeathLine { get; set; }
        }

        // CanBurn [1]
        // CurrentEnergy [100]
        // CurrentHealth [100]
        // 
        // MaxHealth: 1-9999 [100]
        // MaxEnergy: [100]
        // EnergyConsumptionModifier: 0-100 [1.0] ?
        // ProjectileDamageDealtModifier: 0-100 [1.0]
        // ProjectileCritChanceDealtModifier: 0-100 [1.0]
        // MeleeDamageDealtModifier: 0-100 [1.0]
        // MeleeForceModifier: [1.0]
        // RunSpeedModifier: 0.5-2.0 [1.0]
        // SizeModifier: 0.75-1.25 [1.0]

        // Base stats: Grunt
        // MaxHealth: 70
        // ProjectileDamageDealtModifier = 0.9f,
        // ProjectileCritChanceDealtModifier = 0.9f,
        // MeleeDamageDealtModifier = 0.95f,
        // SizeModifier = 0.95f,

        private static BotInfo GruntInfo = new BotInfo()
        {
            AIType = BotAI.Grunt,
            EquipWeaponChance = 0.5f,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 70,
                CurrentHealth = 70,
                ProjectileDamageDealtModifier = 0.9f,
                MeleeDamageDealtModifier = 0.95f,
                SizeModifier = 0.95f,
            },
        };
        private static BotInfo GruntWithWeaponsInfo = new BotInfo()
        {
            AIType = BotAI.Grunt,
            EquipWeaponChance = 1f,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 70,
                CurrentHealth = 70,
                ProjectileDamageDealtModifier = 0.9f,
                MeleeDamageDealtModifier = 0.95f,
                SizeModifier = 0.95f,
            },
        };
        private static BotInfo CowboyInfo = new BotInfo() // Faster Grunt
        {
            AIType = BotAI.Grunt,
            EquipWeaponChance = 1f,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 70,
                CurrentHealth = 70,
                ProjectileDamageDealtModifier = 1.1f,
                MeleeDamageDealtModifier = 0.85f,
                RunSpeedModifier = 1.1f,
                SprintSpeedModifier = 1.1f,
                SizeModifier = 0.9f,
            },
        };
        private static BotInfo HulkInfo = new BotInfo()
        {
            AIType = BotAI.Hulk,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 150,
                CurrentHealth = 150,
                ProjectileDamageDealtModifier = 0.5f,
                MeleeDamageDealtModifier = 1.1f,
                MeleeForceModifier = 1.5f,
                RunSpeedModifier = 0.75f,
                SprintSpeedModifier = 0.75f,
                SizeModifier = 1.15f,
            },
        };
        private static BotInfo BoxerInfo = new BotInfo()
        {
            AIType = BotAI.Hulk,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 110,
                CurrentHealth = 110,
                ProjectileDamageDealtModifier = 0.5f,
                MeleeDamageDealtModifier = 1.1f,
                MeleeForceModifier = 1.5f,
                SizeModifier = 1.15f,
            },
        };
        private static BotInfo SniperInfo = new BotInfo()
        {
            AIType = BotAI.Sniper,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 60,
                CurrentHealth = 60,
                ProjectileDamageDealtModifier = 1.15f,
                ProjectileCritChanceDealtModifier = 1.15f,
                MeleeDamageDealtModifier = 0.85f,
                RunSpeedModifier = 0.8f,
                SprintSpeedModifier = 0.8f,
                SizeModifier = 0.95f,
            },
        };
        private static BotInfo ZombieInfo = new BotInfo()
        {
            EquipWeaponChance = 0.0f,
            AIType = BotAI.ZombieSlow,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 60,
                CurrentHealth = 60,
                MeleeDamageDealtModifier = 0.75f,
                RunSpeedModifier = 0.75f,
                SprintSpeedModifier = 0.75f,
                SizeModifier = 0.95f,
            },
        };
        // Bosses
        private static BotInfo DemotionalistInfo = new BotInfo()
        {
            AIType = BotAI.RangeHard,
            Modifiers = new PlayerModifiers()
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
            },
            IsBoss = true,
        };
        private static BotInfo KingpinInfo = new BotInfo()
        {
            AIType = BotAI.Hard,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 250,
                CurrentHealth = 250,
                ProjectileDamageDealtModifier = 1.2f,
                MeleeDamageDealtModifier = 1.2f,
                SizeModifier = 1.05f,
            },
            IsBoss = true,
        };
        private static BotInfo MeatgrinderInfo = new BotInfo()
        {
            AIType = BotAI.Meatgrinder,
            Modifiers = new PlayerModifiers()
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
            },
            IsBoss = true,
            InitialWeaponDrawn = WeaponItemType.Melee,
        };
        private static BotInfo TeddybearInfo = new BotInfo()
        {
            AIType = BotAI.Hulk,
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 400,
                CurrentHealth = 400,
                MaxEnergy = 400,
                CurrentEnergy = 400,
                MeleeDamageDealtModifier = 1.25f,
                MeleeForceModifier = 2.0f,
                RunSpeedModifier = 0.9f,
                SprintSpeedModifier = 0.9f,
                SizeModifier = 1.25f,
            },
            IsBoss = true,
        };
        private static BotInfo SantaInfo = new BotInfo()
        {
            AIType = BotAI.Hard, // ChallengeA
            Modifiers = new PlayerModifiers()
            {
                MaxHealth = 200,
                CurrentHealth = 200,
                ExplosionDamageTakenModifier = 0.5f,
                MeleeForceModifier = 1.75f,
                SizeModifier = 1.1f,
                InfiniteAmmo = 1,
            },
            IsBoss = true,
            SpawnLine = "Ho ho ho!",
            DeathLine = "Ho ohhhh...",
        };

        public static Dictionary<BotType, BotInfo> BotInfos = new Dictionary<BotType, BotInfo>()
        {
            { BotType.Bandido, GruntWithWeaponsInfo },
            { BotType.Biker, GruntInfo },
            { BotType.BikerHulk, HulkInfo },
            { BotType.Bodyguard, GruntWithWeaponsInfo },
            { BotType.ClownBoxer, BoxerInfo },
            { BotType.ClownCowboy, CowboyInfo },
            { BotType.ClownGangster, GruntWithWeaponsInfo },
            { BotType.Cowboy, CowboyInfo },
            { BotType.Demolitionist, DemotionalistInfo },
            { BotType.Elf, GruntWithWeaponsInfo },
            { BotType.Kingpin, KingpinInfo },
            { BotType.MetroCop, GruntWithWeaponsInfo },
            { BotType.Meatgrinder, MeatgrinderInfo },
            { BotType.NaziScientist, GruntInfo },
            { BotType.Police, GruntWithWeaponsInfo },
            { BotType.Sniper, SniperInfo },
            { BotType.Teddybear, TeddybearInfo },
            { BotType.Santa, SantaInfo },
            { BotType.Thug, GruntInfo },
            { BotType.ThugHulk, HulkInfo },
            { BotType.Zombie, ZombieInfo },
        };

        #endregion

        #region Bot group

        public class Group
        {
            public Group(Dictionary<BotType, float> types)
            {
                Types = types;

                HasBoss = false;
                foreach (var pair in Types)
                {
                    if (BotInfos[pair.Key].IsBoss)
                        HasBoss = true;
                    TotalScore += pair.Value;
                }
            }

            public Dictionary<BotType, float> Types { get; private set; }
            public float TotalScore { get; private set; }
            public bool HasBoss { get; private set; }
        }

        public class GroupSet
        {
            public GroupSet(string name, List<Group> groups)
            {
                Name = name;
                Groups = groups;
            }

            public GroupSet(string name, Group group)
            {
                Name = name;
                Groups = new List<Group>() { group };
            }

            public string Name { get; set; }
            public List<Group> Groups { get; set; }
            public bool HasBoss
            {
                get { return Groups.Where(g => g.HasBoss).Any(); }
            }
        }

        private static List<GroupSet> BotGroupSets = new List<GroupSet>()
        {
            //new GroupSet("Agent", new Group(new Dictionary<BotType, float>()
            //{
            //    { BotType.Agent, 1.0f },
            //})),
            new GroupSet("Bandido", new Group(new Dictionary<BotType, float>()
            {
                { BotType.Bandido, 1f },
            })),
            new GroupSet("Biker", new List<Group>()
            {
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.Biker, 1f },
                }),
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.Biker, 0.6f },
                    { BotType.BikerHulk, 0.4f },
                }),
            }),
            new GroupSet("Clown", new List<Group>()
            {
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.ClownCowboy, 0.5f },
                    { BotType.ClownGangster, 0.25f },
                    { BotType.ClownBoxer, 0.25f },
                }),
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.ClownCowboy, 0.6f },
                    { BotType.ClownGangster, 0.4f },
                }),
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.ClownBoxer, 0.7f },
                    { BotType.ClownGangster, 0.3f },
                }),
            }),
            new GroupSet("Cowboy", new Group(new Dictionary<BotType, float>()
            {
                { BotType.Cowboy, 1f },
            })),
            new GroupSet("Demolitionist", new Group(new Dictionary<BotType, float>()
            {
                { BotType.Demolitionist, 1f },
            })),
            new GroupSet("Gangster", new List<Group>()
            {
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.Kingpin, -1 },
                    { BotType.Bodyguard, 1f },
                }),
                //new Group(new Dictionary<BotType, float>()
                //{
                //    { BotType.Kingpin, -1 },
                //    { BotType.Bodyguard, 1f },
                //    { BotType.Gangster, 1f },
                //    { BotType.GangsterHulk, 1f },
                //}),
            }),
            new GroupSet("Meatgrinder", new Group(new Dictionary<BotType, float>()
            {
                { BotType.Meatgrinder, 1f },
            })),
            new GroupSet("MetroCop", new List<Group>()
            {
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.MetroCop, 1f },
                }),
                //new Group(new Dictionary<BotType, float>()
                //{
                //    { BotType.MetroCop, 0.9f },
                //    { BotType.MetroCop2, 0.1f },
                //}),
            }),
            new GroupSet("Police", new List<Group>()
            {
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.Police, 1f },
                }),
                //new Group(new Dictionary<BotType, float>()
                //{
                //    { BotType.Police, 0.8f },
                //    { BotType.PoliceSWAT, 0.2f },
                //}),
            }),
            //new GroupSet("PoliceSWAT", new Group(new Dictionary<BotType, float>()
            //{
            //    { BotType.PoliceSWAT, 1f },
            //})),
            new GroupSet("Santa", new Group(new Dictionary<BotType, float>()
            {
                { BotType.Santa, -1f },
                { BotType.Elf, 1f },
            })),
            new GroupSet("Sniper", new Group(new Dictionary<BotType, float>()
            {
                { BotType.Sniper, 1f },
            })),
            new GroupSet("Teddybear", new Group(new Dictionary<BotType, float>()
            {
                { BotType.Teddybear, 1f },
            })),
            new GroupSet("Thug", new List<Group>()
            {
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.Thug, 1f },
                }),
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.Thug, 0.6f },
                    { BotType.ThugHulk, 0.4f },
                }),
            }),
            new GroupSet("Zombie", new List<Group>() // TODO
            {
                new Group(new Dictionary<BotType, float>()
                {
                    { BotType.Zombie, 1.0f },
                }),
            }),
        };

        #endregion

        public class PlayerSpawner
        {
            public IObjectPlayerSpawnTrigger Trigger { get; set; }
            public bool HasSpawned { get; set; }
        }

        public class Bot
        {
            public Bot(IPlayer player, BotType type)
            {
                Player = player;
                Type = type;

                SaySpawnLine();
            }
            public IPlayer Player { get; set; }
            public BotType Type { get; set; }
            public BotInfo Info
            {
                get { return BotInfos[Type]; }
            }

            public void SaySpawnLine()
            {
                var spawnLine = Info.SpawnLine;

                if (!string.IsNullOrWhiteSpace(spawnLine))
                    Game.CreateDialogue(spawnLine, new Color(128, 32, 32), Player, duration: 3000f);
            }

            public void SayDeathLine()
            {
                var deathLine = Info.DeathLine;

                if (!string.IsNullOrWhiteSpace(deathLine))
                    Game.CreateDialogue(deathLine, new Color(128, 32, 32), Player, duration: 3000f);
            }
        }

        public static class BotHelper
        {
            private static List<PlayerSpawner> m_playerSpawners;
            private static Events.PlayerDeathCallback m_playerDeathEvent = null;
            private static Dictionary<int, Bot> m_bots = new Dictionary<int, Bot>();

            public static void Initialize()
            {
                m_playerSpawners = GetEmptyPlayerSpawners();
                m_playerDeathEvent = Events.PlayerDeathCallback.Start(OnPlayerDeath);

                var playerCount = Game.GetPlayers().Length;
                var botCount = MAX_PLAYERS - playerCount;
                var botSpawnCount = Math.Min(botCount, m_playerSpawners.Count);

                if (!Game.IsEditorTest)
                {
                    if (botSpawnCount < 3) // Too few for a group, spawn boss instead
                    {
                        var bossGroupSets = BotGroupSets.Select(s => s).Where(s => s.HasBoss).ToList();
                        var bossGroupSet = Helper.GetRandomItem(bossGroupSets);
                        SpawnGroup(Helper.GetRandomItem(bossGroupSet.Groups), botSpawnCount);
                    }
                    else
                    {
                        var groupSet = Helper.GetRandomItem(BotGroupSets);
                        var group = Helper.GetRandomItem(groupSet.Groups);
                        SpawnGroup(group, botSpawnCount);
                    }
                }
                else
                {
                    //Create(BotType.Sniper);
                    SpawnGroup("Santa", botSpawnCount);

                    //var meleeBot = Create(BotType.DebugBot);
                    //var meleeBehavior = BotBehaviors[BotAI.MeleeExpert];
                    //meleeBot.Player.SetBotName("MeleeBot");
                    //meleeBot.Player.SetBotBehaviorSet(meleeBehavior);
                    //meleeBot.Player.SetTeam(PlayerTeam.Independent);

                    //var expertBot = Create(BotType.DebugBot);
                    //var expertBehavior = BotBehaviors[BotAI.Expert];
                    //expertBehavior.SearchForItems = false;
                    //expertBot.Player.SetBotName("ExpertBot");
                    //expertBot.Player.SetBotBehaviorSet(expertBehavior);
                    //expertBot.Player.SetTeam(PlayerTeam.Independent);
                }
            }

            private static void OnPlayerDeath(IPlayer player)
            {
                if (player == null) return;

                switch (player.GetTeam())
                {
                    case PlayerTeam.Team4:
                        Bot enemy;
                        if (m_bots.TryGetValue(player.UniqueID, out enemy))
                        {
                            enemy.SayDeathLine();
                        }
                        break;
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

            // Spawn exact group for debugging purpose. Usually you random the group before every match
            private static void SpawnGroup(string groupName, int botCount)
            {
                var groupSet = BotGroupSets.Where(g => g.Name == groupName).FirstOrDefault();
                if (groupSet == null) return;

                SpawnGroup(groupSet.Groups[0], botCount);
            }

            private static void SpawnGroup(Group group, int groupCount)
            {
                var typeCount = 0;
                var groupCountRemaining = groupCount;

                foreach (var type in group.Types)
                {
                    typeCount++;

                    var botType = type.Key;
                    var weight = type.Value;
                    var share = weight / group.TotalScore;
                    var botCount = groupCount * share;

                    if (!BotInfos[botType].IsBoss)
                    {
                        while (groupCountRemaining > 0 && (botCount > 0 || typeCount == group.Types.Count))
                        {
                            SpawnBot(botType);
                            groupCountRemaining--;
                            botCount--;
                        }
                    }
                    else
                        SpawnBot(botType);
                }
            }

            private static IPlayer SpawnPlayer(BotInfo botInfo, WeaponSet weaponSet)
            {
                var emptySpawners = m_playerSpawners
                    .Select((playerSpawner, index) => new { playerSpawner, index })
                    .Where(Q => Q.playerSpawner.HasSpawned == false)
                    .ToList();

                if (!emptySpawners.Any())
                    return null;

                var rndSpawner = Helper.GetRandomItem(emptySpawners);
                var spawnTrigger = rndSpawner.playerSpawner.Trigger;

                // TODO: fix runtime error
                //spawnTrigger.SetSpawnWeaponItemMelee(weaponSet.Melee);
                //spawnTrigger.SetSpawnWeaponItemRifle(weaponSet.Primary);
                //spawnTrigger.SetSpawnWeaponItemHandgun(weaponSet.Secondary);
                //spawnTrigger.SetSpawnWeaponItemThrown(weaponSet.Throwable);
                //spawnTrigger.SetSpawnWeaponItemPowerup(weaponSet.Powerup);
                spawnTrigger.SetInitialWeaponDrawn(botInfo.InitialWeaponDrawn);
                spawnTrigger.Trigger();

                var player = spawnTrigger.GetLastCreatedPlayer();

                player.GiveWeaponItem(weaponSet.Melee);
                player.GiveWeaponItem(weaponSet.Primary);
                player.GiveWeaponItem(weaponSet.Secondary);
                player.GiveWeaponItem(weaponSet.Throwable);
                player.GiveWeaponItem(weaponSet.Powerup);
                if (weaponSet.UseLazer)
                    player.GiveWeaponItem(WeaponItem.LAZER);

                m_playerSpawners[rndSpawner.index].HasSpawned = true;

                return player;
            }

            private static Bot SpawnBot(BotType botType)
            {
                var info = BotInfos[botType];
                var weaponSet = WeaponSet.Empty;

                if (Rnd.Next(100) / 100.0f <= info.EquipWeaponChance) // Next(100) -> 0-99
                    weaponSet = Helper.GetRandomItem(BotWeapons[botType]);

                var player = SpawnPlayer(info, weaponSet);
                if (player == null) return null;

                var profile = Helper.GetRandomItem(BotProfiles[botType]);

                player.SetProfile(profile);
                player.SetModifiers(info.Modifiers);
                player.SetBotBehaviorSet(BotBehaviors[info.AIType]);
                player.SetBotBehaviorActive(true);
                player.SetTeam(PlayerTeam.Team4);
                player.SetBotName(profile.Name);

                m_bots.Add(player.UniqueID, new Bot(player, botType));
                return m_bots[player.UniqueID];
            }
        }
    }
}

// TODO:
// Add draw weapon first

// Not grunt:
// agent
// assassin (both)
// fritzliebe
// funnyman
// Kriegbär
// mecha

// Meatgrider block?