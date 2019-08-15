using System;
using System.Collections.Generic;
using System.Linq;
using SFDGameScriptInterface;
using static SFDScript.Library.Mocks.MockObjects;

namespace SFDScript.Draft
{
    public class InfectedBullet : GameScriptInterface
    {
        private class InfectedCorpse
        {
            public static int TimeToTurnIntoZombie = 5000;
            public IPlayer Body { get; set; }
            public float DeathTime { get; private set; }
            public bool IsTurningIntoZombie { get; private set; }
            public bool CanTurnIntoZombie { get; private set; }
            public bool IsZombie { get; private set; }

            private bool TurnIntoZombie()
            {
                if (Body.IsRemoved || Body.IsBurnedCorpse) return false;

                var zombieBody = Game.CreatePlayer(Body.GetWorldPosition());
                if (rnd.Next(0, 100) < 10)
                {
                    Game.CreateDialogue("Brainzz", zombieBody);
                }
                zombieBody.SetBotBehaviorSet(BotBehaviorSet.GetBotBehaviorPredefinedSet(GetZombieType(Body)));
                zombieBody.SetTeam(ZombieTeam);

                var modifiers = Body.GetModifiers();
                modifiers.RunSpeedModifier -= 0.15f;
                modifiers.SprintSpeedModifier -= 0.15f;
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

            public InfectedCorpse(IPlayer player)
            {
                Body = player;
                IsTurningIntoZombie = false;
                IsZombie = false;
                CanTurnIntoZombie = true;
                DeathTime = Game.TotalElapsedGameTime;
            }

            public void Update()
            {
                if (IsElapsed(DeathTime, TimeToTurnIntoZombie))
                {
                    if (!IsTurningIntoZombie)
                    {
                        CanTurnIntoZombie = TurnIntoZombie();
                    }
                    if (!IsZombie)
                    {
                        UpdateTurningIntoZombieAnimation();
                    }
                }
            }

            private bool isKneeling;
            private float kneelingTime;
            private void UpdateTurningIntoZombieAnimation()
            {
                if (!isKneeling)
                {
                    kneelingTime = Game.TotalElapsedGameTime;
                    isKneeling = true;
                }
                else
                {
                    if (IsElapsed(kneelingTime, 700))
                    {
                        Body.AddCommand(new PlayerCommand(PlayerCommandType.StopCrouch));
                        Body.SetBotBehaivorActive(true);
                        IsZombie = true;
                    }
                }
            }
        }

        private class TheInfected
        {
            private IPlayer m_player;

            public TheInfected(IPlayer player)
            {
                m_player = player;
            }

