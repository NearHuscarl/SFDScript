using System;
using SFDGameScriptInterface;
using static SFDScript.Library.Mocks.MockObjects;

namespace SFDScript.Library
{
    public static class ScriptHelper
    {
        public static readonly Color MESSAGE_COLOR = new Color(24, 238, 200);
        public static readonly Color ERROR_COLOR = new Color(244, 77, 77);
        public static readonly Color WARNING_COLOR = new Color(249, 191, 11);

        public static void PrintMessage(string message, Color? color = null)
        {
            if (color == null) color = MESSAGE_COLOR;
            Game.ShowChatMessage(message, (Color)color);
        }

        public static bool IsElapsed(float timeStarted, float timeToElapse)
        {
            return Game.TotalElapsedGameTime - timeStarted >= timeToElapse;
        }

        public static bool SpawnPlayerHasPlayer(IObject spawnPlayer)
        {
            // Player position y: -20 || +9
            // => -21 -> +10
            // Player position x: unchange
            foreach (var player in Game.GetPlayers())
            {
                var playerPosition = player.GetWorldPosition();
                var spawnPlayerPosition = spawnPlayer.GetWorldPosition();

                if (spawnPlayerPosition.Y - 21 <= playerPosition.Y && playerPosition.Y <= spawnPlayerPosition.Y + 10
                    && spawnPlayerPosition.X == playerPosition.X)
                    return true;
            }

            return false;
        }

        public static void MakeInvisible(IPlayer player)
        {
            if (player != null && !player.IsDead)
            {
                var mod = player.GetModifiers();
                mod.FireDamageTakenModifier = 0;
                mod.ImpactDamageTakenModifier = 0;
                mod.MeleeDamageTakenModifier = 0;
                mod.ExplosionDamageTakenModifier = 0;
                mod.ProjectileDamageTakenModifier = 0;
                player.SetModifiers(mod);
            }
        }

        public static bool IsDifferentTeam(IPlayer player1, IPlayer player2)
        {
            return player1.GetTeam() != player2.GetTeam() || player1.GetTeam() == PlayerTeam.Independent;
        }

        public static Skin GetSkin(IPlayer player)
        {
            var skinName = player.GetProfile().Skin.Name;

            switch(skinName)
            {
                case "Normal":
                    return Skin.NormalMale;
                case "Normal_fem":
                    return Skin.NormalFemale;
                case "Tattoos":
                    return Skin.TatoosMale;
                case "Tattoos_fem":
                    return Skin.TatoosFemale;
                case "BearSkin":
                    return Skin.Bear;
                case "FrankenbearSkin":
                    return Skin.Frankenbear;
                case "MechSkin":
                    return Skin.Mech;
                case "Warpaint":
                    return Skin.WarpaintMale;
                case "Warpaint_fem":
                    return Skin.WarpaintFemale;
                case "Zombie":
                    return Skin.ZombieMale;
                case "Zombie_fem":
                    return Skin.ZombieFemale;
                default:
                    return Skin.None;
            }
        }

        // take into account PlayerModifiers.SizeModifier. Not 100% accurate
        public static Area GetAABB(IPlayer player)
        {
            var aabb = player.GetAABB();
            var sizeModifier = player.GetModifiers().SizeModifier;
            var newWidth = aabb.Width * sizeModifier;
            var newHeight = aabb.Height * sizeModifier;

            aabb.Left -= (newWidth - aabb.Width) / 2;
            aabb.Right += (newWidth - aabb.Width) / 2;
            aabb.Top += (newHeight - aabb.Height) / 2;
            aabb.Bottom -= (newHeight - aabb.Height) / 2;

            return aabb;
        }
    }
}
