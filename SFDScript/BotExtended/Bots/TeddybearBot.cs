using SFDGameScriptInterface;
using SFDScript.Library;
using System.Collections.Generic;
using static SFDScript.BotExtended.GameScript;
using static SFDScript.Library.Mocks.MockObjects;

namespace SFDScript.BotExtended.Bots
{
    public class TeddybearBot : Bot
    {
        public bool IsEnraged { get; private set; }

        public override void OnSpawn(List<Bot> others)
        {
            IsEnraged = false;

            if (others.Count >= 1) // has cults
                Player.SetBotName("Mommy Bear");

            var behavior = Player.GetBotBehaviorSet();
            behavior.GuardRange = 8;
            // behavior.ChaseRange = 16;
            Player.SetBotBehaviorSet(behavior);
        }

        private float m_startEnrageTime = 0f;
        protected override void OnUpdate(float elapsed)
        {
            base.OnUpdate(elapsed);

            if (IsEnraged)
            {
                if (ScriptHelper.IsElapsed(m_startEnrageTime, m_enrageTime))
                {
                    StopEnraging();
                }
            }
        }

        private PlayerModifiers m_normalModifiers;
        private BotBehaviorSet m_normalBehaviorSet;
        private int m_enrageTime = 0;
        public void Enrage(IPlayer victim, int enrageTime)
        {
            if (IsEnraged || Player.IsRemoved || Player == null) return;

            Game.CreateDialogue("GRRRRRR", Bot.DialogueColor, Player);
            Player.SetGuardTarget(victim);
            
            m_normalModifiers = Player.GetModifiers();
            m_normalBehaviorSet = Player.GetBotBehaviorSet();
            var enrageModifiers = m_normalModifiers;
            enrageModifiers.RunSpeedModifier = 1.25f;
            enrageModifiers.SprintSpeedModifier = 1.25f;
            enrageModifiers.MeleeForceModifier = 3f;
            Player.SetModifiers(enrageModifiers);
            Player.SetBotBehaviorSet(GetBehaviorSet(BotAI.Ninja, SearchItems.Makeshift));
            Player.SetStrengthBoostTime(enrageTime);

            IsEnraged = true;
            m_enrageTime = enrageTime;
            m_startEnrageTime = Game.TotalElapsedGameTime;
        }

        private void StopEnraging()
        {
            Player.SetGuardTarget(null);
            Player.SetModifiers(m_normalModifiers);
            Player.SetBotBehaviorSet(m_normalBehaviorSet);
            Player.SetStrengthBoostTime(0);
            IsEnraged = false;
        }
    }
}
