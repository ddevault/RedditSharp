using Newtonsoft.Json;
using System;

namespace RedditSharp
{
    public enum ModActionType
    {
        BanUser,
        UnBanUser,
        RemoveLink,
        ApproveLink,
        RemoveComment,
        ApproveComment,
        AddModerator,
        InviteModerator,
        UnInviteModerator,
        AcceptModeratorInvite,
        RemoveModerator,
        AddContributor,
        RemoveContributor,
        EditSettings,
        EditFlair,
        Distinguish,
        MarkNSFW,
        WikiBanned,
        WikiContributor,
        WikiUnBanned,
        WikiPageListed,
        RemoveWikiContributor,
        WikiRevise,
        WikiPermlevel,
        IgnoreReports,
        UnIgnoreReports,
        SetPermissions,
        SetSuggestedsort,
        Sticky,
        UnSticky,
        SetContestMode,
        UnSetContestMode,
        LockPost, //actual value is "Lock" but it's a reserved word
        Unlock,
        MuteUser,
        UnMuteUser
    }

    public class ModActionTypeConverter : JsonConverter
    {
        /// <summary>
        /// Replaces "LockPost" with "lock" since "lock" is a reserved word and can't be used in the enum
        /// </summary>
        /// <returns>String representation of enum value recognized by Reddit's api</returns>
        public static string GetRedditParamName(ModActionType action)
        {
            if (action == ModActionType.LockPost) return "lock";
            else return action.ToString("g").ToLower();
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ModActionType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = reader.Value.ToString();
            if (value.ToLower() == "lock")
            {
                return ModActionType.LockPost;
            }
            else
            {
                return Enum.Parse(typeof(ModActionType), value, true);
            }

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) writer.WriteNull();
            else writer.WriteValue(GetRedditParamName((ModActionType) value));
        }
    }
}
