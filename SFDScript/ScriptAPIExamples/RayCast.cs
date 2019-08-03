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

        // Example script how to RayCast in the world from a player and show some debugging information about the hit objects while testing the map in the editor.
        public void OnStartup()
        {
            Events.UpdateCallback.Start(OnUpdate, 0);
        }

        public void OnUpdate(float ms)
        {
            IUser[] users = Game.GetActiveUsers();
            IPlayer plr = (users != null && users.Length > 0 ? users[0].GetPlayer() : null); // any player instance

            if (plr != null)
            {
                Vector2 worldPos = plr.GetWorldPosition() + Vector2.UnitY * 12f;
                Vector2 worldPosEnd = worldPos + plr.AimVector * 100f;

                if (Game.IsEditorTest)
                {
                    Game.DrawLine(worldPos, worldPosEnd, Color.Red);
                }

                // RayCastInput offers some filtering capabilities, for now just filter on everything with a set CategoryBit (effectively ignoring background objects).
                RayCastInput rci = new RayCastInput() { IncludeOverlap = true, MaskBits = 0xFFFF, FilterOnMaskBits = true };
                // You can also filter on specific types. If you only want to raycast players you would add the IPlayer type to the Types array property: rci.Types = new Type[1] { typeof(IPlayer) };
                RayCastResult[] results = Game.RayCast(worldPos, worldPosEnd, rci);
                foreach (RayCastResult result in results)
                {

                    if (Game.IsEditorTest)
                    {
                        Game.DrawCircle(result.Position, 1f, Color.Yellow);
                        Game.DrawLine(result.Position, result.Position + result.Normal * 5f, Color.Yellow);
                        Game.DrawArea(result.HitObject.GetAABB(), Color.Yellow);
                        Game.DrawText(result.HitObject.UniqueID.ToString(), result.Position, Color.Yellow);
                    }

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
