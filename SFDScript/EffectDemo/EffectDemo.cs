using System.Collections.Generic;
using System.Linq;
using SFDGameScriptInterface;

namespace SFDScript.EffectDemo
{
    /// <summary>
    /// Display all available effects. Type "/effect help" (without quote) for more information
    /// </summary>
    public class EffectDemo : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public EffectDemo() : base(null) { }

        public void OnStartup()
        {
            EffectsPerRow = 4;
            ScrollXOffset = 0;
            ScrollYOffset = 0;
            EffectQueries = new List<string>(); // empty list means apply all effects

            Events.UpdateCallback.Start(OnUpdate);
            Events.PlayerKeyInputCallback.Start(OnKeyInput);
            Events.UserMessageCallback.Start(OnUserMessage);
        }

        private readonly Dictionary<string, string> EffectNames = new Dictionary<string, string>()
        {
            { "CustomFloatText", EffectName.CustomFloatText },
            { "Gib", EffectName.Gib },
            { "PlayerBurned", EffectName.PlayerBurned },
            { "FireNodeTrailAir", EffectName.FireNodeTrailAir },
            { "FireNodeTrailGround", EffectName.FireNodeTrailGround },
            { "Dig", EffectName.Dig },
            { "ChainsawSmoke", EffectName.ChainsawSmoke },
            { "DestroyDefault", EffectName.DestroyDefault },
            { "DestroyWood", EffectName.DestroyWood },
            { "DestroyMetal", EffectName.DestroyMetal },
            { "DestroyPaper", EffectName.DestroyPaper },
            { "GrenadeDud", EffectName.GrenadeDud },
            { "DestroyCloth", EffectName.DestroyCloth },
            { "Smack", EffectName.Smack },
            { "Block", EffectName.Block },
            { "BulletHit", EffectName.BulletHit },
            { "BulletHitPaper", EffectName.BulletHitPaper },
            { "BulletHitCloth", EffectName.BulletHitCloth },
            { "BulletHitDirt", EffectName.BulletHitDirt },
            { "BulletHitMoney", EffectName.BulletHitMoney },
            { "BulletHitMetal", EffectName.BulletHitMetal },
            { "BulletHitWood", EffectName.BulletHitWood },
            { "BulletHitDefault", EffectName.BulletHitDefault },
            { "DestroyGlass", EffectName.DestroyGlass },
            { "CameraShaker", EffectName.CameraShaker },
            { "AcidSplash", EffectName.AcidSplash },
            { "WaterSplash", EffectName.WaterSplash },
            { "PlayerLandFull", EffectName.PlayerLandFull },
            { "PlayerFootstep", EffectName.PlayerFootstep },
            { "Blood", EffectName.Blood },
            { "BloodTrail", EffectName.BloodTrail },
            { "Explosion", EffectName.Explosion },
            { "MeleeHitUnarmed", EffectName.MeleeHitUnarmed },
            { "MeleeHitBlunt", EffectName.MeleeHitBlunt },
            { "MeleeHitSharp", EffectName.MeleeHitSharp },
            { "PaperDestroyed", EffectName.PaperDestroyed },
            { "ClothHit", EffectName.ClothHit },
            { "GlassParticles", EffectName.GlassParticles },
            { "ItemGleam", EffectName.ItemGleam },
            { "WoodParticles", EffectName.WoodParticles },
            { "Sparks", EffectName.Sparks },
            { "Steam", EffectName.Steam },
            { "DustTrail", EffectName.DustTrail },
            { "CloudDissolve", EffectName.CloudDissolve },
            { "TraceSpawner", EffectName.TraceSpawner },
            { "SmokeTrail", EffectName.SmokeTrail },
            { "Fire", EffectName.Fire },
            { "FireTrail", EffectName.FireTrail },
            { "BulletSlowmoTrace", EffectName.BulletSlowmoTrace },
            { "Electric", EffectName.Electric },
            { "ImpactDefault", EffectName.ImpactDefault },
            { "ImpactPaper", EffectName.ImpactPaper },
        };

