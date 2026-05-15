using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace PlayablesPlugins
{
    public static class LunaParticlesTunerDataParsing
    {
        public static bool TryGetProperty(Dictionary<string, PropertyInfo> allProperties, string key, out PropertyInfo property, out string resolvedKey)
        {
            if (allProperties.TryGetValue(key, out property) && property != null)
            {
                resolvedKey = key;
                return true;
            }

            var normalizedName = GetSettingName(key);
            foreach (var pair in allProperties)
            {
                if (!string.Equals(GetSettingName(pair.Key), normalizedName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                property = pair.Value;
                resolvedKey = pair.Key;
                return property != null;
            }

            property = null;
            resolvedKey = null;
            return false;
        }

        public static string GetSettingType(string rawKey)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
            {
                return "float";
            }

            var firstSep = rawKey.IndexOf('|');
            if (firstSep < 0 || firstSep >= rawKey.Length - 1)
            {
                return "float";
            }

            var rest = rawKey.Substring(firstSep + 1);
            var secondSep = rest.IndexOf('|');
            return (secondSep < 0 ? rest : rest.Substring(0, secondSep)).ToLowerInvariant();
        }

        public static bool TrySerializeValueByDeclaredType(string rawKey, object value, out string serializedValue)
        {
            switch (GetSettingType(rawKey))
            {
                case "select_one":
                    if (TryConvertToSelectOne(value, out var selectOneSerValue))
                    {
                        serializedValue = $"\"{EscapeJsonString(selectOneSerValue)}\"";
                        return true;
                    }

                    serializedValue = null;
                    return false;
                case "float_with_random":
                    if (TryConvertToFloatWithRandom(value, out var randomFloatValue))
                    {
                        serializedValue = $"\"{EscapeJsonString(randomFloatValue)}\"";
                        return true;
                    }

                    serializedValue = null;
                    return false;
                case "bool":
                    if (TryConvertToBool(value, out var boolSerValue))
                    {
                        serializedValue = boolSerValue ? "true" : "false";
                        return true;
                    }

                    serializedValue = null;
                    return false;
                case "int":
                    if (TryConvertToFloat(value, out var intNumericValue))
                    {
                        serializedValue = Mathf.RoundToInt(intNumericValue).ToString();
                        return true;
                    }

                    serializedValue = null;
                    return false;
                case "color":
                    if (TryConvertToColor(value, out var colorValue))
                    {
                        serializedValue = $"\"#{ColorToHexRgba(colorValue)}\"";
                        return true;
                    }

                    serializedValue = null;
                    return false;
                case "vector_3":
                    if (TryConvertToVector3(value, out var vector3Value))
                    {
                        serializedValue = $"\"{EscapeJsonString(vector3Value)}\"";
                        return true;
                    }

                    serializedValue = null;
                    return false;
                default:
                    if (TryConvertToFloat(value, out var floatValue))
                    {
                        serializedValue = floatValue.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }

                    serializedValue = null;
                    return false;
            }
        }

        public static bool TryConvertByDeclaredType(string rawKey, object value, out object convertedValue)
        {
            switch (GetSettingType(rawKey))
            {
                case "select_one":
                    if (TryConvertToSelectOne(value, out var selectOneValue))
                    {
                        convertedValue = selectOneValue;
                        return true;
                    }

                    convertedValue = null;
                    return false;
                case "float_with_random":
                    if (TryConvertToFloatWithRandom(value, out var randomFloatValue))
                    {
                        convertedValue = randomFloatValue;
                        return true;
                    }

                    convertedValue = null;
                    return false;
                case "bool":
                    if (TryConvertToBool(value, out var boolConvValue))
                    {
                        convertedValue = boolConvValue;
                        return true;
                    }

                    convertedValue = null;
                    return false;
                case "int":
                    if (TryConvertToFloat(value, out var intNumericValue))
                    {
                        convertedValue = Mathf.RoundToInt(intNumericValue);
                        return true;
                    }

                    convertedValue = null;
                    return false;
                case "color":
                    if (TryConvertToColor(value, out var colorValue))
                    {
                        convertedValue = colorValue;
                        return true;
                    }

                    convertedValue = null;
                    return false;
                case "vector_3":
                    if (TryConvertToVector3(value, out var vector3ConvValue))
                    {
                        convertedValue = vector3ConvValue;
                        return true;
                    }

                    convertedValue = null;
                    return false;
                default:
                    if (TryConvertToFloat(value, out var floatValue))
                    {
                        convertedValue = floatValue;
                        return true;
                    }

                    convertedValue = null;
                    return false;
            }
        }

        public static string ExtractSettingsJson(string data, int id, bool ignoreId = false)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            var targetId = id.ToString();
            var searchStartIndex = 0;

            while (searchStartIndex < data.Length)
            {
                var idKeyIndex = IndexOfKey(data, "id", searchStartIndex);
                if (idKeyIndex < 0)
                {
                    return null;
                }

                var idValueStart = data.IndexOf(':', idKeyIndex);
                if (idValueStart < 0)
                {
                    return null;
                }

                idValueStart++;
                while (idValueStart < data.Length && char.IsWhiteSpace(data[idValueStart]))
                {
                    idValueStart++;
                }

                if (idValueStart >= data.Length)
                {
                    return null;
                }

                string currentId;
                int idValueEnd;

                if (data[idValueStart] == '"')
                {
                    idValueStart++;
                    idValueEnd = data.IndexOf('"', idValueStart);
                    if (idValueEnd < 0)
                    {
                        return null;
                    }

                    currentId = data.Substring(idValueStart, idValueEnd - idValueStart);
                    idValueEnd++;
                }
                else
                {
                    idValueEnd = idValueStart;
                    while (idValueEnd < data.Length && data[idValueEnd] != ',' && data[idValueEnd] != '}' && data[idValueEnd] != ']')
                    {
                        idValueEnd++;
                    }

                    currentId = data.Substring(idValueStart, idValueEnd - idValueStart).Trim();
                }

                if (ignoreId || string.Equals(currentId, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    var settingsKeyIndex = IndexOfKey(data, "settings", idValueEnd);
                    if (settingsKeyIndex < 0)
                    {
                        return null;
                    }

                    var settingsValueStart = data.IndexOf(':', settingsKeyIndex);
                    if (settingsValueStart < 0)
                    {
                        return null;
                    }

                    var settingsBraceStart = data.IndexOf('{', settingsValueStart);
                    if (settingsBraceStart < 0)
                    {
                        return null;
                    }

                    var settingsBraceEnd = FindMatchingBrace(data, settingsBraceStart);
                    if (settingsBraceEnd < 0)
                    {
                        return null;
                    }

                    return data.Substring(settingsBraceStart, settingsBraceEnd - settingsBraceStart + 1);
                }

                searchStartIndex = idValueEnd;
            }

            return null;
        }

        public static Dictionary<string, object> ParseFlatObject(string source)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(source))
            {
                return result;
            }

            var text = source.Trim();
            if (text.StartsWith("{") && text.EndsWith("}"))
            {
                text = text.Substring(1, text.Length - 2);
            }

            var pairs = text.Split(',');
            for (var i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                var separatorIndex = pair.IndexOf(':');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = pair.Substring(0, separatorIndex).Trim().Trim('"', '\'');
                var rawValue = pair.Substring(separatorIndex + 1).Trim().Trim('"', '\'');
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                result[key] = ParseScalar(rawValue);
            }

            return result;
        }

        private static string GetSettingName(string rawKey)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
            {
                return string.Empty;
            }

            var firstSep = rawKey.IndexOf('|');
            if (firstSep <= 0)
            {
                return rawKey;
            }

            return rawKey.Substring(0, firstSep);
        }

        private static bool TryConvertToBool(object value, out bool result)
        {
            switch (value)
            {
                case bool b:
                    result = b;
                    return true;
                case string s:
                    if (bool.TryParse(s, out result))
                    {
                        return true;
                    }

                    if (s == "1") { result = true; return true; }
                    if (s == "0") { result = false; return true; }
                    break;
                case int i:
                    result = i != 0;
                    return true;
                case float f:
                    result = f != 0f;
                    return true;
            }

            result = default;
            return false;
        }

        private static bool TryConvertToFloatWithRandom(object value, out string result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            // DO NOT SIMPLIFY THIS PARSING OF VALUE!
            if (value is float || value is double || value is int || value is long || value is decimal)
            {
                if (TryConvertToFloat(value, out var numericValue))
                {
                    result = numericValue.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
            }

            if (value is string raw)
            {
                var text = raw.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    result = default;
                    return false;
                }

                if (text.Contains("curve"))
                {
                    result = text;
                    return true;
                }

                var split = text.Split('|');
                if (split.Length == 1)
                {
                    if (float.TryParse(split[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var singleValue) ||
                        float.TryParse(split[0].Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out singleValue))
                    {
                        result = singleValue.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }

                    result = default;
                    return false;
                }

                if (split.Length == 2)
                {
                    if (!(float.TryParse(split[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var minValue) ||
                          float.TryParse(split[0].Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out minValue)))
                    {
                        result = default;
                        return false;
                    }

                    if (!(float.TryParse(split[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var maxValue) ||
                          float.TryParse(split[1].Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out maxValue)))
                    {
                        result = default;
                        return false;
                    }

                    result = minValue.ToString(CultureInfo.InvariantCulture) + "|" +
                             maxValue.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static bool TryConvertToSelectOne(object value, out string result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            switch (value)
            {
                case string s:
                    result = s;
                    return true;
                case bool b:
                    result = b ? "true" : "false";
                    return true;
                default:
                    result = Convert.ToString(value, CultureInfo.InvariantCulture);
                    return result != null;
            }
        }

        private static bool TryConvertToVector3(object value, out string result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            if (value is string s)
            {
                var text = s.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    result = default;
                    return false;
                }

                var parts = text.Split('|');
                if (parts.Length == 3)
                {
                    if (float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out _) &&
                        float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out _) &&
                        float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        result = text;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        private static string EscapeJsonString(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static int IndexOfKey(string source, string key, int startIndex)
        {
            var quotedKey = $"\"{key}\"";
            var quotedIndex = source.IndexOf(quotedKey, startIndex, StringComparison.OrdinalIgnoreCase);
            if (quotedIndex >= 0)
            {
                return quotedIndex;
            }

            return source.IndexOf(key, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        private static int FindMatchingBrace(string source, int startIndex)
        {
            var depth = 0;
            for (var i = startIndex; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static object ParseScalar(string value)
        {
            if (string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue;
            }

            return value;
        }

        private static bool TryConvertToFloat(object value, out float result)
        {
            switch (value)
            {
                case float f:
                    result = f;
                    return true;
                case double d:
                    result = (float)d;
                    return true;
                case int i:
                    result = i;
                    return true;
                case long l:
                    result = l;
                    return true;
                case decimal m:
                    result = (float)m;
                    return true;
                case string s:
                    if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                    {
                        return true;
                    }

                    if (float.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
                    {
                        return true;
                    }

                    break;
            }

            result = default;
            return false;
        }

        private static bool TryConvertToColor(object value, out Color result)
        {
            if (value is Color color)
            {
                result = color;
                return true;
            }

            if (value is string colorString)
            {
                var trimmed = colorString.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    result = default;
                    return false;
                }

                if (ColorUtility.TryParseHtmlString(trimmed, out result))
                {
                    return true;
                }

                if (ColorUtility.TryParseHtmlString("#" + trimmed.TrimStart('#'), out result))
                {
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static string ColorToHexRgba(Color color)
        {
            var r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255);
            var g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255);
            var b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255);
            var a = Mathf.Clamp(Mathf.RoundToInt(color.a * 255f), 0, 255);
            return $"{r:X2}{g:X2}{b:X2}{a:X2}";
        }
    }
}