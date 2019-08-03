using SFDGameScriptInterface;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3773
    /// <summary>
    /// The following code demonstrates how to listen on triggered explosions in v.1.3.0.
    /// </summary>
    public class ExplosionListener : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public ExplosionListener() : base(null) { }

        public void OnStartup()
        {
            var player = Game.GetPlayers()[0];

            player.GiveWeaponItem(WeaponItem.GRENADES);
            player.GiveWeaponItem(WeaponItem.GRENADE_LAUNCHER);

            var mod = player.GetModifiers();
            mod.InfiniteAmmo = 1;
            player.SetModifiers(mod);

            Events.ExplosionHitCallback.Start(OnExplosionHit);
        }
        public void OnExplosionHit(ExplosionData explosion, ExplosionHitArg[] args)
        {
            // explosion triggered
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].HitType)
                {
                    case ExplosionHitType.Damage:
                        Game.WriteToConsole(string.Format("Explosion {0} hit {1} {2} for {3} damage", explosion.InstanceID, (args[i].IsPlayer ? "player" : "object"), args[i].ObjectID, args[i].Damage));
                        break;
                    case ExplosionHitType.Shockwave:
                        Game.WriteToConsole(string.Format("Explosion {0} pushed {1} {2}", explosion.InstanceID, (args[i].IsPlayer ? "player" : "object"), args[i].ObjectID));
                        break;
                    case ExplosionHitType.None:
                        Game.WriteToConsole(string.Format("Explosion {0} overlapped {1} {2}", explosion.InstanceID, (args[i].IsPlayer ? "player" : "object"), args[i].ObjectID));
                        break;
                }
            }
        }

    }
}
