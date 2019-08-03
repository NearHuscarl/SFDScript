using SFDGameScriptInterface;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3775
    /// <summary>
    /// The following code demonstrates how to listen on player melee action events in v.1.3.0.
    /// </summary>
    public class MeleeActionListener : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public MeleeActionListener() : base(null) { }

        public void OnStartup()
        {
            Events.PlayerMeleeActionCallback.Start(OnPlayerMeleeAction);
        }

        public void OnPlayerMeleeAction(IPlayer player, PlayerMeleeHitArg[] args)
        {
            // player performed a melee action. args contains all hit objects (if any).
            Game.WriteToConsole(string.Format("Player {0} hit {1} objects during melee {2}", player.UniqueID, args.Length, (player.IsKicking ? "kick" : "attack")));
            foreach (PlayerMeleeHitArg arg in args)
            {
                Game.WriteToConsole(string.Format("Player {0} hit object {1} for {2} damage", player.UniqueID, arg.HitObject.UniqueID, arg.HitDamage));
            }
        }
    }
}