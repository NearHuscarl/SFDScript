using SFDGameScriptInterface;

namespace SFDScript.BotExtended
{
    public partial class GameScript : GameScriptInterface
    {
        public static BotBehaviorSet GetBehaviorSet(BotAI botAI, SearchItems searchItems)
        {
            var botBehaviorSet = new BotBehaviorSet()
            {
                MeleeActions = BotMeleeActions.Default,
                MeleeActionsWhenHit = BotMeleeActions.DefaultWhenHit,
                MeleeActionsWhenEnraged = BotMeleeActions.DefaultWhenEnraged,
                MeleeActionsWhenEnragedAndHit = BotMeleeActions.DefaultWhenEnragedAndHit,
                ChaseRange = 44f,
                GuardRange = 40f,
            };

            switch (botAI)
            {
                case BotAI.Debug:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotD);
                    botBehaviorSet.RangedWeaponBurstTimeMin = 5000;
                    botBehaviorSet.RangedWeaponBurstTimeMax = 5000;
                    botBehaviorSet.RangedWeaponBurstPauseMin = 0;
                    botBehaviorSet.RangedWeaponBurstPauseMax = 0;
                    break;
                }
                case BotAI.Easy:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotD);
                    break;
                }
                case BotAI.Normal:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotC);
                    break;
                }
                case BotAI.Hard:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotB);
                    break;
                }
                case BotAI.Expert:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.BotA);
                    break;
                }
                case BotAI.Hacker:
                {
                    botBehaviorSet.NavigationMode = BotBehaviorNavigationMode.PathFinding;
                    botBehaviorSet.MeleeMode = BotBehaviorMeleeMode.Default;
                    botBehaviorSet.EliminateEnemies = true;
                    botBehaviorSet.SearchForItems = true;
                    botBehaviorSet.OffensiveEnrageLevel = 0.8f;
                    botBehaviorSet.NavigationRandomPausesLevel = 0.1f;
                    botBehaviorSet.DefensiveRollFireLevel = 0.95f;
                    botBehaviorSet.DefensiveAvoidProjectilesLevel = 0.7f;
                    botBehaviorSet.OffensiveClimbingLevel = 0.7f;
                    botBehaviorSet.OffensiveSprintLevel = 0.6f;
                    botBehaviorSet.OffensiveDiveLevel = 0.6f;
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0.9f;
                    botBehaviorSet.ChokePointPlayerCountThreshold = 1;
                    botBehaviorSet.ChokePointValue = 150f;
                    botBehaviorSet.MeleeWaitTimeLimitMin = 100f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 200f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.SetMeleeActionsToExpert();
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.RangedWeaponUsage = true;
                    botBehaviorSet.RangedWeaponAccuracy = 0.85f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 50f;
                    botBehaviorSet.RangedWeaponAimShootDelayMax = 200f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMin = 50f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMax = 50f;
                    botBehaviorSet.RangedWeaponBurstTimeMin = 400f;
                    botBehaviorSet.RangedWeaponBurstTimeMax = 800f;
                    botBehaviorSet.RangedWeaponBurstPauseMin = 400f;
                    botBehaviorSet.RangedWeaponBurstPauseMax = 800f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 800f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.95f;
                    botBehaviorSet.RangedWeaponPrecisionAimShootDelayMin = 25f;
                    botBehaviorSet.RangedWeaponPrecisionAimShootDelayMax = 50f;
                    botBehaviorSet.RangedWeaponPrecisionBurstTimeMin = 800f;
                    botBehaviorSet.RangedWeaponPrecisionBurstTimeMax = 1600f;
                    botBehaviorSet.RangedWeaponPrecisionBurstPauseMin = 100f;
                    botBehaviorSet.RangedWeaponPrecisionBurstPauseMax = 200f;
                    break;
                }
                case BotAI.MeleeExpert:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0.9f;
                    botBehaviorSet.MeleeWaitTimeLimitMin = 600f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 800f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.MeleeWeaponUseFullRange = true;
                    break;
                }
                case BotAI.MeleeHard:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0.75f;
                    botBehaviorSet.MeleeWaitTimeLimitMin = 800f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 1000f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.MeleeWeaponUseFullRange = false;
                    break;
                }
                case BotAI.Ninja: // == BotAI.MeleeExpert + more offensive melee tactic
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0.9f;
                    botBehaviorSet.MeleeWaitTimeLimitMin = 600f;
                    botBehaviorSet.MeleeWaitTimeLimitMax = 800f;
                    botBehaviorSet.MeleeUsage = true;
                    botBehaviorSet.MeleeWeaponUsage = true;
                    botBehaviorSet.MeleeWeaponUseFullRange = true;

                    botBehaviorSet.SearchForItems = true;
                    botBehaviorSet.SearchItems = SearchItems.Melee;
                    botBehaviorSet.OffensiveEnrageLevel = 0.5f;
                    botBehaviorSet.NavigationRandomPausesLevel = 0.1f;
                    botBehaviorSet.DefensiveRollFireLevel = 0.95f;
                    botBehaviorSet.DefensiveAvoidProjectilesLevel = 0.9f;
                    botBehaviorSet.OffensiveClimbingLevel = 0.9f;
                    botBehaviorSet.OffensiveSprintLevel = 0.9f;
                    botBehaviorSet.OffensiveDiveLevel = 0.1f; // 0.7f
                    break;
                }
                case BotAI.RangeExpert:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.RangedA);
                    botBehaviorSet.RangedWeaponAccuracy = 0.85f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 600f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 2000f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.95f;
                    break;
                }
                case BotAI.RangeHard:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.RangedA);
                    botBehaviorSet.RangedWeaponAccuracy = 0.75f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 600f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 2000f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.9f;
                    break;
                }
                case BotAI.Sniper: // == BotAI.RangeExpert + more defensive melee tactic
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.RangedA);
                    botBehaviorSet.RangedWeaponMode = BotBehaviorRangedWeaponMode.ManualAim;
                    botBehaviorSet.RangedWeaponAccuracy = 0.85f;
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 600f;
                    botBehaviorSet.RangedWeaponPrecisionInterpolateTime = 2000f;
                    botBehaviorSet.RangedWeaponPrecisionAccuracy = 0.95f;

                    botBehaviorSet.DefensiveRollFireLevel = 0.95f;
                    botBehaviorSet.DefensiveAvoidProjectilesLevel = 0.6f;
                    botBehaviorSet.OffensiveEnrageLevel = 0.2f;
                    botBehaviorSet.OffensiveClimbingLevel = 0f;
                    botBehaviorSet.OffensiveSprintLevel = 0f;
                    botBehaviorSet.OffensiveDiveLevel = 0f;
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0f;
                    botBehaviorSet.TeamLineUp = false;
                    break;
                }
                case BotAI.Grunt:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Grunt);

                    // Taken from PredefinedAIType.BotB, PredefinedAIType.Grunt is too slow in shooting
                    botBehaviorSet.RangedWeaponAimShootDelayMin = 200f;
                    botBehaviorSet.RangedWeaponAimShootDelayMax = 600f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMin = 200f;
                    botBehaviorSet.RangedWeaponHipFireAimShootDelayMax = 600f;
                    botBehaviorSet.RangedWeaponBurstTimeMin = 400f;
                    botBehaviorSet.RangedWeaponBurstTimeMax = 800f;
                    botBehaviorSet.RangedWeaponBurstPauseMin = 400f;
                    botBehaviorSet.RangedWeaponBurstPauseMax = 800f;
                    break;
                }
                case BotAI.Hulk:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Hulk);
                    break;
                }
                case BotAI.Meatgrinder:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Meatgrinder);
                    break;
                }
                case BotAI.ZombieSlow:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.ZombieA);
                    break;
                }
                case BotAI.ZombieFast:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.ZombieB);
                    break;
                }
                case BotAI.ZombieHulk:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.Hulk);
                    botBehaviorSet.AttackDeadEnemies = true;
                    botBehaviorSet.SearchForItems = false;
                    botBehaviorSet.MeleeWeaponUsage = false;
                    botBehaviorSet.RangedWeaponUsage = false;
                    botBehaviorSet.PowerupUsage = false;
                    botBehaviorSet.ChokePointValue = 32f;
                    botBehaviorSet.ChokePointPlayerCountThreshold = 5;
                    botBehaviorSet.DefensiveRollFireLevel = 0.1f;
                    botBehaviorSet.OffensiveDiveLevel = 0f;
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0f;
                    break;
                }
                case BotAI.ZombieFighter:
                {
                    botBehaviorSet = BotBehaviorSet.GetBotBehaviorPredefinedSet(PredefinedAIType.MeleeB);
                    botBehaviorSet.AttackDeadEnemies = true;
                    botBehaviorSet.SearchForItems = false;
                    botBehaviorSet.MeleeWeaponUsage = false;
                    botBehaviorSet.RangedWeaponUsage = false;
                    botBehaviorSet.PowerupUsage = false;
                    botBehaviorSet.ChokePointValue = 32f;
                    botBehaviorSet.ChokePointPlayerCountThreshold = 5;
                    botBehaviorSet.DefensiveRollFireLevel = 0.1f;
                    botBehaviorSet.OffensiveDiveLevel = 0f;
                    botBehaviorSet.CounterOutOfRangeMeleeAttacksLevel = 0f;
                    break;
                }
                default:
                {
                    botBehaviorSet.NavigationMode = BotBehaviorNavigationMode.None;
                    botBehaviorSet.MeleeMode = BotBehaviorMeleeMode.None;
                    botBehaviorSet.EliminateEnemies = false;
                    botBehaviorSet.SearchForItems = false;
                    break;
                }
            }

            botBehaviorSet.SearchForItems = true;
            botBehaviorSet.SearchItems = searchItems; // Disable SearchItems by setting to None

            return botBehaviorSet;
        }
    }
}
