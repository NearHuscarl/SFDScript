using System;
using System.Collections.Generic;
using SFDGameScriptInterface;

namespace SFDScript.BotExtended
{
    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        public void OnStartup()
        {
            try
            {
                //System.Diagnostics.Debugger.Break();

                if (Game.IsEditorTest)
                {
                    var player = Game.GetPlayers()[0];
                    var modifiers = player.GetModifiers();

                    modifiers.MaxHealth = 5000;
                    modifiers.CurrentHealth = 5000;
                    modifiers.InfiniteAmmo = 1;
                    modifiers.MeleeStunImmunity = 1;

                    player.SetModifiers(modifiers);
                    player.GiveWeaponItem(WeaponItem.KNIFE);
                    player.GiveWeaponItem(WeaponItem.MAGNUM);
                    player.GiveWeaponItem(WeaponItem.LAZER);
                    player.GiveWeaponItem(WeaponItem.FLAMETHROWER);
                    player.GiveWeaponItem(WeaponItem.MOLOTOVS);
                    player.GiveWeaponItem(WeaponItem.STRENGTHBOOST);
                }

                BotHelper.Initialize();
            }
            catch (Exception e)
            {
                Game.ShowChatMessage("[Botextended script]: Error");
                Game.ShowChatMessage(e.Message);
                Game.ShowChatMessage(e.StackTrace);
                Game.ShowChatMessage(e.TargetSite.ToString());
            }
        }

        public void OnShutdown()
        {
            StoreStatistics();
        }

        private void StoreStatistics()
        {
            var storage = Game.LocalStorage;
            var players = Game.GetPlayers();
            var groupDead = true;
            var updatedBotTypes = new List<BotType>();

            foreach (var player in players)
            {
                var botType = BotHelper.GetBotType(player);
                if (botType == BotType.None || updatedBotTypes.Contains(botType)) continue;
                var botTypeKeyPrefix = BotHelper.GET_BOTTYPE_STORAGE_KEY(botType);

                var botWinCountKey = botTypeKeyPrefix + "_WIN_COUNT";
                int botOldWinCount;
                var getBotWinCountAttempt = storage.TryGetItemInt(botWinCountKey, out botOldWinCount);

                var botTotalMatchKey = botTypeKeyPrefix + "_TOTAL_MATCH";
                int botOldTotalMatch;
                var getBotTotalMatchAttempt = storage.TryGetItemInt(botTotalMatchKey, out botOldTotalMatch);

                if (getBotWinCountAttempt && getBotTotalMatchAttempt)
                {
                    if (!player.IsDead)
                        storage.SetItem(botWinCountKey, botOldWinCount + 1);
                    storage.SetItem(botTotalMatchKey, botOldTotalMatch + 1);
                }
                else
                {
                    if (!player.IsDead)
                        storage.SetItem(botWinCountKey, 1);
                    else
                        storage.SetItem(botWinCountKey, 0);
                    storage.SetItem(botTotalMatchKey, 1);
                }

                updatedBotTypes.Add(botType);
                if (!player.IsDead) groupDead = false;
            }

            var currentBotGroup = BotHelper.CurrentBotGroup;
            var currentGroupSetIndex = BotHelper.CurrentGroupSetIndex;
            var botGroupKeyPrefix = BotHelper.GET_GROUP_STORAGE_KEY(BotHelper.CurrentBotGroup, BotHelper.CurrentGroupSetIndex);

            var groupWinCountKey = botGroupKeyPrefix + "_WIN_COUNT";
            int groupOldWinCount;
            var getGroupWinCountAttempt = storage.TryGetItemInt(groupWinCountKey, out groupOldWinCount);

            var groupTotalMatchKey = botGroupKeyPrefix + "_TOTAL_MATCH";
            int groupOldTotalMatch;
            var getGroupTotalMatchAttempt = storage.TryGetItemInt(groupTotalMatchKey, out groupOldTotalMatch);

            if (getGroupWinCountAttempt && getGroupTotalMatchAttempt)
            {
                if (!groupDead)
                    storage.SetItem(groupWinCountKey, groupOldWinCount + 1);
                storage.SetItem(groupTotalMatchKey, groupOldTotalMatch + 1);
            }
            else
            {
                if (!groupDead)
                    storage.SetItem(groupWinCountKey, 1);
                else
                    storage.SetItem(groupWinCountKey, 0);
                storage.SetItem(groupTotalMatchKey, 1);
            }
        }
    }
}

// TODO:
// Add draw weapon first

// fritzliebe
// Kriegbar
// mecha

// Commands:
// botextended groupInterval

// Fix a bug where the group is registered as win if you exit the server instead of continue map

// Group
// Add bulletproof and meleeproof superfighters
// Multiple spawn|dead lines?

    // commit
    // add soldier group
    // reduce cowboy aim delay
    // misc bug fixes and refactor