        private List<string> effectQueries;
        public List<string> EffectQueries
        {
            get { return effectQueries; }
            set
            {
                var queries = value;

                if (!queries.Any())
                {
                    effectQueries = EffectNames.Keys.ToList();
                }
                else
                {
                    var effectKeys = EffectNames.Keys;
                    effectQueries = new List<string>();

                    foreach (var query in queries)
                    {
                        foreach (var effectKey in effectKeys)
                        {
                            if (effectKey.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                            {
                                effectQueries.Add(effectKey);
                            }
                        }
                    }
                }
            }
        }
        public int ScrollYOffset { get; set; }
        public int ScrollXOffset { get; set; }

        private int effectsPerRow;
        public int EffectsPerRow
        {
            get { return effectsPerRow; }
            set { effectsPerRow = (int)MathHelper.Clamp(value, 1, 20);  }
        }

        private float effectTimer = 0f;
        private void OnUpdate(float dt)
        {
            effectTimer += dt;

            var playerPosition = Game.GetPlayers()[0].GetWorldPosition();
            var startX = playerPosition.X + -250f + ScrollXOffset;
            var startY = 20f + ScrollYOffset;
            var position = new Vector2(startX, startY);
            var row = 0;

            for (var i = 0; i < EffectQueries.Count; i++)
            {
                var effectNameStr = GetEffectString(EffectQueries[i]);
                var effectName = EffectNames[EffectQueries[i]];
                var displayLength = 85;

                Game.DrawText(effectNameStr, position + Vector2.UnitY * 35f);

                if (effectTimer >= 3000)
                {
                    PlayerEffect(effectName, position);
                }

                if ((i + 1) % EffectsPerRow == 0)
                {
                    row++;
                    position.X = startX - displayLength;
                }

                position.X += displayLength;
                position.Y = startY + row * -45f;
            }

            if (effectTimer >= 3000) effectTimer = 0f;
        }

        private string GetEffectString(string effectName)
        {
            switch (effectName)
            {
                case "TraceSpawner":
                    return effectName + " (player)";
                default:
                    return effectName;
            }
        }

        private void PlayerEffect(string effectName, Vector2 position)
        {
            switch (effectName)
            {
                case EffectName.CustomFloatText:
                    Game.PlayEffect(effectName, position, "floating shit");
                    break;

                case EffectName.TraceSpawner:
                    var objId = Game.GetPlayers()[0].UniqueID;
                    Game.PlayEffect(effectName, position, objId, EffectName.Sparks, 2000f);
                    break;

                case EffectName.BulletSlowmoTrace:
                    Game.PlayEffect(effectName, position, position.X, position.Y);
                    break;

                default:
                    Game.PlayEffect(effectName, position);
                    break;
            }
        }

        private void OnKeyInput(IPlayer player, VirtualKeyInfo[] keyInfos)
        {
            foreach (var keyInfo in keyInfos)
            {
                if (keyInfo.Event == VirtualKeyEvent.Pressed)
                {
                    switch (keyInfo.Key)
                    {
                        case VirtualKey.AIM_CLIMB_UP:
                            ScrollYOffset -= 20;
                            break;
                        case VirtualKey.AIM_CLIMB_DOWN:
                            ScrollYOffset += 20;
                            break;
                        case VirtualKey.AIM_RUN_LEFT:
                            ScrollXOffset -= 20;
                            break;
                        case VirtualKey.AIM_RUN_RIGHT:
                            ScrollXOffset += 20;
                            break;
                        case VirtualKey.RELOAD:
                            ScrollXOffset = 0;
                            ScrollYOffset = 0;
                            break;
                        case VirtualKey.ATTACK:
                            EffectsPerRow++;
                            break;
                        case VirtualKey.BLOCK:
                            EffectsPerRow--;
                            break;
                        case VirtualKey.KICK:
                            effectTimer = float.MaxValue;
                            break;
                    }
                }
            }
        }

        private void OnUserMessage(UserMessageCallbackArgs args)
        {
            if (!args.IsCommand || (args.Command != "EFFECT" && args.Command != "E"))
            {
                return;
            }

            var message = args.CommandArguments.ToLowerInvariant();
            var words = message.Split(' ');
            var command = words.FirstOrDefault();

            switch (command)
            {
                case "?":
                case "h":
                case "help":
                    PrintHelp();
                    break;

                default:
                {
                    if (message == "all" || message == "")
                    {
                        EffectQueries = new List<string>();
                    }
                    else
                        EffectQueries = message.Split(' ').ToList();
                    break;
                }
            }
        }

        private void PrintHelp()
        {
            var errorColor = new Color(244, 77, 77);

            Game.ShowChatMessage("--EffectDemo help--", errorColor);
            Game.ShowChatMessage("/<e|effect> [all]: Display all effects");
            Game.ShowChatMessage("/<e|effect> <help|h|?>: Print this help");
            Game.ShowChatMessage("/<e|effect> <effect-names>: Filtered effect by effect-names");
            Game.ShowChatMessage("For example:", errorColor);
            Game.ShowChatMessage("/effect bullet >> display bullet effects");
            Game.ShowChatMessage("/e >> display all effects");
            Game.ShowChatMessage("/e fire blood melee >> display effects related to fire, blood or melee attack");
            Game.ShowChatMessage("Hotkeys", errorColor);
            Game.ShowChatMessage("- UP: Scroll up effect sheet");
            Game.ShowChatMessage("- DOWN: Scroll down effect sheet");
            Game.ShowChatMessage("- RELOAD: Reset scrolling position");
            Game.ShowChatMessage("- ATTACK: Increase number of effects per row");
            Game.ShowChatMessage("- BLOCK: Decrease number of effects per row");
            Game.ShowChatMessage("- KICK: Play all displayed effects immediately");
        }
    }
}