            private float m_damageTimer = 0;
            public void Update(float dt)
            {
                if (!m_player.IsRemoved && !m_player.IsBurnedCorpse)
                {
                    m_damageTimer += dt;
                    if (m_damageTimer > 2000)
                    {
                        var health = m_player.GetHealth();
                        health -= 0.5f;
                        m_player.SetHealth(health);
                        Game.PlayEffect(EffectName.BloodTrail, m_player.GetWorldPosition());
                        m_damageTimer = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public InfectedBullet() : base(null) { }

        private static Random rnd = new Random();
        private static readonly PlayerTeam ZombieTeam = PlayerTeam.Team4;
        private static Dictionary<int, TheInfected> m_infectedPlayers = new Dictionary<int, TheInfected>();
        private static List<InfectedCorpse> m_infectedCorpses = new List<InfectedCorpse>();
        private Dictionary<int, IProjectile> m_customBullets = new Dictionary<int, IProjectile>();

        public void OnStartup()
        {
            Game.GetPlayers()[0].GiveWeaponItem(WeaponItem.M60);
            Game.RunCommand("ia 1");

            Events.UpdateCallback.Start(OnUpdate);
            Events.ProjectileCreatedCallback.Start(OnProjectileCreated);
            Events.ProjectileHitCallback.Start(OnProjectileHit);
            Events.PlayerDamageCallback.Start(OnPlayerDamage);
            Events.PlayerDeathCallback.Start(OnPlayerDeath);
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
                        ToInfectedBullet(projectile);
                        break;
                }
            }
        }

        private void OnProjectileHit(IProjectile projectile, ProjectileHitArgs args)
        {
            if (args.RemoveFlag)
            {
                m_customBullets.Remove(projectile.InstanceID);
            }
        }

        private void OnPlayerDamage(IPlayer player, PlayerDamageArgs args)
        {
            if (args.DamageType == PlayerDamageEventType.Projectile)
            {
                if (m_customBullets.ContainsKey(args.SourceID))
                {
                    if (rnd.Next(0, 100) < 15 && !CanInfectFrom(player) && !player.IsBurnedCorpse)
                    {
                        Infect(player);
                    }
                }
            }
            if (args.DamageType == PlayerDamageEventType.Melee)
            {
                var attacker = Game.GetPlayer(args.SourceID);

                UpdateInfectedStatus(player, attacker, args);
            }
        }

        private void UpdateInfectedStatus(IPlayer player, IPlayer attacker, PlayerDamageArgs args)
        {
            if (player == null) return;

            if (!CanInfectFrom(player) && !player.IsBurnedCorpse && attacker != null)
            {
                var attackerPunching = args.DamageType == PlayerDamageEventType.Melee
                    && attacker.CurrentWeaponDrawn == WeaponItemType.NONE
                    && !attacker.IsKicking && !attacker.IsJumpKicking;

                if (CanInfectFrom(attacker) && attackerPunching)
                {
                    Infect(player);
                }
            }
        }

        private static void OnPlayerDeath(IPlayer player, PlayerDeathArgs args)
        {
            if (player == null) return;

            if (m_infectedPlayers.ContainsKey(player.UniqueID))
            {
                m_infectedCorpses.Add(new InfectedCorpse(player));
            }
        }

        private bool CanInfectFrom(IPlayer player)
        {
            return m_infectedPlayers.ContainsKey(player.UniqueID)
                || HasZombieSkin(player);
        }

        private void Infect(IPlayer player)
        {
            Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(), "infected");
            m_infectedPlayers.Add(player.UniqueID, new TheInfected(player));

            if (player.IsDead)
            {
                m_infectedCorpses.Add(new InfectedCorpse(player));
            }
        }

        private void OnUpdate(float dt)
        {
            UpdateTheInfected(dt);
        }

        private void UpdateTheInfected(float dt)
        {
            foreach (var player in m_infectedPlayers.Values)
            {
                player.Update(dt);
            }

            foreach (var corpse in m_infectedCorpses.ToList())
            {
                corpse.Update();

                if (corpse.IsZombie || !corpse.CanTurnIntoZombie)
                {
                    m_infectedCorpses.Remove(corpse);
                }
            }
        }

        private void ToInfectedBullet(IProjectile projectile)
        {
            if (!m_customBullets.ContainsKey(projectile.InstanceID))
            {
                m_customBullets.Add(projectile.InstanceID, projectile);
            }
        }

        public static bool IsElapsed(float timeStarted, float timeToElapse)
        {
            return Game.TotalElapsedGameTime - timeStarted >= timeToElapse;
        }

        #region Zombie helper methods
        public static PredefinedAIType GetZombieType(IPlayer player)
        {
            if (player == null)
            {
                throw new Exception("Player cannot be null");
            }

            if (player.IsUser)
            {
                return PredefinedAIType.ZombieB;
            }
            else
            {
                var playerAI = player.GetBotBehavior().PredefinedAI;

                switch (playerAI)
                {
                    // Expert, Hard
                    case PredefinedAIType.BotA:
                    case PredefinedAIType.BotB:
                    case PredefinedAIType.ChallengeA:
                    case PredefinedAIType.MeleeB:
                        return PredefinedAIType.ZombieB;

                    default:
                        return PredefinedAIType.ZombieA;
                }
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

        private bool HasZombieSkin(IPlayer player)
        {
            return player.GetProfile().Skin.Name == "Zombie"
                || player.GetProfile().Skin.Name == "Zombie_fem"
                || player.GetProfile().Skin.Name == "FrankenbearSkin";
        }
        #endregion
    }
}
