using SFDGameScriptInterface;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3770
    /// <summary>
    /// The following code demonstrates how to manipulate projectiles and listen on projectile hit events in v.1.3.0.
    /// 
    /// You can do all kinds of interesting things with projectiles. But changing position and velocity too often and
    /// too suddenly can make the projectiles look jittery and buggy on clients if the client have a fluctuating ping
    /// (which makes the client-side prediction fail - it can't predict what you want to do in your code :P). It's
    /// just how it is. Keep that in mind when testing your code in the editor vs a public game.
    /// </summary>
    public class ProjectileManipulation : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public ProjectileManipulation() : base(null) { }

        // Example script to manipulate projectiles
        public void OnStartup()
        {
            var player = Game.GetPlayers()[0];

            player.GiveWeaponItem(WeaponItem.BAZOOKA);
            player.GiveWeaponItem(WeaponItem.SHOTGUN);
            player.GiveWeaponItem(WeaponItem.PISTOL);

            var mod = player.GetModifiers();
            mod.InfiniteAmmo = 1;
            player.SetModifiers(mod);

            Events.UpdateCallback.Start(OnUpdate, 0);
            Events.ProjectileHitCallback.Start(OnProjectileHit);
        }

        public void OnProjectileHit(IProjectile projectile, ProjectileHitArgs args)
        {
            Game.WriteToConsole(string.Format("Projectile {0} hit {1} {2} for {3} damage", projectile.InstanceID, (args.IsPlayer ? "player" : "object"), args.HitObjectID, args.Damage));
        }

        public void OnUpdate(float ms)
        {
            foreach (IProjectile proj in Game.GetProjectiles())
            {
                // lower velocity for bazooka rockets to 300
                if (proj.ProjectileItem == ProjectileItem.BAZOOKA)
                {
                    if (proj.Velocity.Length() > 201f)
                    {
                        proj.Velocity = proj.Direction * 200f;
                    }
                }
                // shotguns can only reach 200 world units
                if (proj.ProjectileItem == ProjectileItem.SHOTGUN || proj.ProjectileItem == ProjectileItem.DARK_SHOTGUN)
                {
                    if (proj.TotalDistanceTraveled > 100f)
                    {
                        proj.FlagForRemoval();
                    }
                }

                // pistols rounds affected by gravity
                if (proj.ProjectileItem == ProjectileItem.PISTOL)
                {
                    proj.Velocity = new Vector2(proj.Velocity.X, proj.Velocity.Y - 4f * ms);
                }
            }
        }
    }
}
