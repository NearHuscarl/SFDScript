using System;
using System.Collections.Generic;
using SFDGameScriptInterface;

namespace SFDScript.Draft
{
    public class SantaBullet : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public SantaBullet() : base(null) { }

        public void OnStartup()
        {
            Game.GetPlayers()[0].GiveWeaponItem(WeaponItem.M60);
            Game.RunCommand("ia 1");

            Events.ProjectileCreatedCallback.Start(OnProjectileCreated);
            Events.ObjectTerminatedCallback.Start(OnObjectTerminated);
        }

        private static Random rnd = new Random();
        private static T GetItem<T>(List<T> list)
        {
            var rndIndex = rnd.Next(list.Count);
            return list[rndIndex];
        }

        private Dictionary<int, IObject> m_customBullets = new Dictionary<int, IObject>();
        private readonly List<string> m_presents = new List<string>()
        {
            "XmasPresent00",
            "WpnPistol",
            "WpnPistol45",
            "WpnSilencedPistol",
            "WpnMachinePistol",
            "WpnMagnum",
            "WpnRevolver",
            "WpnPumpShotgun",
            "WpnDarkShotgun",
            "WpnTommygun",
            "WpnSMG",
            "WpnM60",
            "WpnPipeWrench",
            "WpnChain",
            "WpnWhip",
            "WpnHammer",
            "WpnKatana",
            "WpnMachete",
            "WpnChainsaw",
            "WpnKnife",
            "WpnSawedoff",
            "WpnBat",
            "WpnBaton",
            "WpnShockBaton",
            "WpnLeadPipe",
            "WpnUzi",
            "WpnSilencedUzi",
            "WpnBazooka",
            "WpnAxe",
            "WpnAssaultRifle",
            "WpnMP50",
            "WpnSniperRifle",
            "WpnCarbine",
            "WpnFlamethrower",
            "ItemPills",
            "ItemMedkit",
            "ItemSlomo5",
            "ItemSlomo10",
            "ItemStrengthBoost",
            "ItemSpeedBoost",
            "ItemLaserSight",
            "ItemBouncingAmmo",
            "ItemFireAmmo",
            "WpnGrenades",
            "WpnMolotovs",
            "WpnMines",
            "WpnShuriken",
            "WpnBow",
            "WpnFlareGun",
            "WpnGrenadeLauncher",
        };
        private readonly List<string> m_oofs = new List<string>()
        {
            "WpnGrenadesThrown",
            "WpnMolotovsThrown",
            "WpnMineThrown",
        };
        private void OnObjectTerminated(IObject[] objs)
        {
            foreach (var obj in objs)
            {
                if (m_customBullets.ContainsKey(obj.UniqueID))
                {
                    var customBullet = m_customBullets[obj.UniqueID];
                    var position = customBullet.GetWorldPosition();

                    // normally, the present spawn some random shits upon destroyed. make the present disappeared
                    // and spawn something else as a workaround
                    customBullet.SetWorldPosition(new Vector2(-5000, 5000));

                    var rndNum = rnd.Next(0, 100);
                    if (rndNum < 1) // big oof
                    {
                        SpawnBadSanta(position);
                    }
                    if (1 <= rndNum && rndNum < 5)
                    {
                        Game.CreateObject(GetItem(m_oofs), position);
                    }
                    if (5 <= rndNum && rndNum < 30)
                    {
                        Game.CreateObject(GetItem(m_presents), position);
                    }

                    m_customBullets.Remove(obj.UniqueID);
                }
            }
        }

        private void SpawnBadSanta(Vector2 position)
        {
            var player = Game.CreatePlayer(position);

            player.SetModifiers(new PlayerModifiers()
            {
                MaxHealth = 200,
                CurrentHealth = 200,
                ExplosionDamageTakenModifier = 0.5f,
                MeleeForceModifier = 1.5f,
                SizeModifier = 1.1f,
                InfiniteAmmo = 1,
            });
            player.SetProfile(new IProfile()
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
            player.SetBotName("Bad Santa");

            player.SetBotBehaviorSet(BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.ChallengeA));
            player.SetBotBehaviorActive(true);

            player.SetTeam(PlayerTeam.Independent);

            player.GiveWeaponItem(WeaponItem.KNIFE);
            player.GiveWeaponItem(WeaponItem.M60);
            player.GiveWeaponItem(WeaponItem.UZI);

            Game.CreateDialogue("Ho ho ho!", new Color(128, 32, 32), player);
        }

        private void OnProjectileCreated(IProjectile[] projectiles)
        {
            foreach (var projectile in projectiles)
            {
                switch (projectile.ProjectileItem)
                {
                    case ProjectileItem.BAZOOKA:
                    case ProjectileItem.GRENADE_LAUNCHER:
                        break;

                    default:
                        ToPresentBullet(projectile);
                        break;
                }
            }
        }

        private void ToPresentBullet(IProjectile projectile)
        {
            var customBullet = Game.CreateObject("XmasPresent00",
                worldPosition: projectile.Position,
                angle: (float)Math.Atan2(projectile.Direction.X, projectile.Direction.Y),
                linearVelocity: projectile.Velocity / 50 + new Vector2(0, 3),
                angularVelocity: 50f * (int)(projectile.Direction.X % 1),
                faceDirection: (int)(projectile.Direction.X % 1)
                );

            customBullet.TrackAsMissile(true);
            m_customBullets.Add(customBullet.UniqueID, customBullet);
            projectile.FlagForRemoval();
        }
    }
}
