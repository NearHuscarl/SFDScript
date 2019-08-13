using SFDGameScriptInterface;
using SFDScript.Library;
using System;
using System.Collections.Generic;

namespace SFDScript.ScriptAPIExamples
{
    /// <summary>
    /// Add more metal effects when gibbed for players which have PlayerHitEffect.Metal
    /// </summary>
    public class DestroyAnimation : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public DestroyAnimation() : base(null) { }

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
            Game.RunCommand("ia 1");

            foreach (var player in Game.GetPlayers())
            {
                var mod = player.GetModifiers();
                mod.CanBurn = 0;
                player.SetModifiers(mod);
                player.SetHitEffect(PlayerHitEffect.Metal);
            }

            Events.PlayerDeathCallback.Start(OnPlayerDeath);
        }

        public void OnPlayerDeath(IPlayer player, PlayerDeathArgs args)
        {
            if (player.GetHitEffect() == PlayerHitEffect.Metal && args.Removed)
            {
                var deathPosition = player.GetWorldPosition();
                var effects = new List<Tuple<string, int>>() {
                    Tuple.Create(EffectName.BulletHitMetal, 1),
                    Tuple.Create(EffectName.Steam, 2),
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

                Game.TriggerExplosion(deathPosition);

                for (var i = 0; i < 4; i++)
                {
                    var debrisLinearVelocity = RandomHelper.Direction(15, 165) * 10;
                    var debris = Game.CreateObject(RandomHelper.GetItem(DebrisList),
                        deathPosition,
                        0f,
                        debrisLinearVelocity,
                        0f);
                    debris.SetMaxFire();

                    Game.CreateObject(RandomHelper.GetItem(DebrisList),
                        deathPosition,
                        0f,
                        debrisLinearVelocity * -Vector2.UnitX,
                        0f);

                    if (RandomHelper.Between(0, 100) < 50)
                    {
                        Game.CreateObject(RandomHelper.GetItem(WiringTubeList),
                            deathPosition,
                            0f,
                            RandomHelper.Direction(0, 180) * 6,
                            0f);
                    }
                }
            }
        }
    }
}
