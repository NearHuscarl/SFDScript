namespace SFDScript.Library
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=33&t=2170&p=13526&hilit=effect#p13526
    public static class Effect
    {
        public static readonly string BLOOD = "BLD";
        // IGame.PlayEffect("CAM_S", Vector2 position, float strength, float milliseconds, bool UNDOCUMENTED)
        // IGame.PlayEffect("CAM_S", Vector2.Zero, 1f, 500f, true)
        public static readonly string CAMERA_SHAKER = "CAM_S";
        // IGame.PlayEffect("CFTXT", Vector2 Position, string Text)
        public static readonly string CUSTOM_FLOAT_TEXT = "CFTXT";
        public static readonly string ELECTRIC = "Electric";
        public static readonly string EXPLOSION = "EXP";
        public static readonly string ITEM_GLEAM = "GLM";
        public static readonly string SPARKS = "S_P";
        public static readonly string STEAM = "STM";

        public static readonly string ACID_SPLASH = "ACS";
        public static readonly string WATER_SPLASH = "WS";

        public static readonly string WOOD_PARTICLES = "W_P";

        public static readonly string BLOOD_TRAIL = "TR_B";
        public static readonly string DUST_TRAIL = "TR_D";
        public static readonly string SMOKE_TRAIL = "TR_S";

        // From smallest effect to biggest
        public static readonly string FIRE_TRAIL = "TR_F";
        public static readonly string FIRE = "FIRE";
        public static readonly string PLAYER_BURNED = "PLRB";

        // Not working
        public static readonly string PICKUP_TEXT = "PWT";
        public static readonly string FIRE_BIG = "FBG";
        public static readonly string HIT = "HIT";
        public static readonly string GLASS_PARTICLES = "G_P";
        public static readonly string PAPER_DESTROYED = "PPR_D";
        public static readonly string TRACE_SPAWNER = "TR_SPR";
    }
}
