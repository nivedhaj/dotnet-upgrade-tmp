using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ct._Core.Util
{
    /// <summary>
    /// Helper methods
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Compute hash for string encoded as UTF8 - SHA1
        /// </summary>
        /// <param name="input">String to be hashed - UTF 8 only</param>
        /// <returns>40-character hex string</returns>
        public static string SHA1FromString(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = null;
            SHA1 sha = new SHA1CryptoServiceProvider();
            // This is one implementation of the abstract class SHA1.
            hash = sha.ComputeHash(inputBytes);

            return HexStringFromBytes(hash);
        }
        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("X2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
        //ref: https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
        public static double BytesToKb(long value, int decimalPlaces = 2)
        {
            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            double adjustedSize = (double)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }
            return adjustedSize;
        }

        //GUID
        public static Guid Parse(string input)
        {
            var guid = Guid.Empty;
            Guid.TryParse(input, out guid);
            return guid;
        }
        public static bool IsValidGuid(string input)
        {
            return Parse(input) != Guid.Empty;
        }

        //JSON
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

        public static Dictionary<string, object> Flatten(string json)
        {
            var converter = new ExpandoObjectConverter();
            dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(json, converter);
            var dict = new Dictionary<string, object>();
            generateDictionary((ExpandoObject)dyn, dict, "");
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
        /// Serialize
        /// </summary>
        /// <param name="dyn"></param>
        /// <returns></returns>
        public static string Serialize(dynamic dyn)
        {
            var result = JsonConvert.SerializeObject(dyn);
            return result;
        }

        /// <summary>
        ///     Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default(T);

            Stream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(jsonWriter, source);
                jsonWriter.Flush();
            }
            using (StreamReader reader = new StreamReader(stream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        /// <summary>
        /// Is Valid Mobile
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static bool IsValidMobile(string mobile)
        {
            return !string.IsNullOrEmpty(mobile) && mobile.Length == 10;
        }
        /// <summary>
        /// Is Valid Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            var regex = new Regex(@"\s");
            var email_contains_white_space = regex.IsMatch(email);
            if (email_contains_white_space) return false;

            try
            {
                var addr = new MailAddress(email); //will throw if email is invalid
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Capitalize Name etc
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            //If passed in string contains whitespace then capitalize individual elements.
            s = s.Trim();
            var arr = s.Split(null); //split on white space.
            for (var i = 0; i < arr.Length; i++)
            {
                var t = arr[i];
                if (string.IsNullOrEmpty(t)) continue;
                arr[i] = char.ToUpper(t[0]) + t.Substring(1).ToLower();
            }
            //replace multiple whitespaces with a single space
            //ref:http://stackoverflow.com/questions/206717/how-do-i-replace-multiple-spaces-with-a-single-space-in-c
            var options = RegexOptions.None;
            var regex = new Regex(@"[ ]{2,}", options);
            var result = string.Join(" ", arr);
            result = regex.Replace(result, @" ");

            return result;
        }
        /// <summary>
        /// Get Header from HttpRequest. Returns default(T) if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequest"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetHeader<T>(HttpRequest httpRequest, string key)
        {
            T result = default(T);
            try
            {
                var headers = httpRequest.Headers.FirstOrDefault(o => o.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)).Value;
                if (!StringValues.IsNullOrEmpty(headers))
                {
                    var header = headers.FirstOrDefault();
                    header = header.Replace("Bearer", "").Trim(); //For auth 
                    result = (T)Convert.ChangeType(header, typeof(T));
                }
            }
            catch { } //do nothing
            return result;
        }

        /// <summary>
        /// Csv to list
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static IEnumerable<T> CsvToList<T>(string[] csv)
        {
            var cols = csv.First().Split(",");
            var rows = csv.Skip(1);
            var result = new List<T>();
            foreach (var row in rows)
            {
                var jo = new JObject();
                var values = SplitCsvRow(row);
                for (int i = 0; i < cols.Count(); i++)
                {
                    var t = values[i].GetType();
                    jo.Add(cols[i], values[i]);
                }
                var obj = jo.ToObject<T>();
                result.Add(obj);
            }
            return result;
        }

        /// <summary>
        /// Split csv row
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string[] SplitCsvRow(string input)
        {
            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
            List<string> list = new List<string>();
            string curr = null;
            foreach (Match match in csvSplit.Matches(input))
            {
                curr = match.Value;
                if (0 == curr.Length)
                {
                    list.Add("");
                }

                list.Add(curr.TrimStart(','));
            }
            return list.ToArray();
        }

        /// <summary>
        /// Serialize to snake_case. Method name convention intentionally follows snake casing.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string serialize_to_snake_case(object obj)
        {
            var snake_json_serializer = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            string json_string = JsonConvert.SerializeObject(obj, snake_json_serializer);

            return json_string;
        }

        /// <summary>
        /// Get bytes from object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] GetBytesFromObject(object obj)
        {
            string _s = JsonConvert.SerializeObject(obj);
            return GetBytesFromObject(_s);
        }
        /// <summary>
        /// String as Bytes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] GetBytesFromObject(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return bytes;
        }

        /// <summary>
        /// Remove a repeating pattern from string.
        /// x x => x; abc abc => abc; x y x y => x y
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string DeduplicatePattern(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var result = s;
            var arr = s.Split(null);
            if (arr.Length >= 2 && arr.Length % 2 == 0)
            {
                var pattern_arr = new List<List<string>>();
                var chunk_size = arr.Length / 2;
                foreach (var x in arr.Chunk(chunk_size))
                {
                    pattern_arr.Add(x.ToList());
                }
                var pattern1 = string.Join(" ", pattern_arr[0]).Trim();
                var pattern2 = string.Join(" ", pattern_arr[1]).Trim();
                if (pattern1 == pattern2)
                {
                    result = pattern1;
                }
            }
            return result;
        }

        /// <summary>
        /// /// Basic sanitization of input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Sanitize(string input)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(input))
            {
                var options = RegexOptions.None;
                var regex = new Regex(@"[ ]{2,}", options);
                result = regex.Replace(input, @" ").Trim();
            }
            return result;
        }

        /// <summary>
        /// Sanitize an array of strings
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static List<string> Sanitize(List<string> inputs)
        {
            var result = new List<string>();
            if (inputs == null || inputs.Count == 0) return result;
            return inputs.Select(o => Sanitize(o)).ToList();
        }
    }
}