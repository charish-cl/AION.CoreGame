using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SimpleSaveSystem
{
    /// <summary>
    /// 轻量级 JSON 序列化器 (支持 Dictionary, List, HashSet, Enum)
    /// </summary>
    public static class JsonSerializer
    {
        public static string Serialize(object obj)
        {
            if (obj == null) return "null";
            StringBuilder sb = new StringBuilder();
            SerializeValue(sb, obj, obj.GetType());
            return sb.ToString();
        }

        public static void Deserialize(string json, object target)
        {
            if (string.IsNullOrEmpty(json) || target == null) return;
            var tokens = Tokenize(json);
            int index = 0;
            DeserializeObject(tokens, ref index, target, target.GetType());
        }

        // --- 序列化部分 ---
        private static void SerializeValue(StringBuilder sb, object value, Type type)
        {
            if (value == null) { sb.Append("null"); return; }

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                if (value is bool b) sb.Append(b ? "true" : "false");
                else if (value is string s) sb.Append($"\"{EscapeString(s)}\"");
                else if (value is float f) sb.Append(f.ToString("G9"));
                else if (value is double d) sb.Append(d.ToString("G17"));
                else sb.Append(value);
            }
            else if (type.IsEnum) sb.Append((int)value);
            else if (typeof(IList).IsAssignableFrom(type)) SerializeList(sb, value);
            else if (typeof(IDictionary).IsAssignableFrom(type)) SerializeDictionary(sb, value);
            else SerializeObject(sb, value, type);
        }

        private static void SerializeObject(StringBuilder sb, object obj, Type type)
        {
            sb.Append('{');
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            bool first = true;
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                if (!first) sb.Append(',');
                sb.Append($"\"{field.Name}\":");
                SerializeValue(sb, field.GetValue(obj), field.FieldType);
                first = false;
            }
            sb.Append('}');
        }

        private static void SerializeList(StringBuilder sb, object list)
        {
            sb.Append('[');
            bool first = true;
            foreach (var item in (IEnumerable)list)
            {
                if (!first) sb.Append(',');
                SerializeValue(sb, item, item.GetType());
                first = false;
            }
            sb.Append(']');
        }

        private static void SerializeDictionary(StringBuilder sb, object dict)
        {
            sb.Append('{');
            bool first = true;
            foreach (DictionaryEntry kv in (IDictionary)dict)
            {
                if (!first) sb.Append(',');
                sb.Append($"\"{kv.Key}\":");
                SerializeValue(sb, kv.Value, kv.Value?.GetType() ?? typeof(object));
                first = false;
            }
            sb.Append('}');
        }

        private static string EscapeString(string str) => str?.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

        // --- 反序列化部分 ---
        private enum TokenType { ObjectStart, ObjectEnd, ArrayStart, ArrayEnd, Colon, Comma, String, Number, True, False, Null }
        private class Token { public TokenType Type; public string Value; }

        private static List<Token> Tokenize(string json)
        {
            var tokens = new List<Token>();
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (char.IsWhiteSpace(c)) continue;
                if (c == '{') tokens.Add(new Token { Type = TokenType.ObjectStart });
                else if (c == '}') tokens.Add(new Token { Type = TokenType.ObjectEnd });
                else if (c == '[') tokens.Add(new Token { Type = TokenType.ArrayStart });
                else if (c == ']') tokens.Add(new Token { Type = TokenType.ArrayEnd });
                else if (c == ':') tokens.Add(new Token { Type = TokenType.Colon });
                else if (c == ',') tokens.Add(new Token { Type = TokenType.Comma });
                else if (c == '"')
                {
                    int start = ++i;
                    while (i < json.Length && (json[i] != '"' || json[i - 1] == '\\')) i++;
                    tokens.Add(new Token { Type = TokenType.String, Value = UnescapeString(json.Substring(start, i - start)) });
                }
                else if (char.IsDigit(c) || c == '-')
                {
                    int start = i;
                    while (i + 1 < json.Length && (char.IsDigit(json[i + 1]) || json[i + 1] == '.')) i++;
                    tokens.Add(new Token { Type = TokenType.Number, Value = json.Substring(start, i - start + 1) });
                }
                else if (json.Substring(i).StartsWith("true")) { tokens.Add(new Token { Type = TokenType.True }); i += 3; }
                else if (json.Substring(i).StartsWith("false")) { tokens.Add(new Token { Type = TokenType.False }); i += 4; }
                else if (json.Substring(i).StartsWith("null")) { tokens.Add(new Token { Type = TokenType.Null }); i += 3; }
            }
            return tokens;
        }

        private static string UnescapeString(string str) => str.Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").Replace("\\\\", "\\");

        private static void DeserializeObject(List<Token> tokens, ref int index, object target, Type type)
        {
            if (tokens[index++].Type != TokenType.ObjectStart) return;
            var fields = new Dictionary<string, FieldInfo>();
            foreach (var f in type.GetFields()) fields[f.Name] = f;

            while (index < tokens.Count && tokens[index].Type != TokenType.ObjectEnd)
            {
                string key = tokens[index++].Value;
                index++; // Skip :
                if (fields.TryGetValue(key, out var field)) field.SetValue(target, DeserializeValue(tokens, ref index, field.FieldType));
                else SkipValue(tokens, ref index); // Skip unknown fields
                if (tokens[index].Type == TokenType.Comma) index++;
            }
            index++; // Skip }
        }

        private static object DeserializeValue(List<Token> tokens, ref int index, Type type)
        {
            var token = tokens[index];
            if (token.Type == TokenType.Null) { index++; return null; }
            if (token.Type == TokenType.True) { index++; return true; }
            if (token.Type == TokenType.False) { index++; return false; }
            if (token.Type == TokenType.Number) { index++; return Convert.ChangeType(token.Value, type.IsEnum ? typeof(int) : type); }
            if (token.Type == TokenType.String) { index++; return token.Value; }
            if (token.Type == TokenType.ArrayStart) return DeserializeList(tokens, ref index, type);
            if (token.Type == TokenType.ObjectStart)
            {
                if (typeof(IDictionary).IsAssignableFrom(type)) return DeserializeDictionary(tokens, ref index, type);
                var obj = Activator.CreateInstance(type);
                DeserializeObject(tokens, ref index, obj, type);
                return obj;
            }
            return null;
        }

        private static object DeserializeList(List<Token> tokens, ref int index, Type type)
        {
            index++; // Skip [
            bool isHashSet = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
            var listType = isHashSet ? typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]) : type;
            var list = (IList)Activator.CreateInstance(listType);
            Type elemType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);

            while (tokens[index].Type != TokenType.ArrayEnd)
            {
                list.Add(DeserializeValue(tokens, ref index, elemType));
                if (tokens[index].Type == TokenType.Comma) index++;
            }
            index++; // Skip ]
            
            if (isHashSet)
            {
                var set = Activator.CreateInstance(type);
                var add = type.GetMethod("Add");
                foreach (var item in list) add.Invoke(set, new[] { item });
                return set;
            }
            return list;
        }

        private static object DeserializeDictionary(List<Token> tokens, ref int index, Type type)
        {
            index++; // Skip {
            var dict = (IDictionary)Activator.CreateInstance(type);
            Type keyType = type.GetGenericArguments()[0], valType = type.GetGenericArguments()[1];
            while (tokens[index].Type != TokenType.ObjectEnd)
            {
                object key = Convert.ChangeType(tokens[index++].Value, keyType);
                index++; // Skip :
                dict[key] = DeserializeValue(tokens, ref index, valType);
                if (tokens[index].Type == TokenType.Comma) index++;
            }
            index++; // Skip }
            return dict;
        }

        private static void SkipValue(List<Token> tokens, ref int index)
        {
            int depth = 0;
            do
            {
                var t = tokens[index++];
                if (t.Type == TokenType.ObjectStart || t.Type == TokenType.ArrayStart) depth++;
                else if (t.Type == TokenType.ObjectEnd || t.Type == TokenType.ArrayEnd) depth--;
            } while (depth > 0);
        }
    }
}