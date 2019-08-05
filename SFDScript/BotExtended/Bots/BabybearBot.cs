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
        private IPlayer m_offender;
        private static readonly float DamageRequiredForMomToEnrage = 20f;

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
        }

        private float m_damageTaken = 0f;
        public override void OnDamage(IPlayer attacker, PlayerDamageArgs args)
        {
            m_offender = attacker;

            if (args.DamageType == PlayerDamageEventType.Melee)
            {
                Game.CreateDialogue("Is melee'ed by " + attacker.Name, Player, "Debugging");
            }
            if (args.DamageType == PlayerDamageEventType.Projectile)
            {
                Game.CreateDialogue("Is shot by " + attacker.Name, Player, "Debugging");
            }

            m_damageTaken += args.Damage;

            if (m_damageTaken >= DamageRequiredForMomToEnrage)
            {
                m_damageTaken = 0f;
                m_mommy.Enrage(attacker, 10000);
            }
        }

        public override void OnDeath(PlayerDeathArgs args)
        {
            if (args.Removed) return;

            if (RandomHelper.Between(0, 1) <= 0.75f)
                Game.PlaySound("CartoonScream", Player.GetWorldPosition());

            m_mommy.Enrage(m_offender, 20000);
        }
    }
}
