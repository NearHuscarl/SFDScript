using SFDGameScriptInterface;
using System;

namespace SFDScript.BotExtended
{
    public class BotInfo
    {
        public BotInfo()
        {
            EquipWeaponChance = 1f;
            AIType = BotAI.Debug;
            SearchItems = SearchItems.None;
            IsBoss = false;
            SpawnLine = "";
            SpawnLineChance = 1f;
            DeathLine = "";
            DeathLineChance = 1f;
            StartInfected = false;
            ImmuneToInfect = false;
        }

        private float equipWeaponChance;
        public float EquipWeaponChance
        {
            get { return equipWeaponChance; }
            set { equipWeaponChance = MathHelper.Clamp(value, 0, 1); }
        }

        public BotAI AIType { get; set; }
        public SearchItems SearchItems { get; set; }
        public PlayerModifiers Modifiers { get; set; }
        public bool IsBoss { get; set; }
        public string SpawnLine { get; set; }
        public float SpawnLineChance { get; set; }
        public string DeathLine { get; set; }
        public float DeathLineChance { get; set; }

        private bool startInfected;
        public bool StartInfected
        {
            get { return startInfected; }
            set
            {
                if (ImmuneToInfect && value == true)
                    throw new Exception("StartInfected and ImmuneToInfected cannot be set to true");
                startInfected = value;
            }
        } // Starting as infected by zombie

        public bool ImmuneToInfect { get; set; }
    }
}
