using SFDGameScriptInterface;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3765
    /// <summary>
    /// In this part I will illustrate in the code how to manipulate player item ammo introduced in the 1.3.0 update.
    /// </summary>
    public class AmmoManagement : GameScriptInterface
    {
        public AmmoManagement() : base(null) { }

        // Example script to give the last round in every magazine the ability to bounce if no powerup is already active.
        public void OnStartup()
        {
            var player = Game.GetPlayers()[0];

            player.GiveWeaponItem(WeaponItem.PISTOL);
            player.GiveWeaponItem(WeaponItem.CARBINE);

            var mod = player.GetModifiers();
            mod.InfiniteAmmo = 1;
            player.SetModifiers(mod);

            Events.UpdateCallback.Start(OnUpdate, 0);

            IPlayer plr = Game.GetPlayers()[0]; // any player instance

            // Example to set 5 rounds to the current primary weapon.
            plr.SetCurrentPrimaryWeaponAmmo(5);

            // Example to set primary weapon with a full magazine, loaded and ready.
            plr.SetCurrentPrimaryWeaponAmmo(plr.CurrentPrimaryWeapon.MagSize * plr.CurrentPrimaryWeapon.WeaponMagCapacity, 0);

            // Example to set one extra set of rounds (one spare mag or one set of extra shells for shotguns) for a weapon. 
            // The weapon must be reloaded after setting this.
            plr.SetCurrentPrimaryWeaponAmmo(0, plr.CurrentPrimaryWeapon.WeaponMagCapacity);

            // Example to set 6 bouncing rounds to the weapon. Note: Only one powerup ammo type can be active at any time for one weapon.
            plr.SetCurrentPrimaryWeaponAmmo(6, ProjectilePowerup.Bouncing);

            // Example to stop powerup rounds to the weapon.
            plr.SetCurrentPrimaryWeaponAmmo(0, ProjectilePowerup.Bouncing);


            // Example to set 2 thrown items of current throwable item
            plr.SetCurrentThrownItemAmmo(2);

            // Example to remove current thrown item.
            plr.SetCurrentThrownItemAmmo(0);


            // Example to restore melee weapon durability to 100%
            plr.SetCurrentMeleeDurability(1f);
            plr.SetCurrentMeleeMakeshiftDurability(1f);

            // Example to set durability to 0 for current melee weapon
            plr.SetCurrentMeleeDurability(0f);
        }

        public void OnUpdate(float ms)
        {
            foreach (IPlayer plr in Game.GetPlayers())
            {
                RifleWeaponItem rifle = plr.CurrentPrimaryWeapon;
                if (rifle.PowerupBouncingRounds == 0 && rifle.PowerupFireRounds == 0)
                {
                    // WeaponMagCapacity > 1 indicates it's a shotgun.
                    // Let just the very last shell in shotguns be bouncing.
                    if (rifle.WeaponMagCapacity == 1 && rifle.CurrentAmmo == 1 || rifle.TotalAmmo == 1)
                    {
                        plr.SetCurrentPrimaryWeaponAmmo(1, ProjectilePowerup.Bouncing);
                    }
                }
                // Same logic for secondary weapons
                HandgunWeaponItem handgun = plr.CurrentSecondaryWeapon;
                if (handgun.PowerupBouncingRounds == 0 && handgun.PowerupFireRounds == 0)
                {
                    // WeaponMagCapacity > 1 indicates it's a shotgun.
                    // Let just the very last shell in shotguns be bouncing.
                    if (handgun.WeaponMagCapacity == 1 && handgun.CurrentAmmo == 1 || handgun.TotalAmmo == 1)
                    {
                        plr.SetCurrentSecondaryWeaponAmmo(1, ProjectilePowerup.Bouncing);
                    }
                }
            }
        }

    }
}
