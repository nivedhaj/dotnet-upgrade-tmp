using System;
using ct._Core.Repo.Dto;
using Newtonsoft.Json;
using StackExchange.Redis;
using Repo.Dto;
using System.Collections.Generic;
using Repo.Db.Index;
using System.Linq;

namespace ct._Core.Service
{
    /// <summary>
    /// Redis Cache
    /// </summary>
    public class RedisCache
    {
        /// <summary>
        /// Redist Enpoint - Set in StartUp.cs
        /// </summary>
        public static string Url;
        /// <summary>
        /// Redis port
        /// </summary>
        public static int Port;
        /// <summary>
        /// Password
        /// </summary>
        public static string Password;
        /// <summary>
        /// Redi Connection
        /// </summary>
        public static ConnectionMultiplexer Connection { get { return LazyConnection.Value; } }

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection =

            new Lazy<ConnectionMultiplexer>(
                () =>
                {
                    return
                        ConnectionMultiplexer.Connect(new ConfigurationOptions
                        {
                            AbortOnConnectFail = false,
                            EndPoints = { { Url, Port } },
                            SyncTimeout = 9000,
                            Password = Password
                        });
                });

        internal static string GetKey_Org(string id)
        {
            var _prefix = "org";
            return $"{_prefix}:{id}";
        }
        internal static string GetKey_Course(string inst_id)
        {
            var _prefix = "course";
            return $"{_prefix}:{inst_id}";
        }
        internal static string GetKey_Inst(string id)
        {
            var _prefix = "inst";
            return $"{_prefix}:{id}";
        }
        internal static string GetKey_Scope(string id)
        {
            var _prefix = "scope";
            return $"{_prefix}:{id}";
        }
        internal static string GetKey_Student(string id)
        {
            var _prefix = "student";
            return $"{_prefix}:{id}";
        }

        internal static ClientDto GetClientDto(string clientId)
        {
            var redis = Connection.GetDatabase();
            var client_dto_string = redis.StringGet(RedisCache.GetKey_Inst(clientId));
            var client_dto = JsonConvert.DeserializeObject<ClientDto>(client_dto_string);
            return client_dto;
        }
        internal static List<CourseDto> GetCourseDto(string clientId)
        {
            var redis = Connection.GetDatabase();
            var isConnected = Connection.IsConnected;
            if (!isConnected) return new List<CourseDto>();
            var course_dto_string = redis.StringGet(RedisCache.GetKey_Course(clientId));
            if (string.IsNullOrEmpty(course_dto_string)) return new List<CourseDto>();
            var course_dto = JsonConvert.DeserializeObject<List<CourseDto>>(course_dto_string);
            return course_dto;
        }
        /// <summary>
        /// Student Details - Ignores RTE by default.
        /// </summary>
        /// <param name="clientId">InstId</param>
        /// <param name="includeRTE">Defaults to false</param>
        /// <returns></returns>
        internal static List<Student_Inst_Index.Result> GetStudentDetails(string clientId, bool includeRTE = false)
        {
            var redis = Connection.GetDatabase();
            var dto_string = redis.StringGet(GetKey_Student(clientId));
            try
            {
                var stus = JsonConvert.DeserializeObject<List<Student_Inst_Index.Result>>(dto_string);
                if (includeRTE) return stus;
                else //exclude RTE
                {
                    var result = stus
                                    .Where(o => o.Tags == null || (o.Tags != null && !o.Tags.Contains("RTE"))).ToList();
                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        internal static string GetKey_Inventory(string id)
        {
            var _prefix = "inventory";
            return $"{_prefix}:{id}";
        }

        internal static List<InventoryItemDto> GetInventoryDetails(string clientId)
        {
            var redis = Connection.GetDatabase();
            var dto_string = redis.StringGet(RedisCache.GetKey_Inventory(clientId));
            var result = JsonConvert.DeserializeObject<List<InventoryItemDto>>(dto_string);
            return result;
        }


        /// <summary>
        /// Retrieve profiles using key profile:key. Multiple profiles can exist for the same email. Like 2 wards of a parent studying in the same inst
        /// </summary>
        /// <param name="key">One of Id | Email of the user</param>
        /// <returns></returns>
        internal static List<StudentProfile> GetStudentProfiles(string key)
        {
            var redisKey = $"profile:{key.Trim()}";
            var redis = Connection.GetDatabase();
            string dto_string = redis.StringGet(redisKey);
            var result = string.IsNullOrEmpty(dto_string) ? null : JsonConvert.DeserializeObject<List<StudentProfile>>(dto_string);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">One of Id | Email of the user</param>
        /// <param name="profiles">There multiple profiles available</param>
        internal static void SetStudentProfiles(string key, object profiles)
        {
            var redisKey = $"profile:{key.ToLower().Trim()}";
            var json = JsonHelper.Serialize(profiles);
            Connection.GetDatabase().StringSet(redisKey, json);
        }

        internal static void DeleteKeyProfile(string key)
        {
            var redisKey = $"profile:{key.ToLower().Trim()}";
            Connection.GetDatabase().KeyDelete(redisKey);
        }
    }
}
