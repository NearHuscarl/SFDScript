﻿namespace SFDScript.BotExtended
{
    public enum BotAI
    {
        Debug,

        Hacker,
        Expert,
        Hard,
        Normal,
        Easy,

        MeleeExpert, // == BotAI.Hacker but with range weapons disabled
        MeleeHard, // == BotAI.Expert but with range weapons disabled
        RangeExpert, // == BotAI.Hacker but with melee weapons disabled
        RangeHard, // == BotAI.Expert but with melee weapons disabled

        Grunt,
        Hulk,

        Meatgrinder,
        Ninja,
        Sniper,
        Soldier,

        ZombieSlow,
        ZombieFast,
        ZombieHulk,
        ZombieFighter,
    }
}