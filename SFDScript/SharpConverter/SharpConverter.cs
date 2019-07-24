using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFDGameScriptInterface;
using SFDScript.Library;

/// <summary>
/// Convert some markers info from Map Editor into C# code
/// </summary>
namespace SFDScript.SharpConverter
{
    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        public void OnStartup()
        {
            // Open Maps/BotProfiles.sfdm in map editor

            //var result = "";

            //result = ConvertOneByCustomID<IObjectPlayerProfileInfo>("ClownCowboyProfile");

            //result = ConvertByCustomID<IObjectPlayerProfileInfo>("Hacker");

            //result = ConvertAll<IObjectPlayerProfileInfo>();

            //result = ConvertAll<IObjectPlayerProfileInfo>();

            // Hover your mouse on a tile in the Map Editor window to get position tooltip
            //result = ConvertByPosition<IObjectPlayerModifierInfo>(new Vector2(-44, 156));

            System.Diagnostics.Debugger.Break();
        }

        public string ConvertOneByCustomID<T>(string customID) where T : IObject
        {
            var obj = Game.GetSingleObjectByCustomID<T>(customID);

            return Convert(obj);
        }

        public string ConvertAll<T>() where T : IObject
        {
            var objects = new List<IObject>(Game.GetObjects<T>());
            var castedObj = objects.Select(obj => ((T)obj)).ToList();

            return Convert(castedObj);
        }

        public string ConvertByCustomID<T>(string customID) where T : IObject
        {
            var objects = new List<IObject>(Game.GetObjectsByCustomID<T>(customID));
            var castedObj = objects.Select(obj => ((T)obj)).ToList();

            return Convert(castedObj);
        }

        public string ConvertByPosition<T>(Vector2 position) where T : IObject
        {
            var objects = new List<IObject>(Game.GetObjectsByArea<T>(new Area(position, position)));
            var castedObj = objects.Select(obj => ((T)obj)).ToList();

            return Convert(castedObj);
        }

        public string Convert<T>(List<T> objects) where T : IObject
        {
            var sb = new StringBuilder();
            var type = typeof(T);

            sb.AppendLine("new List<" + type.Name + ">()");
            sb.AppendLine("{");

            foreach (var obj in objects)
            {
                sb.Append(Convert(obj, 4));
                sb.Replace(';', ',', sb.Length - 3, 1); // \r\n is 2 characters
            }

            sb.AppendLine("};");

            return sb.ToString();
        }

        private string Convert<T>(T obj, int indentSize = 0) where T : IObject
        {
            if (obj == null) return "";

            var a = obj as IObjectPlayerProfileInfo;
            if (a != null)
            {
                return Convert(a, indentSize);
            }

            var b = obj as IProfile;
            if (b != null)
            {
                return Convert(b, indentSize);
            }

            var c = obj as IObjectPlayerModifierInfo;
            if (c != null)
            {
                return Convert(c, indentSize);
            }

            var d = obj as PlayerModifiers;
            if (d != null)
            {
                return Convert(d, indentSize);
            }

            throw new NotImplementedException();
        }

        public string Convert(IObjectPlayerProfileInfo profileInfo, int indentSize = 0)
        {
            if (profileInfo == null) return "";

            return Convert(profileInfo.GetProfile(), indentSize);
        }

        public string Convert(IProfile profile, int indentSize = 0)
        {
            if (profile == null) return "";

            var sb = new StringBuilder();
            var indent = new string(' ', indentSize);

            sb.AppendLine(indent + "new IProfile()");
            sb.AppendLine(indent + "{");
            sb.AppendLine(indent + "    Name = \"" + profile.Name + "\",");
            sb.AppendLine(indent + "    Accesory = " + GetClothingItemInfo(profile.Accessory) + ",");
            sb.AppendLine(indent + "    ChestOver = " + GetClothingItemInfo(profile.ChestOver) + ",");
            sb.AppendLine(indent + "    ChestUnder = " + GetClothingItemInfo(profile.ChestUnder) + ",");
            sb.AppendLine(indent + "    Feet = " + GetClothingItemInfo(profile.Feet) + ",");
            sb.AppendLine(indent + "    Gender = Gender." + SharpHelper.EnumToString(profile.Gender) + ",");
            sb.AppendLine(indent + "    Hands = " + GetClothingItemInfo(profile.Hands) + ",");
            sb.AppendLine(indent + "    Head = " + GetClothingItemInfo(profile.Head) + ",");
            sb.AppendLine(indent + "    Legs = " + GetClothingItemInfo(profile.Legs) + ",");
            sb.AppendLine(indent + "    Skin = " + GetClothingItemInfo(profile.Skin) + ",");
            sb.AppendLine(indent + "    Waist = " + GetClothingItemInfo(profile.Waist) + ",");
            sb.AppendLine(indent + "};");

            return sb.ToString();
        }

