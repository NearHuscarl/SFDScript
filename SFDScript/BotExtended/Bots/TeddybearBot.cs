using SFDGameScriptInterface;
using SFDScript.Library;
using System;
using System.Collections.Generic;
using static SFDScript.BotExtended.GameScript;
using static SFDScript.Library.Mocks.MockObjects;

namespace SFDScript.BotExtended.Bots
{
    public class TeddybearBot : Bot
    {
        public bool IsEnraged { get; private set; }
        private static readonly List<string> PlayerEnrageReactions = new List<string>()
        {
            "Oh no",
            "Fuck",
            "Guess I will die",
            "Wait. I'm sorry",
            "It's not my fault",
        };

        public override void OnSpawn(List<Bot> others)
        {
            IsEnraged = false;

            if (others.Count >= 1) // has cults
                Player.SetBotName("Mommy Bear");

            var behavior = Player.GetBotBehaviorSet();
            behavior.GuardRange = 1f;
            behavior.ChaseRange = 1f;
            Player.SetBotBehaviorSet(behavior);
        }

        private float m_startEnrageTime = 0f;
        private bool m_moveorattack = false;
        protected override void OnUpdate(float elapsed)
        {
            base.OnUpdate(elapsed);

            if (IsEnraged)
            {
                if (ScriptHelper.IsElapsed(m_startEnrageTime, m_enrageTime))
                {
                    StopEnraging();
                }

                // This is a workaround to make a bot target specific IPlayer
                // Need to set
                // behavior.GuardRange = 1f;
                // behavior.ChaseRange = 1f;
                // TODO: need more testing
                if (Vector2.Distance(Player.GetWorldPosition(), m_offender.GetWorldPosition()) < 75)
                {
                    if (!m_moveorattack && Game.IsEditorTest)
                        Game.CreateDialogue("Attack", Player);
                    Player.SetGuardTarget(null);
                    m_moveorattack = true;
                }
                else
                {
                    if (m_moveorattack && Game.IsEditorTest)
                        Game.CreateDialogue("Move", Player);
                    Player.SetGuardTarget(m_offender);
                    m_moveorattack = false;
                }

                if (m_offender.IsDead)
                {
                    m_offender = FindClosestTarget(Player.GetWorldPosition());
                    Game.CreateDialogue("Oh fuck", m_offender); // TODO: remove
                }
            }
        }

        private PlayerModifiers m_normalModifiers;
        private BotBehaviorSet m_normalBehaviorSet;
        private int m_enrageTime = 0;
        private IPlayer m_offender;
        public void Enrage(IPlayer offender, int enrageTime)
        {
            if (Player.IsRemoved || Player == null) return;
            if (IsEnraged)
                Game.CreateDialogue("MOTHERFUCKER HOW DARE YOU", DialogueColor, Player);
            else
                Game.CreateDialogue("GRRRRRR", DialogueColor, Player);
            Player.SetGuardTarget(offender);

            Game.CreateDialogue(RandomHelper.GetItem(PlayerEnrageReactions), offender);
            
            m_normalModifiers = Player.GetModifiers();
            var enrageModifiers = Player.GetModifiers();
            enrageModifiers.RunSpeedModifier = IsEnraged ? 1.5f : 1.25f;
            enrageModifiers.SprintSpeedModifier = IsEnraged ? 1.5f : 1.25f;
            enrageModifiers.MeleeForceModifier = IsEnraged ? 4f : 3f;
            Player.SetModifiers(enrageModifiers);

            m_normalBehaviorSet = Player.GetBotBehaviorSet();
            var bs = GetBehaviorSet(BotAI.RagingHulk, IsEnraged ? SearchItems.Melee : SearchItems.Makeshift);
            Player.SetBotBehaviorSet(bs);

            Player.SetStrengthBoostTime(enrageTime);

            IsEnraged = true;
            m_enrageTime = enrageTime;
            m_startEnrageTime = Game.TotalElapsedGameTime;
            m_offender = offender;
        }

        private void StopEnraging()
        {
            Player.SetGuardTarget(null);
            Player.SetModifiers(m_normalModifiers);
            Player.SetBotBehaviorSet(m_normalBehaviorSet);
            Player.SetStrengthBoostTime(0);
            IsEnraged = false;
            m_offender = null;
        }

        public static IPlayer FindClosestTarget(Vector2 position)
        {
            IPlayer target = null;

            foreach (var player in Game.GetPlayers())
            {
                if (player.IsDead || player.IsRemoved || ScriptHelper.GetSkin(player) == Skin.Bear)
                    continue;

                if (target == null) target = player;

                var offenderDistance = Vector2.Distance(target.GetWorldPosition(), position);
                var potentialOffenderDistance = Vector2.Distance(player.GetWorldPosition(), position);

                if (potentialOffenderDistance < offenderDistance)
                {
                    target = player;
                }
            }

            return target;
        }
    }
}
