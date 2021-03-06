using SFDGameScriptInterface;
using System;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3779
    /// <summary>
    /// The following code demonstrates how to perform RayCasts in the world between two points. Available in v.1.3.0.
    /// </summary>
    public class RayCast : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public RayCast() : base(null) { }

        // Example script how to RayCast in the world from a player and show some debugging information
        // about the hit objects while testing the map in the editor.
        public void OnStartup()
        {
            foreach (var p in Game.GetPlayers())
            {
                p.GiveWeaponItem(WeaponItem.M60);
            }

            Events.UpdateCallback.Start(OnUpdate, 0);
            Events.PlayerKeyInputCallback.Start(OnKeyInput);
        }

        private IPlayer GetPlayer()
        {
            foreach (var user in Game.GetActiveUsers())
            {
                return user.GetPlayer();
            }
            return null;
        }

        // RayCastInput offers some filtering capabilities, for now just filter on everything with
        // a set CategoryBit (effectively ignoring background objects).
        private RayCastInput m_rcInput = new RayCastInput()
        {
            IncludeOverlap = true,
            MaskBits = 0xFFFF,
            FilterOnMaskBits = true
        };

        private RayCastFilterMode Cycle(RayCastFilterMode mode)
        {
            if (mode == RayCastFilterMode.True)
                return RayCastFilterMode.False;
            if (mode == RayCastFilterMode.False)
                return RayCastFilterMode.Any;
            return RayCastFilterMode.True;
        }

        private void OnKeyInput(IPlayer player, VirtualKeyInfo[] keyInfos)
        {
            foreach (var keyInfo in keyInfos)
            {
                if (keyInfo.Event == VirtualKeyEvent.Pressed)
                {
                    switch (keyInfo.Key)
                    {
                        case VirtualKey.DRAW_MELEE:
                            m_rcInput.BlockExplosions = Cycle(m_rcInput.BlockExplosions);
                            Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(),
                                "BlockExplosions = " + m_rcInput.BlockExplosions);
                            break;
                        case VirtualKey.DRAW_HANDGUN:
                            m_rcInput.BlockFire = Cycle(m_rcInput.BlockFire);
                            Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(),
                                "BlockFire = " + m_rcInput.BlockFire);
                            break;
                        case VirtualKey.DRAW_RIFLE:
                            m_rcInput.BlockMelee = Cycle(m_rcInput.BlockMelee);
                            Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(),
                                "BlockMelee = " + m_rcInput.BlockMelee);
                            break;
                        case VirtualKey.DRAW_GRENADE:
                            m_rcInput.ProjectileHit = Cycle(m_rcInput.ProjectileHit);
                            Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(),
                                "ProjectileHit = " + m_rcInput.ProjectileHit);
                            break;
                        case VirtualKey.DRAW_SPECIAL:
                            m_rcInput.AbsorbProjectile = Cycle(m_rcInput.AbsorbProjectile);
                            Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(),
                                "AbsorbProjectile = " + m_rcInput.AbsorbProjectile);
                            break;

                        case VirtualKey.BLOCK:
                            m_rcInput.ClosestHitOnly = !m_rcInput.ClosestHitOnly;
                            Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(),
                                "ClosestHitOnly = " + m_rcInput.ClosestHitOnly);
                            break;
                        case VirtualKey.ATTACK:
                            m_rcInput.IncludeOverlap = !m_rcInput.IncludeOverlap;
                            Game.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(),
                                "IncludeOverlap = " + m_rcInput.IncludeOverlap);
                            break;
                    }
                }
            }
        }

        private void OnUpdate(float ms)
        {
            var player = GetPlayer();
            if (player == null) return;

            Vector2 position, direction, worldPos, worldPosEnd;

            if (player.GetWeaponMuzzleInfo(out position, out direction))
            {
                worldPos = position;
                worldPosEnd = position + direction * 100f;
            }
            else
            {
                worldPos = player.GetWorldPosition() + Vector2.UnitY * 12f;
                worldPosEnd = worldPos + player.AimVector * 100f;
            }

            Game.DrawLine(worldPos, worldPosEnd, Color.Red);

            // You can also filter on specific types. If you only want to raycast players you would add
            // the IPlayer type to the Types array property: rci.Types = new Type[1] { typeof(IPlayer) };
            RayCastResult[] results = Game.RayCast(worldPos, worldPosEnd, m_rcInput);
            foreach (RayCastResult result in results)
            {
                Game.DrawCircle(result.Position, 1f, Color.Yellow);
                Game.DrawLine(result.Position, result.Position + result.Normal * 5f, Color.Yellow);

                if (result.HitObject != null)
                {
                    Game.DrawArea(result.HitObject.GetAABB(), Color.Yellow);
                    Game.DrawText(result.HitObject.UniqueID.ToString(), result.Position, Color.Yellow);

                    // Destroy nearby glass that the player is looking at
                    if (result.Fraction < 0.3f && result.HitObject.Name.IndexOf("glass", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        result.HitObject.Destroy();
                    }
                }
            }
        }
    }
}