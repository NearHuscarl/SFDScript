using SFDGameScriptInterface;
using SFDScript.Library;
using System;
using System.Collections.Generic;

namespace SFDScript.ScriptAPIExamples
{
    /// <summary>
    /// Usually when received enough overkill damage, player burst into pieces with blood and gore effects
    /// This script replace the blood and gore with metal debris and other explosive effects for robot-like
    /// character like Mecha in the campaign
    /// </summary>
    public class DestroyAnimation : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public DestroyAnimation() : base(null) { }

        private Area m_gibArea;
        private bool m_metalGibbing = false;
        private const int GIB_AREA_SIZE = 15;

        private readonly List<string> DebrisList = new List<string> {
            "MetalDebris00A",
            "MetalDebris00B",
            "MetalDebris00C",
            "MetalDebris00D",
            "MetalDebris00E",
            "ItemDebrisDark00",
            "ItemDebrisDark01",
            "ItemDebrisShiny00",
            "ItemDebrisShiny01",
        };
        private readonly List<string> WiringTubeList = new List<string> {
            "WiringTube00A",
            "WiringTube00A_D",
            "WiringTube00B",
        };

        public void OnStartup()
        {
            Game.GetPlayers()[0].GiveWeaponItem(WeaponItem.M60);

            foreach (var player in Game.GetPlayers())
            {
                var mod = player.GetModifiers();
                mod.CanBurn = 0;
                player.SetModifiers(mod);
                player.SetHitEffect(PlayerHitEffect.Metal);
            }

            Events.PlayerDeathCallback.Start(OnPlayerDeath);
            Events.ObjectCreatedCallback.Start(OnObjectCreated);
            Events.ObjectTerminatedCallback.Start(OnObjectTerminated);
        }

        private void OnObjectCreated(IObject[] objs)
        {
            if (m_metalGibbing)
            {
                foreach (var obj in objs)
                {
                    if (obj.Name.StartsWith("Gib") && m_gibArea.Contains(obj.GetWorldPosition()))
                    {
                        var debris = Game.CreateObject(RandomHelper.GetItem(DebrisList),
                            obj.GetWorldPosition(),
                            obj.GetAngle(),
                            obj.GetLinearVelocity() * 2,
                            obj.GetAngularVelocity());

                        var debris2 = Game.CreateObject(RandomHelper.GetItem(DebrisList),
                            obj.GetWorldPosition(),
                            -obj.GetAngle(),
                            -obj.GetLinearVelocity(),
                            obj.GetAngularVelocity());

                        if (RandomHelper.Between(0, 100) < 50)
                        {
                            Game.CreateObject(RandomHelper.GetItem(WiringTubeList),
                                obj.GetWorldPosition(),
                                obj.GetAngle(),
                                RandomHelper.Direction(15, 165) * 20,
                                -obj.GetAngularVelocity());
                        }

                        debris.SetMaxFire();
                        obj.Remove();
                    }
                }
                m_metalGibbing = false;
            }
        }

        public void OnPlayerDeath(IPlayer player, PlayerDeathArgs args)
        {
            if (args.Removed)
            {
                var pos = player.GetWorldPosition();
                var bottomLeft = new Vector2(pos.X - (GIB_AREA_SIZE / 2), pos.Y - (GIB_AREA_SIZE / 2));
                var topRight = new Vector2(pos.X + (GIB_AREA_SIZE / 2), pos.Y + (GIB_AREA_SIZE / 2));
                m_gibArea = new Area(bottomLeft, topRight);

                var effects = new List<Tuple<string, int>>() {
                    Tuple.Create(EffectName.BulletHitMetal, 3),
                    Tuple.Create(EffectName.Explosion, 1),
                    Tuple.Create(EffectName.Steam, 3),
                    Tuple.Create(EffectName.Electric, 4),
                };

                foreach (var effect in effects)
                {
                    var effectName = effect.Item1;
                    var count = effect.Item2;

                    for (var i = 0; i < count; i++)
                    {
                        var position = player.GetWorldPosition();
                        position.X += RandomHelper.Between(-10, 10);
                        position.Y += RandomHelper.Between(-10, 10);
                        Game.PlayEffect(effectName, position);
                    }
                }

                Game.TriggerExplosion(player.GetWorldPosition());
                m_metalGibbing = true;
            }
        }

        public void OnObjectTerminated(IObject[] objs) { }
    }
}
