using SFDGameScriptInterface;
using System.Collections.Generic;

namespace SFDScript.BotExtended.Bots
{
    public class TeddybearBot : Bot
    {
        public bool IsHunting { get; set; }

        public override void OnSpawn(List<Bot> others)
        {
            IsHunting = false;

            if (others.Count >= 1) // has cults
                Player.SetBotName("Mommy Bear");
        }

        public void Help(IPlayer player)
        {
            var position = player.GetWorldPosition();
        }
    }

}
