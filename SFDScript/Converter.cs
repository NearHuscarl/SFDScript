﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFDGameScriptInterface;

// Convert some markers info from Map Editor into C# code
namespace SFDScript.ProfileConverter
{
    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        public void OnStartup()
        {
            //var playerProfileInfo = (IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("ClownCowboy");
            //var profile = playerProfileInfo.GetProfile();
            //var profileStr = Convert(profile);

            var playerProfileInfos = new List<IObject>(Game.GetObjectsByCustomID("NinjaA"));
            System.Diagnostics.Debugger.Break();
            var playerProfileInfosList = playerProfileInfos.Select(Q => ((IObjectPlayerProfileInfo)Q).GetProfile()).ToList();
            var profilesStr = Convert(playerProfileInfosList);
        }

        public string Convert(PlayerModifiers playerModifiers, string indent = "")
        {
            if (playerModifiers == null) return "";

            var sb = new StringBuilder();

            sb.AppendLine(indent + "new PlayerModifiers()");
            sb.AppendLine(indent + "{");
            sb.Append(GetProperty(playerModifiers, "MaxHealth", "int", indent));
            sb.Append(GetProperty(playerModifiers, "MaxEnergy", "int", indent));
            sb.Append(GetProperty(playerModifiers, "CurrentHealth", "float", indent));
            sb.Append(GetProperty(playerModifiers, "CurrentEnergy", "float", indent));
            sb.Append(GetProperty(playerModifiers, "EnergyConsumptionModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "ExplosionDamageTakenModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "ProjectileDamageTakenModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "ProjectileCritChanceTakenModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "FireDamageTakenModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "MeleeDamageTakenModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "ImpactDamageTakenModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "ProjectileDamageDealtModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "ProjectileCritChanceDealtModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "MeleeDamageDealtModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "MeleeForceModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "MeleeStunImmunity", "int", indent));
            sb.Append(GetProperty(playerModifiers, "CanBurn", "int", indent));
            sb.Append(GetProperty(playerModifiers, "RunSpeedModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "SprintSpeedModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "EnergyRechargeModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "SizeModifier", "float", indent));
            sb.Append(GetProperty(playerModifiers, "InfiniteAmmo", "int", indent));
            sb.Append(GetProperty(playerModifiers, "ItemDropMode", "int", indent));
            sb.AppendLine(indent + "};");

            return sb.ToString();
        }

        private string GetProperty(object obj, string propertyName, string type, string indent)
        {
            var property = GetFieldValue(obj, propertyName, type);
            if (property == "") return "";

            return indent + "    " + property + "," + Environment.NewLine;
        }

        private string GetFieldValue(object obj, string propertyName, string type)
        {
            var stats = "";

            switch (type)
            {
                case "int":
                    stats = GetFieldValue<int>(obj, propertyName).ToString();
                    break;

                case "float":
                    // "0.00#######" -> Print at least 2 decimal digits
                    stats = GetFieldValue<float>(obj, propertyName).ToString("0.0########").Replace(',', '.');
                    break;
            }

            if (float.Parse(stats, System.Globalization.CultureInfo.InvariantCulture) == -1) // value unchanged
                return "";

            if (type == "float") stats += "f"; // 14.0 -> 14.0f

            return propertyName + " = " + stats;
        }

        private T GetFieldValue<T>(object obj, string propertyName)
        {
            return (T)obj.GetType().GetField(propertyName).GetValue(obj);
        }

        public string Convert(List<IProfile> profiles, string indent = "", string listName = "profiles")
        {
            var profileInfosBuilder = new StringBuilder();

            profileInfosBuilder.AppendLine("List<IProfile> " + listName + " = new List<IProfile>()");
            profileInfosBuilder.AppendLine("{");

            foreach (var profile in profiles)
            {
                profileInfosBuilder.Append(Convert(profile, "    "));
                profileInfosBuilder.Replace(';', ',', profileInfosBuilder.Length - 3, 1); // \r\n is 2 characters
            }

            profileInfosBuilder.AppendLine("};");

            return profileInfosBuilder.ToString();
        }

        public string Convert(IProfile profile, string indent = "")
        {
            if (profile == null) return "";

            var sb = new StringBuilder();

            sb.AppendLine(indent + "new IProfile()");
            sb.AppendLine(indent + "{");
            sb.AppendLine(indent + "    Name = \"" + profile.Name + "\",");
            sb.AppendLine(indent + "    Accesory = " + GetClothingItemInfo(profile.Accessory) + ",");
            sb.AppendLine(indent + "    ChestOver = " + GetClothingItemInfo(profile.ChestOver) + ",");
            sb.AppendLine(indent + "    ChestUnder = " + GetClothingItemInfo(profile.ChestUnder) + ",");
            sb.AppendLine(indent + "    Feet = " + GetClothingItemInfo(profile.Feet) + ",");
            sb.AppendLine(indent + "    Gender = Gender." + EnumToString(profile.Gender) + ",");
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

        private string EnumToString<T>(T enumVar)
        {
            return Enum.GetName(typeof(T), enumVar);
        }
    }
}
