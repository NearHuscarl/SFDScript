using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScript.Hellraiser
{

    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        Random rnd = new Random();
        IPlayer randPlayer;

        public void OnStartup()
        {
            //System.Diagnostics.Debugger.Break();
            var players = Game.GetPlayers();
            var randPlayer = players[rnd.Next(0, players.Length)];

            IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            Timer.SetIntervalTime(100);
            Timer.SetRepeatCount(0);
            Timer.SetScriptMethod("Teleport");
            Timer.Trigger();

            if (randPlayer.IsUser)
                Teleporters.Add(randPlayer);

            PlayerModifiers modify = randPlayer.GetModifiers();
            modify.SizeModifier = 2.0f;
            modify.MeleeDamageDealtModifier = 3f;
            modify.MaxHealth = 300;
            modify.FireDamageTakenModifier = -1;
            modify.CurrentHealth = 300;
            randPlayer.SetModifiers(modify);
        }
        List<IPlayer> Teleporters = new List<IPlayer>();
        List<IPlayer> NonTeleporters = new List<IPlayer>();

        public void Teleport(TriggerArgs args)
        {
            for (int i = NonTeleporters.Count - 1; i >= 0; i--)
            {
                IPlayer ply = NonTeleporters[i];
                if (ply.IsOnGround && ply.IsIdle || ply.IsLedgeGrabbing || ply.IsLedgeGrabbing)
                {
                    NonTeleporters.Remove(ply);
                    Teleporters.Add(ply);
                }
            }
            for (int i = Teleporters.Count - 1; i >= 0; i--)
            {
                IPlayer ply = Teleporters[i];
                Vector2 Here = ply.GetWorldPosition();
                Vector2 HereR = ply.GetWorldPosition() + new Vector2(20, 0);
                Vector2 HereL = ply.GetWorldPosition() + new Vector2(-20, 0);
                if (ply.IsBlocking && ply.IsWalking && ply.FacingDirection == 1)
                {
                    Game.PlaySound("EXP", Here, 1f);
                    Game.PlaySound("EXP", HereR, 1f);
                    Game.PlayEffect("explosion1", Here);
                    Game.PlayEffect("explosion1", HereR);
                    ply.SetWorldPosition(HereR);
                    Teleporters.Remove(ply);
                    NonTeleporters.Add(ply);
                }
                if (ply.IsBlocking && ply.IsWalking && ply.FacingDirection == -1)
                {
                    Game.PlaySound("EXP", Here, 1f);
                    Game.PlaySound("EXP", HereL, 1f);
                    Game.PlayEffect("explosion1", Here);
                    Game.PlayEffect("explosion1", HereL);
                    ply.SetWorldPosition(HereL);
                    Teleporters.Remove(ply);
                    NonTeleporters.Add(ply);
                }
            }
        }
    }
}
