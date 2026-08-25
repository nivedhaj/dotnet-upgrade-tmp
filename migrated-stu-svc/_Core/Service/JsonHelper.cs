using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace ct._Core.Service
{
    /// <summary>
    /// Helper for Json
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object Deserialize(string json)
        {
            return ToObject(JToken.Parse(json));
        }

        private static object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                        .ToDictionary(prop => prop.Name,
                            prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue)token).Value;
            }
        }

        /// <summary>
        /// Flatten a Json object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, object> Flatten(string json)
        {
            var converter = new ExpandoObjectConverter();
            dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(json, converter);
            var dict = new Dictionary<string, object>();
            generateDictionary((ExpandoObject)dyn, dict, "");
            return dict;
        }
        /// <summary>
        /// Use this as it handles deeply nested json and array indices
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Flatten2(string json)
        {
            var jo = JObject.Parse(json);
            IEnumerable<JToken> jTokens = jo.Descendants().Where(p => p.Count() == 0);
            var dict = jTokens.Aggregate(new Dictionary<string, string>(), (properties, jToken) =>
            {
                properties.Add(jToken.Path, jToken.ToString());
                return properties;
            });
            return dict;
        }

        private static void generateDictionary(ExpandoObject output, Dictionary<string, object> dict, string parent)
        {
            foreach (var v in output)
            {
                var key = parent + v.Key;
                var o = v.Value;
                if (o == null)
                {
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, o);
                    }
                }
                else if (o.GetType() == typeof(ExpandoObject))
                {
                    generateDictionary((ExpandoObject)o, dict, key + ".");
                }
                else if (o is IList)
                {
                    var list = new List<object>();
                    foreach (dynamic x in (IList)o)
                    {
                        var ns = x.GetType().Namespace;
                        if (ns == "System")
                            list.Add(x);
                        else
                            generateDictionary((ExpandoObject)x, dict, key + ".");
                    }

                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, string.Join(",", list));
                    }
                }
                else
                {
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, o);
                    }
                }
            }
        }

        /// <summary>
        /// Serialize a dynamic object and retain casing
        /// </summary>
        /// <param name="dyn"></param>
        /// <returns></returns>
        public static string Serialize(dynamic dyn)
        {
            var result = JsonConvert.SerializeObject(dyn);
            return result;
        }
        
        /// <summary>
        /// Serialize an object and camelCase the props
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeToCamelCase(object obj)
        {
            var contractResolver = new DefaultContractResolver{NamingStrategy = new CamelCaseNamingStrategy()};
            var jsonSettings = new JsonSerializerSettings {ContractResolver = contractResolver};
            var json = JsonConvert.SerializeObject(obj, jsonSettings);
            return json;
        }
        
        /// <summary>
        /// Walk Tokens
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<JToken> WalkTokens(this JToken node)
        {
            if (node == null)
                yield break;
            yield return node;
            foreach (var child in node.Children())
                foreach (var childNode in child.WalkTokens())
                    yield return childNode;
        }

        public static IDictionary<string, object> ToValueDictionary(this JToken root)
        {
            return root.WalkTokens().OfType<JValue>().ToDictionary(value => value.Path, value => value.Value);
        }
    }
}