        private string GetClothingItemInfo(IProfileClothingItem clothingItem)
        {
            if (clothingItem == null) return "null";

            return "new IProfileClothingItem(\"" + clothingItem.Name + "\", \"" + clothingItem.Color1 + "\", \"" + clothingItem.Color2 + "\", \"" + clothingItem.Color3 + "\")";
        }

        public string Convert(PlayerModifiers playerModifiers, int indentSize = 0)
        {
            if (playerModifiers == null) return "";

            var sb = new StringBuilder();
            var indent = new string(' ', indentSize);

            sb.AppendLine(indent + "new PlayerModifiers()");
            sb.AppendLine(indent + "{");
            sb.AppendLine(indent + "    MaxHealth = " + V(playerModifiers.MaxHealth) + ",");
            sb.AppendLine(indent + "    MaxEnergy = " + V(playerModifiers.MaxEnergy) + ",");
            sb.AppendLine(indent + "    CurrentHealth = " + V(playerModifiers.CurrentHealth) + ",");
            sb.AppendLine(indent + "    CurrentEnergy = " + V(playerModifiers.CurrentEnergy) + ",");
            sb.AppendLine(indent + "    EnergyConsumptionModifier = " + V(playerModifiers.EnergyConsumptionModifier) + ",");
            sb.AppendLine(indent + "    ExplosionDamageTakenModifier = " + V(playerModifiers.ExplosionDamageTakenModifier) + ",");
            sb.AppendLine(indent + "    ProjectileDamageTakenModifier = " + V(playerModifiers.ProjectileDamageTakenModifier) + ",");
            sb.AppendLine(indent + "    ProjectileCritChanceTakenModifier = " + V(playerModifiers.ProjectileCritChanceTakenModifier) + ",");
            sb.AppendLine(indent + "    FireDamageTakenModifier = " + V(playerModifiers.FireDamageTakenModifier) + ",");
            sb.AppendLine(indent + "    MeleeDamageTakenModifier = " + V(playerModifiers.MeleeDamageTakenModifier) + ",");
            sb.AppendLine(indent + "    ImpactDamageTakenModifier = " + V(playerModifiers.ImpactDamageTakenModifier) + ",");
            sb.AppendLine(indent + "    ProjectileDamageDealtModifier = " + V(playerModifiers.ProjectileDamageDealtModifier) + ",");
            sb.AppendLine(indent + "    ProjectileCritChanceDealtModifier = " + V(playerModifiers.ProjectileCritChanceDealtModifier) + ",");
            sb.AppendLine(indent + "    MeleeDamageDealtModifier = " + V(playerModifiers.MeleeDamageDealtModifier) + ",");
            sb.AppendLine(indent + "    MeleeForceModifier = " + V(playerModifiers.MeleeForceModifier) + ",");
            sb.AppendLine(indent + "    MeleeStunImmunity = " + V(playerModifiers.MeleeStunImmunity) + ",");
            sb.AppendLine(indent + "    CanBurn = " + V(playerModifiers.CanBurn) + ",");
            sb.AppendLine(indent + "    RunSpeedModifier = " + V(playerModifiers.RunSpeedModifier) + ",");
            sb.AppendLine(indent + "    SprintSpeedModifier = " + V(playerModifiers.SprintSpeedModifier) + ",");
            sb.AppendLine(indent + "    EnergyRechargeModifier = " + V(playerModifiers.EnergyRechargeModifier) + ",");
            sb.AppendLine(indent + "    SizeModifier = " + V(playerModifiers.SizeModifier) + ",");
            sb.AppendLine(indent + "    InfiniteAmmo = " + V(playerModifiers.InfiniteAmmo) + ",");
            sb.AppendLine(indent + "    ItemDropMode = " + V(playerModifiers.ItemDropMode) + ",");
            sb.AppendLine(indent + "};");

            return sb.ToString();
        }

        private string V(int value)
        {
            return value.ToString();
        }
        private string V(float value)
        {
            return value.ToString("0.0########").Replace(',', '.') + "f";
        }

        public string Convert(IObjectPlayerModifierInfo playerModifierInfo, int indentSize = 0)
        {
            return Convert(playerModifierInfo.GetModifiers(), indentSize);
        }
    }
}
