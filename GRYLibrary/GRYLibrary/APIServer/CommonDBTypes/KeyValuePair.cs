using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Globalization;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer.CommonDBTypes
{
    [PrimaryKey(nameof(Key))]
    public class KeyValuePair
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

        public static uint GetUint(DbSet<KeyValuePair> kvps, string name)
        {
            return GetValue(kvps, name, uint.Parse);
        }

        public static void SetUint(DbSet<KeyValuePair> kvps, string name, uint value)
        {
            SetValue(kvps, name, x => x.ToString(), value);
        }

        public static bool GetBool(DbSet<KeyValuePair> kvps, string name)
        {
            return GetValue(kvps, name, bool.Parse);
        }

        public static void SetBool(DbSet<KeyValuePair> kvps, string name, bool value)
        {
            SetValue(kvps, name, x => x.ToString(), value);
        }

        public static decimal GetDecimal(DbSet<KeyValuePair> kvps, string name)
        {
            //The value gets serialized culture-invariant in SetDecimal, so it must also get parsed culture-invariant.
            return GetValue(kvps, name, value => decimal.Parse(value, CultureInfo.InvariantCulture));
        }

        public static void SetDecimal(DbSet<KeyValuePair> kvps, string name, decimal value)
        {
            SetValue(kvps, name, x => x.ToString(CultureInfo.InvariantCulture), value);
        }

        public static DateTime GetDateTime(DbSet<KeyValuePair> kvps, string name)
        {
            return GetValue(kvps, name, GUtilities.DateTimeParse);
        }

        public static void SetDateTime(DbSet<KeyValuePair> kvps, string name, DateTime value)
        {
            SetValue(kvps, name, GUtilities.DateTimeToString, value);
        }

        public static string GetString(DbSet<KeyValuePair> kvps, string name)
        {
            return GetValue(kvps, name, x => x);
        }

        public static void SetString(DbSet<KeyValuePair> kvps, string name, string value)
        {
            SetValue(kvps, name, x => x, value);
        }

        public static bool HasKey(DbSet<KeyValuePair> kvps, string name)
        {
            return kvps.Where(kvp => kvp.Key == name).Any();
        }

        public static T GetValue<T>(DbSet<KeyValuePair> kvps, string name, Func<string, T> deserialize)
        {
            return deserialize(kvps.Where(kvp => kvp.Key == name).First().Value);
        }

        public static void SetValue<T>(DbSet<KeyValuePair> kvps, string name, Func<T, string> serialize, T value)
        {
            KeyValuePair kvp = kvps.Where(kvp => kvp.Key == name).FirstOrDefault();
            if (kvp == default)
            {
                kvps.Add(new KeyValuePair()
                {
                    Key = name,
                    Value = serialize(value)
                });
            }
            else
            {
                kvp.Value = serialize(value);
            }
        }
    }
}
