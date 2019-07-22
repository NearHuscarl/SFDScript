using SFDGameScriptInterface;
using SFDScript.Library;
using System.Collections.Generic;
using static SFDScript.Library.Mocks.MockObjects;

namespace SFDScript.BotExtended.Bots
{
    public class BabybearBot : Bot
    {
        private TeddybearBot m_mommy = null;
        private static Queue<string> Names = new Queue<string>(new[] { "Timmy", "Jimmy" });
        public override void OnSpawn(List<Bot> others)
        {
            UpdateInterval = 0;

            foreach (var bot in others)
            {
                if (bot.Type == BotType.Teddybear)
                {
                    m_mommy = (TeddybearBot)bot;
                    break;
                }
            }
            if (m_mommy.Player == null) return;

            Player.SetBotName(Names.Dequeue());

            var behavior = Player.GetBotBehaviorSet();
            behavior.RangedWeaponUsage = false;
            behavior.SearchForItems = false;
            behavior.OffensiveClimbingLevel = 0.9f;
            behavior.OffensiveSprintLevel = 0.85f;
            behavior.GuardRange = 16;
            behavior.ChaseRange = 16;
            Player.SetBotBehaviorSet(behavior);

            Player.SetGuardTarget(m_mommy.Player);
            m_previousHealth = Player.GetHealth();
        }

        private readonly float DamageRequiredForMomToHunt = 20f;
        private float m_previousHealth = 0f;
        private float m_damageTaken = 0f;
        protected override void OnUpdate(float elapsed)
        {
            base.OnUpdate(elapsed);

            m_damageTaken += (m_previousHealth - Player.GetHealth());

            if (m_damageTaken >= DamageRequiredForMomToHunt)
            {
                m_damageTaken = 0f;

                IPlayer culprit = null;
                foreach (var player in Game.GetPlayers())
                {
                    if (ScriptHelper.IsHiting(player, Player))
                    {
                        culprit = player;
                        break;
                    }
                }
                m_mommy.Enrage(culprit, 10000);
            }

            m_previousHealth = Player.GetHealth();
        }

        public override void OnDamage()
        {
            var players = Game.GetPlayers();

            foreach (var player in players)
            {
                if (ScriptHelper.IsAiming(player, Player))
                    Game.CreateDialogue("Is aimed by " + player.Name, Player, "Debugging");
            }
        }

        public override void OnDeath()
        {
            if (SharpHelper.RandomBetween(0, 1) <= 0.75f)
                Game.PlaySound("CartoonScream", Player.GetWorldPosition());

            IPlayer culprit = null;
            foreach (var player in Game.GetPlayers())
            {
                if (ScriptHelper.IsHiting(player, Player))
                {
                    culprit = player;
                    break;
                }
            }
            m_mommy.Enrage(culprit, 20000);
        }
    }
}
