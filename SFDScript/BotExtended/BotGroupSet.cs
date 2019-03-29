﻿using SFDGameScriptInterface;
using SFDScript.BotExtended.Group;
using System.Collections.Generic;

namespace SFDScript.BotExtended
{
    public partial class GameScript : GameScriptInterface
    {

        private static List<BotType> CommonZombieTypes = new List<BotType>()
        {
            BotType.Zombie,
            BotType.ZombieAgent,
            BotType.ZombieFlamer,
            BotType.ZombieGangster,
            BotType.ZombieNinja,
            BotType.ZombiePolice,
            BotType.ZombieSoldier,
            BotType.ZombieThug,
            BotType.ZombieWorker,
        };
        private static List<BotType> MutatedZombieTypes = new List<BotType>()
        {
            BotType.ZombieBruiser,
            BotType.ZombieChild,
            BotType.ZombieFat,
            BotType.ZombieFlamer,
        };

        public static GroupSet GetGroupSet(BotGroup botGroup)
        {
            var groupSet = new GroupSet();

            switch (botGroup)
            {
                case BotGroup.Assassin:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.AssassinMelee, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.AssassinRange, 1f),
                    });
                    break;
                }
                case BotGroup.Agent:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Agent, 1f),
                    });
                    break;
                }
                case BotGroup.Bandido:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Bandido, 1f),
                    });
                    break;
                }
                case BotGroup.Biker:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Biker, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Biker, 0.5f),
                        new SubGroup(BotType.Thug, 0.5f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Biker, 0.6f),
                        new SubGroup(BotType.BikerHulk, 0.4f),
                    });
                    break;
                }
                case BotGroup.Clown:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ClownCowboy, 0.5f),
                        new SubGroup(BotType.ClownGangster, 0.25f),
                        new SubGroup(BotType.ClownBoxer, 0.25f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ClownCowboy, 0.6f),
                        new SubGroup(BotType.ClownGangster, 0.4f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ClownBoxer, 0.7f),
                        new SubGroup(BotType.ClownGangster, 0.3f),
                    });
                    break;
                }
                case BotGroup.Cowboy:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Cowboy, 1f),
                    });
                    break;
                }
                case BotGroup.Gangster:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Gangster, 0.8f),
                        new SubGroup(BotType.GangsterHulk, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Gangster, 0.7f),
                        new SubGroup(BotType.ThugHulk, 0.3f),
                    });
                    break;
                }
                case BotGroup.Marauder:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(new List<BotType>()
                        {
                            BotType.MarauderBiker,
                            BotType.MarauderCrazy,
                            BotType.MarauderNaked,
                            BotType.MarauderRifleman,
                            BotType.MarauderRobber,
                            BotType.MarauderTough,
                        }, 1f),
                    });
                    break;
                }
                case BotGroup.MetroCop:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop, 0.7f),
                        new SubGroup(BotType.Agent2, 0.3f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop, 0.5f),
                        new SubGroup(BotType.Agent2, 0.5f),
                    });
                    break;
                }
                case BotGroup.Police:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Police, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Police, 0.7f),
                        new SubGroup(BotType.PoliceSWAT, 0.3f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.PoliceSWAT, 0.8f),
                        new SubGroup(BotType.Police, 0.2f),
                    });
                    break;
                }
                case BotGroup.PoliceSWAT:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.PoliceSWAT, 1f),
                    });
                    break;
                }
                case BotGroup.Sniper:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Sniper, 1f),
                    });
                    break;
                }
                case BotGroup.Soldier:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Soldier, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Soldier, 0.7f),
                        new SubGroup(BotType.Sniper, 0.3f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Soldier, 0.9f),
                        new SubGroup(BotType.Soldier2, 0.1f),
                    });
                    break;
                }
                case BotGroup.Thug:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Thug, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Thug, 0.5f),
                        new SubGroup(BotType.Biker, 0.5f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Thug, 0.6f),
                        new SubGroup(BotType.ThugHulk, 0.4f),
                    });
                    break;
                }
                case BotGroup.Zombie:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Zombie, 0.4f),
                        new SubGroup(CommonZombieTypes, 0.6f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.8f),
                        new SubGroup(BotType.ZombieBruiser, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.6f),
                        new SubGroup(BotType.ZombieBruiser, 0.4f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.8f),
                        new SubGroup(BotType.ZombieChild, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.6f),
                        new SubGroup(BotType.ZombieChild, 0.4f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.8f),
                        new SubGroup(BotType.ZombieFat, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.6f),
                        new SubGroup(BotType.ZombieFat, 0.4f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.8f),
                        new SubGroup(BotType.ZombieFlamer, 0.2f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.6f),
                        new SubGroup(BotType.ZombieFlamer, 0.4f),
                    });
                    break;
                }
                case BotGroup.ZombieHard:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(MutatedZombieTypes, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.2f),
                        new SubGroup(MutatedZombieTypes, 0.8f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.4f),
                        new SubGroup(MutatedZombieTypes, 0.6f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(CommonZombieTypes, 0.7f),
                        new SubGroup(MutatedZombieTypes, 0.3f),
                    });
                    break;
                }

                case BotGroup.Boss_Demolitionist:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Demolitionist),
                    });
                    break;
                }
                case BotGroup.Boss_Funnyman:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Funnyman),
                        new SubGroup(BotType.ClownBodyguard, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Funnyman),
                        new SubGroup(new List<BotType>()
                        {
                            BotType.ClownBoxer,
                            BotType.ClownCowboy,
                            BotType.ClownGangster,
                        }, 1f),
                    });
                    break;
                }
                case BotGroup.Boss_Jo:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Jo),
                        new SubGroup(BotType.Biker, 0.6f),
                        new SubGroup(BotType.BikerHulk, 0.4f),
                    });
                    break;
                }
                case BotGroup.Boss_Hacker:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Hacker),
                        new SubGroup(BotType.Hacker),
                    });
                    break;
                }
                case BotGroup.Boss_Incinerator:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Incinerator),
                    });
                    break;
                }
                case BotGroup.Boss_Kingpin:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Kingpin),
                        new SubGroup(BotType.Bodyguard, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Kingpin),
                        new SubGroup(BotType.GangsterHulk, 0.55f),
                        new SubGroup(BotType.Bodyguard2, 0.45f),
                    });
                    break;
                }
                case BotGroup.Boss_MadScientist:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Kriegbär),
                        new SubGroup(BotType.Fritzliebe),
                    });
                    break;
                }
                case BotGroup.Boss_Meatgrinder:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Meatgrinder),
                    });
                    break;
                }
                case BotGroup.Boss_Mecha:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Mecha),
                    });
                    break;
                }
                case BotGroup.Boss_MetroCop:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop2),
                        new SubGroup(BotType.MetroCop, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.MetroCop2),
                        new SubGroup(BotType.MetroCop, 0.7f),
                        new SubGroup(BotType.Agent2, 0.3f),
                    });
                    break;
                }
                case BotGroup.Boss_Ninja:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Ninja),
                    });
                    break;
                }
                case BotGroup.Boss_Santa:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Santa),
                        new SubGroup(BotType.Elf, 1f),
                    });
                    break;
                }
                case BotGroup.Boss_Teddybear:
                {
                    // TODO: uncomment
                    //groupSet.AddGroup(new List<SubGroup>()
                    //{
                    //    new SubGroup(BotType.Teddybear),
                    //});
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.Teddybear),
                        new SubGroup(BotType.Babybear),
                        new SubGroup(BotType.Babybear),
                    });
                    break;
                }
                case BotGroup.Boss_Zombie:
                {
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ZombieFighter),
                        new SubGroup(CommonZombieTypes, 1f),
                    });
                    groupSet.AddGroup(new List<SubGroup>()
                    {
                        new SubGroup(BotType.ZombieFighter),
                        new SubGroup(CommonZombieTypes, 0.7f),
                        new SubGroup(MutatedZombieTypes, 0.3f),
                    });
                    break;
                }
            }

            return groupSet;
        }
    }
}
