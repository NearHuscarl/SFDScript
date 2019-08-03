using SFDGameScriptInterface;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3779
    /// <summary>
    /// To perform a HitTest on any IObject simply use the bool IObject.HitTest(Vector2 position) function.
    /// You can also perform RayCasts on individual IObjects.
    /// </summary>
    public class HitTest : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public HitTest() : base(null) { }

        public void OnStartup()
        {
            var players = Game.GetPlayers();
            IObject obj = players.Length > 0 ? players[0] : null; // any IObject instance

            point = obj.GetWorldPosition() - Vector2.UnitX * 10f;
            pA = obj.GetWorldPosition() + Vector2.UnitX * 10f;
            pB = pA + Vector2.UnitX * 100f;

            Events.UpdateCallback.Start(OnUpdate, 0);
        }

        private Vector2 point;
        private Vector2 pA;
        private Vector2 pB;

        public void OnUpdate(float ms)
        {
            var players = Game.GetPlayers();
            IObject obj = players.Length > 0 ? players[0] : null; // any IObject instance

            Game.DrawCircle(point, 1f, Color.Magenta);

            if (obj.HitTest(point))
            {
                Game.DrawText("Hit a point", obj.GetWorldPosition(), Color.Yellow);
            }

            Game.DrawLine(pA, pB, Color.Magenta);

            if (obj.RayCast(pA, pB, true).Hit)
            {
                // Object hit between pA and pB
                Game.DrawText("Hit a line", obj.GetWorldPosition(), Color.Yellow);
            }
        }
    }
}
