using SFDGameScriptInterface;
using SFDScript.Library;
using System.Collections.Generic;
using static SFDScript.Library.Mocks.MockObjects;

namespace SFDScript.BotExtended.Bots
{
    public class MechaBot : Bot
    {
        public MechaBot() : base()
        {
            UpdateInterval = 50;
        }

        public override void OnSpawn(List<Bot> others)
        {
            var behavior = Player.GetBotBehaviorSet();
            behavior.SearchForItems = false;
            behavior.DefensiveAvoidProjectilesLevel = 0f;
            behavior.DefensiveBlockLevel = 0f;
            behavior.MeleeWeaponUsage = false;
            behavior.RangedWeaponUsage = false;

            Player.SetBotBehaviorSet(behavior);
            Player.SetHitEffect(PlayerHitEffect.Metal);
        }

        private float m_electricElapsed = 0f;
        protected override void OnUpdate(float elapsed)
        {
            if (Player == null || Player.IsRemoved) return;

            if (Player.IsDead)
            {
                UpdateCorpse(elapsed);
            }
            else
            {
                if (m_isDeathKneeling)
                {
                    UpdateDealthKneeling(elapsed);
                }
                else
                {
                    var mod = Player.GetModifiers();
                    var healthLeft = mod.CurrentHealth / mod.MaxHealth;

                    if (healthLeft <= 0.4f)
                        UpdateNearDeathEffects(elapsed, healthLeft);
                }
            }
        }

        private void UpdateCorpse(float elapsed)
        {
            m_electricElapsed += elapsed;

            if (m_electricElapsed >= 1000)
            {
                if (SharpHelper.RandomBoolean())
                {
                    var position = Player.GetWorldPosition();
                    position.X += SharpHelper.RandomBetween(-10, 10);
                    position.Y += SharpHelper.RandomBetween(-10, 10);

                    Game.PlayEffect(Effect.ELECTRIC, position);

                    if (SharpHelper.RandomBoolean())
                    {
                        Game.PlayEffect(Effect.STEAM, position);
                        Game.PlayEffect(Effect.STEAM, position);
                        Game.PlayEffect(Effect.STEAM, position);
                    }
                    // TODO: Customize spark effects
                    if (SharpHelper.RandomBoolean())
                        Game.PlayEffect(Effect.SPARKS, position);
                    if (SharpHelper.RandomBoolean())
                        Game.PlayEffect(Effect.FIRE, position);

                    Game.PlaySound("ElectricSparks", position);
                    m_electricElapsed = 0f;
                }
                else
                {
                    m_electricElapsed -= SharpHelper.RandomBetween(0, m_electricElapsed);
                }
            }
        }
        private void UpdateNearDeathEffects(float elapsed, float healthLeft)
        {
            m_electricElapsed += elapsed;

            if (m_electricElapsed >= 700)
            {
                if (SharpHelper.RandomBoolean())
                {
                    var position = Player.GetWorldPosition();
                    position.X += SharpHelper.RandomBetween(-10, 10);
                    position.Y += SharpHelper.RandomBetween(-10, 10);

                    if (healthLeft <= 0.2f)
                    {
                        Game.PlayEffect(Effect.FIRE, position);
                        Game.PlaySound("Flamethrower", position);
                    }
                    if (healthLeft <= 0.3f)
                    {
                        Game.PlayEffect(Effect.SPARKS, position);
                    }
                    if (healthLeft <= 0.4f)
                    {
                        if (SharpHelper.RandomBoolean())
                        {
                            Game.PlayEffect(Effect.STEAM, position);
                            Game.PlayEffect(Effect.STEAM, position);
                        }
                        Game.PlayEffect(Effect.ELECTRIC, position);
                        Game.PlaySound("ElectricSparks", position);
                    }
                    m_electricElapsed = 0f;
                }
                else
                {
                    m_electricElapsed -= SharpHelper.RandomBetween(0, m_electricElapsed);
                }
            }
        }

        public override void OnDamage()
        {
            var mod = Player.GetModifiers();
            var currentHealth = mod.CurrentHealth;
            var maxHealth = mod.MaxHealth;

            if (currentHealth / maxHealth <= 0.25f)
            {
                var position = Player.GetWorldPosition();
                Game.PlayEffect(Effect.ELECTRIC, position);
                Game.PlaySound("ElectricSparks", position);
            }
        }

        private bool m_hasDie = false;
        public override void OnDeath()
        {
            // Player.Remove() will call the death event one more time, make sure OnDeath() is only called once
            if (Player == null || m_hasDie) return;
            m_hasDie = true;

            var newPlayer = Game.CreatePlayer(Player.GetWorldPosition());

            Decorate(newPlayer);
            var newMod = newPlayer.GetModifiers();
            newMod.CurrentHealth = newMod.MaxHealth;

            newPlayer.SetModifiers(newMod);
            newPlayer.SetValidBotEliminateTarget(false);
            newPlayer.SetStatusBarsVisible(false);
            newPlayer.SetNametagVisible(false);
            newPlayer.SetFaceDirection(Player.GetFaceDirection());

            Player.Remove();
            Player = newPlayer;

            ScriptHelper.MakeInvisible(Player);
            StartDeathKneeling();
        }

        private bool m_isDeathKneeling = false;
        private void StartDeathKneeling()
        {
            if (Player == null) return;

            Player.ClearCommandQueue();
            Player.SetBotBehaviorActive(false);
            m_isDeathKneeling = true;
            Player.AddCommand(new PlayerCommand(PlayerCommandType.DeathKneelInfinite));
        }
        private void StopKneeling()
        {
            Player.AddCommand(new PlayerCommand(PlayerCommandType.StopDeathKneel));
            m_isDeathKneeling = false;
            Player.SetBotBehaviorActive(true);
        }

        private float m_kneelingTime = 0f;
        private bool m_hasShotGrenades = false;
        private void UpdateDealthKneeling(float elapsed)
        {
            if (Player.IsDeathKneeling)
            {
                m_kneelingTime += elapsed;

                if (m_kneelingTime >= 1000 && !m_hasShotGrenades)
                {
                    m_grenadeDirection = new Vector2(Player.GetFaceDirection(), 1f);

                    for (uint i = 1; i <= 3; i++)
                    {
                        Events.UpdateCallback.Start(ShootGrenades, 300 * i, 1);
                    }
                    m_hasShotGrenades = true;
                }

                if (m_kneelingTime >= 2500)
                {
                    StopKneeling();
                    Player.Kill();
                }
            }
            else
            {
                if (!m_hasShotGrenades)
                {
                    StartDeathKneeling();
                    m_kneelingTime = 0f;
                }
                else
                {
                    StopKneeling();
                    Player.Kill();
                }
            }
        }

        private Vector2 m_grenadeDirection;
        private void ShootGrenades(float elapsed)
        {
            Game.PlaySound("GLauncher", Player.GetWorldPosition());
            Game.SpawnProjectile(ProjectileItem.GRENADE_LAUNCHER, Player.GetWorldPosition() + new Vector2(-5, 20), m_grenadeDirection);
            m_grenadeDirection.X *= 2f;
        }
    }
}
