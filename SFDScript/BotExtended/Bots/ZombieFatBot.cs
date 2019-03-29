using static SFDScript.Library.Mocks.MockObjects;

namespace SFDScript.BotExtended.Bots
{
    public class ZombieFatBot : Bot
    {
        public override void OnDeath()
        {
            Game.TriggerExplosion(Player.GetWorldPosition());
        }
    }
}
