using System;

namespace RedditSharp
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumStringAttribute : Attribute
    {
        public EnumStringAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public static string EnumToString(Enum value)
        {
            var type = value.GetType();

            var memInfo = type.GetMember(value.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(EnumStringAttribute),
                false);

            if (attributes.Length == 0)
                return null;

            return ((EnumStringAttribute)attributes[0]).Name;
        }
    }
}