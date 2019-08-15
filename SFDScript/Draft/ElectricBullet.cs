using SFDGameScriptInterface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFDScript.Draft
{
    public class ElectricBullet : GameScriptInterface
    {
        private class StunnedPlayer
        {
            public static readonly float StunnedTime = 3000f;
            private IPlayer m_player;

            public int UniqueID
            {
                get { return m_player.UniqueID; }
            }

            private bool isStunned;
            public bool IsStunned
            {
                get
                {
                    return isStunned;
                }
                private set
                {
                    isStunned = value;
                    if (isStunned)
                    {
                        m_player.SetInputEnabled(false);
                        m_player.AddCommand(new PlayerCommand(PlayerCommandType.DeathKneelInfinite));
                    }
                    else
                    {
                        m_player.AddCommand(new PlayerCommand(PlayerCommandType.StopDeathKneel));
                        m_player.SetInputEnabled(true);
                    }
                }
            }

            public StunnedPlayer(IPlayer player) : this(player, player.GetWorldPosition())
            {
            }

            public StunnedPlayer(IPlayer player, Vector2 hitPosition)
            {
                m_player = player;
                IsStunned = true;

                Game.PlayEffect(EffectName.Electric, hitPosition);
                Game.PlaySound("ElectricSparks", hitPosition);

                if (rnd.Next(0, 100) < 30)
                {
                    Game.SpawnFireNode(hitPosition, Vector2.Zero);
                    Game.PlayEffect(EffectName.FireTrail, hitPosition);
                }
            }

            private float m_stunnedTimer = 0;
            private float m_stunnedEffectTimer = 0;
            public void Update(float dt)
            {
                if (IsStunned)
                {
                    m_stunnedTimer += dt;
                    m_stunnedEffectTimer += dt;

                    if (m_stunnedEffectTimer >= 400)
                    {
                        var position = m_player.GetWorldPosition();
                        position.X += rnd.Next(-10, 10);
                        position.Y += rnd.Next(-10, 10);

                        Game.PlayEffect(EffectName.Electric, position);
                        m_stunnedEffectTimer = 0;
                    }

                    if (m_stunnedTimer >= StunnedTime)
                    {
                        IsStunned = false;
                    }
                }
            }
        }

        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public ElectricBullet() : base(null) { }

        private static Random rnd = new Random();
        private Dictionary<int, IProjectile> m_customBullets = new Dictionary<int, IProjectile>();
        private static Dictionary<int, StunnedPlayer> m_stunnedPlayers = new Dictionary<int, StunnedPlayer>();

        public void OnStartup()
        {
            var me = Game.GetPlayers()[0];
            me.GiveWeaponItem(WeaponItem.PISTOL);
            me.GiveWeaponItem(WeaponItem.M60);
            var mod = me.GetModifiers();
            mod.MeleeStunImmunity = 1;
            me.SetModifiers(mod);

            Game.RunCommand("ia 1");
            Game.RunCommand("ih 1");

            foreach (var player in Game.GetPlayers())
            {
                if (player == me) continue;

                var bot = Game.CreatePlayer(player.GetWorldPosition());
                bot.SetBotName("bot " + rnd.Next(0, 1000));
                bot.SetBotBehaviorSet(BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotD));
                bot.SetBotBehaviorActive(true);
                player.Remove();
            }


            Events.UpdateCallback.Start(OnUpdate);
            Events.ProjectileCreatedCallback.Start(OnProjectileCreated);
            Events.ProjectileHitCallback.Start(OnProjectileHit);
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
                        ToEMPBullet(projectile);
                        break;
                }
            }
        }

        private void OnProjectileHit(IProjectile projectile, ProjectileHitArgs args)
        {
            if (m_customBullets.ContainsKey(projectile.InstanceID))
            {
                var rndNum = rnd.Next(0, 100);
                if (rndNum < 1)
                {
                    ElectrocuteRange(args.HitPosition);
                }
                if (1 <= rndNum && rndNum < 21)
                {
                    var obj = Game.GetObject(args.HitObjectID);
                    Electrocute(args);
                }

                m_customBullets.Remove(projectile.InstanceID);
            }
        }

        private Vector2? m_empBlastCenter = null;
        public static readonly float EMPBlastRadius = 15f;
        private float m_stunnedRangeTimer = 0f;
        private void ElectrocuteRange(Vector2 position)
        {
            foreach (var player in Game.GetPlayers())
            {
                if (IsTouchingCircle(player.GetAABB(), position, EMPBlastRadius) && CanBeStunned(player))
                {
                    Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(), "stunned");
                    m_stunnedPlayers.Add(player.UniqueID, new StunnedPlayer(player));
                }
            }

            for (var i = 0; i < 360; i += 72) // Play electric effect 5 times in circle (360 / 5 = 72)
            {
                var degree = i;
                var radianAngle = degree * Math.PI / 180.0f;

                var direction = new Vector2()
                {
                    X = (float)Math.Cos(radianAngle),
                    Y = (float)Math.Sin(radianAngle),
                };

                Game.PlayEffect(EffectName.Electric, position + direction * EMPBlastRadius);
                Game.PlaySound("ElectricSparks", position);
            }

            m_empBlastCenter = position;
        }

        private void Electrocute(ProjectileHitArgs args)
        {
            var position = args.HitPosition;

            if (args.IsPlayer)
            {
                var player = (IPlayer)Game.GetObject(args.HitObjectID);

                if (CanBeStunned(player))
                {
                    Game.PlayEffect(EffectName.CustomFloatText, position, "stunned");
                    m_stunnedPlayers.Add(player.UniqueID, new StunnedPlayer(player, position));
                }
            }
        }

        private bool CanBeStunned(IPlayer player)
        {
            return !player.IsRemoved && !player.IsDead && !m_stunnedPlayers.ContainsKey(player.UniqueID);
        }

        private void OnUpdate(float dt)
        {
            UpdatePlayers(dt);

            m_stunnedRangeTimer += dt;
            if (m_stunnedRangeTimer >= StunnedPlayer.StunnedTime)
            {
                m_stunnedRangeTimer = 0;
                m_empBlastCenter = null;
            }
            if (m_empBlastCenter != null)
                Game.DrawCircle((Vector2)m_empBlastCenter, EMPBlastRadius, Color.Red);
        }

        private void UpdatePlayers(float dt)
        {
            foreach (var player in m_stunnedPlayers.Values.ToList())
            {
                player.Update(dt);

                if (!player.IsStunned)
                {
                    m_stunnedPlayers.Remove(player.UniqueID);
                }
            }
        }

        private void ToEMPBullet(IProjectile projectile)
        {
            if (!m_customBullets.ContainsKey(projectile.InstanceID))
            {
                m_customBullets.Add(projectile.InstanceID, projectile);
            }
        }

        private bool IsTouchingCircle(Area area, Vector2 center, float radius)
        {
            var lines = new List<Vector2[]>()
            {
                new Vector2[] { area.BottomRight, area.BottomLeft },
                new Vector2[] { area.BottomLeft, area.TopLeft },
                new Vector2[] { area.TopLeft, area.TopRight },
                new Vector2[] { area.TopRight, area.BottomRight },
            };

            var minDistanceToCenter = float.MaxValue;

            foreach (var line in lines)
            {
                var distanceToCenter = FindDistanceToSegment(center, line[0], line[1]);
                if (distanceToCenter < minDistanceToCenter) minDistanceToCenter = distanceToCenter;
            }

            return minDistanceToCenter <= radius;
        }

        // https://stackoverflow.com/a/1501725/9449426
        private float FindDistanceToSegment(Vector2 point, Vector2 p1, Vector2 p2)
        {
            // Return minimum distance between line segment vw and point point
            var lengthSquare = (float)(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));  // i.e. |p2-p1|^2 -  avoid a sqrt
            if (lengthSquare == 0.0) return Vector2.Distance(point, p1);   // p1 == p2 case
            // Consider the line extending the segment, parameterized as p1 + t (p2 - p1).
            // We find projection of point point onto the line. 
            // It falls where t = [(point-p1) . (p2-p1)] / |p2-p1|^2
            // We clamp t from [0,1] to handle points outside the segment vw.
            var t = MathHelper.Clamp(Vector2.Dot(point - p1, p2 - p1) / lengthSquare, 0, 1);
            var projection = p1 + t * (p2 - p1);  // Projection falls on the segment
            return Vector2.Distance(point, projection);
        }
    }
}
