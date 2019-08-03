using SFDGameScriptInterface;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3766
    /// <summary>
    /// The following code demonstrates how to use the local storage feature.
    /// </summary>
    public class StorageTestBed : GameScriptInterface
    {
        public StorageTestBed() : base(null) { }

        int timesStarted = 0;

        public void OnStartup()
        {
            if (!Game.LocalStorage.TryGetItemInt("timesStarted", out timesStarted))
            {
                timesStarted = 0;
            }
            timesStarted += 1;
            Game.LocalStorage.SetItem("timesStarted", timesStarted);
            Game.ShowPopupMessage("Times started: " + timesStarted.ToString());
        }

        public void OnShutdown()
        {
            Game.LocalStorage.SetItem("timesEnded", timesStarted);
        }
    }
}
