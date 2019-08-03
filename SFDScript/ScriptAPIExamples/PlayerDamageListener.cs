using SFDGameScriptInterface;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3771
    /// <summary>
    /// The following code demonstrates how to listen on player damage events in v.1.3.0.
    /// </summary>
    public class PlayerDamageListener : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public PlayerDamageListener() : base(null) { }

        // Example script to read player damage 
        public void OnStartup()
        {
            Events.PlayerDeathCallback.Start(OnPlayerDeath);
            Events.PlayerDamageCallback.Start(OnPlayerDamage);
        }

        public void OnPlayerDamage(IPlayer player, PlayerDamageArgs args)
        {
            if (args.DamageType == PlayerDamageEventType.Melee && args.SourceID != 0)
            {
                IPlayer hitBy = Game.GetPlayer(args.SourceID);
                Game.WriteToConsole(string.Format("Player {0} took {1} melee damage from player {2}", player.UniqueID, args.Damage, hitBy.UniqueID));
            }
            else
            {
                Game.WriteToConsole(string.Format("Player {0} took {1} {2} damage", player.UniqueID, args.Damage, args.DamageType));
            }
        }

        public void OnPlayerDeath(IPlayer player, PlayerDeathArgs args)
        {
            // player just died or was removed (or both if falling outside the map while alive or gibbed while alive).
            if (args.Killed)
            {
                Game.WriteToConsole(string.Format("Player {0} died", player.UniqueID));
            }
            if (args.Removed)
            {
                Game.WriteToConsole(string.Format("Player {0} removed", player.UniqueID));
            }
        }
    }
}
