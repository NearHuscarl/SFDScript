using SFDGameScriptInterface;

namespace SFDScript.ColorPallete
{
    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        #region Startup Calls

        // https://www.mythologicinteractiveforums.com/viewtopic.php?f=33&t=1725&p=11003&hilit=profile#p11003
        public void OnStartup()
        {
            string paletteName = Game.GetClothingItemColorPaletteName("Belt");
            ColorPalette colorPalette = Game.GetColorPalette(paletteName);

            if (colorPalette != null)
            {
                // read available colors from string[] colorPalette.PrimaryColorPackages, colorPalette.SecondaryColorPackages, colorPalette.TertiaryColorPackages if needed.

                // iterate over all availalbe PrimaryColors
                foreach (string primaryColor in colorPalette.PrimaryColorPackages)
                {
                    Game.WriteToConsole(primaryColor);
                }
            }
        }

        #endregion
    }
}
