using System;
using System.Collections.Generic;
using System.Text;
using SFDGameScriptInterface;

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
            var playerProfileInfo = (IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("CowboyA");
            var profile = playerProfileInfo.GetProfile();
            var profileStr = ConvertProfileToString(profile);

            var profiles = new List<IProfile>
            {
                ((IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("CowboyA")).GetProfile(),
                ((IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("CowboyB")).GetProfile(),
                ((IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("CowboyC")).GetProfile(),
                ((IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("CowboyD")).GetProfile(),
                ((IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("CowboyE")).GetProfile(),
                ((IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("CowboyF")).GetProfile(),
                ((IObjectPlayerProfileInfo)Game.GetSingleObjectByCustomID("CowboyG")).GetProfile(),
            };
            var profilesStr = ConvertProfilesToString(profiles);

            System.Diagnostics.Debugger.Break();
        }

        public string ConvertProfilesToString(List<IProfile> profiles, string listName = "profiles")
        {
            var profileInfosBuilder = new StringBuilder();

            profileInfosBuilder.AppendLine("List<IProfile> " + listName + " = new List<IProfile>()");
            profileInfosBuilder.AppendLine("{");

            foreach (var profile in profiles)
            {
                profileInfosBuilder.Append(ConvertProfileToString(profile, "    "));
                profileInfosBuilder.Replace(';', ',', profileInfosBuilder.Length - 3, 1); // \r\n is 2 characters
            }

            profileInfosBuilder.AppendLine("};");

            return profileInfosBuilder.ToString();
        }

        public string ConvertProfileToString(IProfile profile, string indent = "")
        {
            var profileInfoBuilder = new StringBuilder();

            profileInfoBuilder.AppendLine(indent + "new IProfile()");
            profileInfoBuilder.AppendLine(indent + "{");
            profileInfoBuilder.AppendLine(indent + "    Name = \"" + profile.Name + "\",");
            profileInfoBuilder.AppendLine(indent + "    Accesory = " + GetClothesInfo(profile.Accessory) + ",");
            profileInfoBuilder.AppendLine(indent + "    ChestOver = " + GetClothesInfo(profile.ChestOver) + ",");
            profileInfoBuilder.AppendLine(indent + "    ChestUnder = " + GetClothesInfo(profile.ChestUnder) + ",");
            profileInfoBuilder.AppendLine(indent + "    Feet = " + GetClothesInfo(profile.Feet) + ",");
            profileInfoBuilder.AppendLine(indent + "    Gender = Gender." + EnumToString(profile.Gender) + ",");
            profileInfoBuilder.AppendLine(indent + "    Hands = " + GetClothesInfo(profile.Hands) + ",");
            profileInfoBuilder.AppendLine(indent + "    Head = " + GetClothesInfo(profile.Head) + ",");
            profileInfoBuilder.AppendLine(indent + "    Legs = " + GetClothesInfo(profile.Legs) + ",");
            profileInfoBuilder.AppendLine(indent + "    Skin = " + GetClothesInfo(profile.Skin) + ",");
            profileInfoBuilder.AppendLine(indent + "    Waist = " + GetClothesInfo(profile.Waist) + ",");
            profileInfoBuilder.AppendLine(indent + "};");

            return profileInfoBuilder.ToString();
        }

        private string GetClothesInfo(IProfileClothingItem clothingItem)
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
