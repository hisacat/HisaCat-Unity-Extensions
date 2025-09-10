using System;
using UnityEngine;

namespace HisaCat
{
    public class EnumStringAttribute : PropertyAttribute
    {
        private readonly Type enumType;
        public EnumStringAttribute(Type enumType)
        {
            this.enumType = enumType;
        }

        public Type EnumType => this.enumType;

        public static TEnum Convert<TEnum>(string enumString, TEnum defaultValue = default) where TEnum : struct
        {
            if (System.Enum.TryParse<TEnum>(enumString, out var result))
                return result;

            Debug.LogError($"[EnumStringAttribute] Cannot parse to <b>{typeof(TEnum).Name}</b> enum from string: <b>\"{enumString}\"</b>. use default value <b>\"{defaultValue}\"</b>");
            return defaultValue;
        }
    }
}
