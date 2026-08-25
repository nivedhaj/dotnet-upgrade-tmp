using ct._Core.Repo.Resource;
using ct._Core.Persistence;
using ct._Core.Util;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Repo.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ct._Core.Security
{
    /// <summary>
    /// User Context Interface
    /// </summary>
    public interface IUserContext
    {
        /// <summary>
        /// Sessiond Id - Not related to DB Session
        /// </summary>
        string SessionId { get; set; }

        /// <summary>
        /// App Id
        /// </summary>
        /// <returns></returns>
        string AppId { get; set; }
        /// <summary>
        /// Short Name
        /// </summary>
        /// <returns></returns>
        string ShortName { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        /// <value></value>
        string Token { get; set; }
        /// <summary>
        /// Sms Sender Id
        /// </summary>
        /// <returns></returns>
        string SmsSenderId { get; set; }
        /// <summary>
        /// Is Mobile App
        /// </summary>
        /// <returns></returns>
        bool IsMobileApp { get; set; }
        //client
        /// <summary>
        /// Org Id
        /// </summary>
        /// <returns></returns>
        string OrgId { get; set; }
        /// <summary>
        /// Inst Id
        /// </summary>
        /// <returns></returns>
        string InstId { get; set; }
        /// <summary>
        /// Inst Alias
        /// </summary>
        /// <returns></returns>
        string InstAlias { get; set; }
        /// <summary>
        /// Bound By Org
        /// </summary>
        /// <returns></returns>
        bool BoundByOrg { get; set; }
        /// <summary>
        /// Bound by Institute
        /// </summary>
        /// <returns></returns>
        bool BoundByInst { get; set; }
        /// <summary>
        /// Bound by User
        /// </summary>
        /// <returns></returns>
        bool BoundByUser { get; set; }
        /// <summary>
        /// Is Root Admin
        /// </summary>
        /// <returns></returns>
        bool IsRootAdmin { get; set; }
        //persona
        /// <summary>
        /// Current Persona Id
        /// </summary>
        /// <returns></returns>
        string CurrentPersonaId { get; set; }
        /// <summary>
        /// User Scopes defined as a set of wild card endpoints and allow (get-1, post-2, both-3) and deny (0) values
        /// </summary>
        List<string> Scope { get; set; }
        //user
        /// <summary>
        /// User Id
        /// </summary>
        /// <returns></returns>
        string UserId { get; set; }
        /// <summary>
        /// User Email
        /// </summary>
        /// <returns></returns>
        string UserEmail { get; set; }
        /// <summary>
        /// Policy to be used at run time
        /// </summary>
        // IPolicy Policy { get; set; }
        string TimeZoneById { get; set; }
        //query params
        /// <summary>
        /// Entity Id
        /// </summary>
        /// <returns></returns>
        string Id { get; set; }
        /// <summary>
        /// No of records to return
        /// </summary>
        /// <returns></returns>
        int Top { get; set; }
        /// <summary>
        /// No of records to skip
        /// </summary>
        /// <returns></returns>
        int Skip { get; set; }
        /// <summary>
        /// Sort order
        /// </summary>
        /// <returns></returns>
        string OrderBy { get; set; }
        /// <summary>
        /// Search string
        /// </summary>
        /// <returns></returns>
        string Search { get; set; }
        //db context
        /// <summary>
        /// DB session
        /// </summary>
        /// <returns></returns>
        IPostgresSession Session { get; set; }
        /// <summary>
        /// Should Entity / Entities be persisted? Defaults to true
        /// </summary>
        /// <returns></returns>
        bool Persist { get; set; }
        string Topic { get; set; }
        /// <summary>
        /// Current User Context
        /// </summary>
        /// <returns></returns>
        UserContext Current();
    }
    /// <summary>
    /// User Context Provider
    /// </summary>
    public interface IUserContextProvider
    {
        /// <summary>
        /// Get the current User Context
        /// </summary>
        /// <returns></returns>
        dynamic Current();
    }
    /// <summary>
    /// User Context - Injected into DAL 
    /// </summary>
    public class UserContext : IUserContext
    {
        IUserContextProvider _ucp;
        /// <summary>
        /// Sessiond Id - Not related to DB Session
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// App Id
        /// </summary>
        /// <returns></returns>
        public string AppId { get; set; }
        /// <summary>
        /// Short name of the Client (Inst / Org)
        /// </summary>
        /// <returns></returns>
        public string ShortName { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        /// <value></value>
        public string Token { get; set; }
        /// <summary>
        /// Sms Sender Id
        /// </summary>
        /// <returns></returns>
        public string SmsSenderId { get; set; }
        /// <summary>
        /// Is Mobile App
        /// </summary>
        /// <returns></returns>
        public bool IsMobileApp { get; set; }
        //client
        /// <summary>
        /// Org Id
        /// </summary>
        /// <returns></returns>
        public string OrgId { get; set; }
        /// <summary>
        /// Inst Id
        /// </summary>
        /// <returns></returns>
        public string InstId { get; set; }

        /// <summary>
        /// Inst Alias
        /// </summary>
        /// <returns></returns>
        public string InstAlias { get; set; }
        /// <summary>
        /// Is bound by Org
        /// </summary>
        /// <returns></returns>
        public bool BoundByOrg { get; set; }
        /// <summary>
        /// Is bound by Inst
        /// </summary>
        /// <returns></returns>
        public bool BoundByInst { get; set; }
        /// <summary>
        /// Is bound by User (Parent etc)
        /// </summary>
        /// <returns></returns>
        public bool BoundByUser { get; set; }
        /// <summary>
        /// Is Root Admin?
        /// </summary>
        /// <returns></returns>
        public bool IsRootAdmin { get; set; }
        //persona
        /// <summary>
        /// Current Persona Id
        /// </summary>
        /// <returns></returns>
        public string CurrentPersonaId { get; set; }
        /// <summary>
        /// Scope of the request
        /// </summary>
        /// <returns></returns>
        public List<string> Scope { get; set; }
        //user
        /// <summary>
        /// User Id
        /// </summary>
        /// <returns></returns>
        public string UserId { get; set; }
        /// <summary>
        /// User Email
        /// </summary>
        /// <returns></returns>
        public string UserEmail { get; set; }
        /// <summary>
        /// User Timezone
        /// </summary>
        /// <returns></returns>
        public string TimeZoneById { get; set; }
        //query params
        /// <summary>
        /// Entity Id
        /// </summary>
        /// <returns></returns>
        public string Id { get; set; }
        /// <summary>
        /// No of records to get. Defaults to 10
        /// </summary>
        /// <returns></returns>
        public int Top { get; set; } = 10;
        /// <summary>
        /// Records to Skip. Defaults to 0
        /// </summary>
        /// <returns></returns>
        public int Skip { get; set; } = 0;
        /// <summary>
        /// Sort order
        /// </summary>
        /// <returns></returns>
        public string OrderBy { get; set; } //k1:v1;k2:v2 => v1,v2 are asc/desc. Default asc. k1,k2 are terms
        /// <summary>
        /// k1:v1, k2:v2 => v1, v2 if present, represent filters or facets like isActive, X A etc.
        /// k1, k2 represent search terms if v1, v2 are present 
        /// k1, k2 represent search values - with terms pre-defined in the query called - when v1,v2 are absent.
        /// </summary>
        public string Search { get; set; }
        //db context
        /// <summary>
        /// DB Session
        /// </summary>
        /// <returns></returns>
        public IPostgresSession Session { get; set; }
        /// <summary>
        /// Defaults to true.If set to false, the entity should not be persisted in Db.
        /// The DB Laayer should check this value before persisting
        /// </summary>
        /// <returns></returns>
        public bool Persist { get; set; }
        /// <summary>
        /// Topic
        /// </summary>
        /// <value></value>
        public string Topic { get; set; }
        UserContext()
        {
            Persist = true;
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="ucp"></param>
        /// <returns></returns>
        public UserContext(IUserContextProvider ucp) : this()
        {
            BoundByInst = true;
            IsRootAdmin = false;
            Scope = new List<string>();
            _ucp = ucp;
        }

        /// <summary>
        /// Create new UserContext if UC provider is not null.Else return this
        /// </summary>
        /// <returns></returns>
        public UserContext Current()
        {
            if (_ucp == null) return this; //_ucp can be null if called from Tests & UC properties would be set from within the test
            Dictionary<string, Object> payload = _ucp.Current();
            var jo = JObject.Parse((string)payload["token"]);

            var _appId = jo.GetValue("AppId").ToString();
            var _clientId = jo.GetValue("ClientId").ToString();
            var _userEmail = jo.GetValue("UserEmail").ToString();
            var _personaId = jo.GetValue("PersonaId").ToString();
            var _userId = jo.GetValue("UserId").ToString();
            var _sessionId = jo.GetValue("SessionId").ToString();
            var _scope = jo.GetValue("Scope").ToObject<List<string>>();
            var _session = (IPostgresSession)payload?["Session"];
            var _shortName = jo.GetValue("ShortName").ToString();
            var _smsSenderId = jo.GetValue("SMSSenderId")?.ToString();
            var _topic = (string)payload?["Topic"];

            var _queryCollection = (IQueryCollection)payload?["QueryCollection"];
            _queryCollection?.TryGetValue("id", out StringValues _ids);
            _queryCollection?.TryGetValue("$orderby", out StringValues _orrderbys);
            _queryCollection?.TryGetValue("$top", out StringValues _tops);
            _queryCollection?.TryGetValue("$skip", out StringValues _skips);
            _queryCollection?.TryGetValue("$search", out StringValues _searches);
            _queryCollection?.TryGetValue("persist", out StringValues _persists);

            var _client = _session.Load<object>(_clientId) as JObject;
            var _persona = _session.Load<object>(_personaId) as JObject;

            string persona_type = _persona?["PersonaTypeAsString"]?.ToString()?.ToLower();
            bool client_is_group = _client?["IsGroup"]?.Value<bool>() ?? false;
            string owner_id = _client?["OwnerId"]?.ToString();
            string client_id = _client?["Id"]?.ToString() ?? _clientId; //same as _clientId

            var _boundByUser = persona_type == "parent" || persona_type == "student";
            var _boundByInst = !client_is_group && !_boundByUser;
            var _boundByOrg = client_is_group && !_boundByUser;
            var _orgId = client_is_group ? client_id : owner_id;
            var _instId = !client_is_group ? client_id : "";

            // Clear the session so that the db layer gets a session that doesn't contain any entities
            // such as the dynamic entities for client / persona that are loaded above.
            _session.Advanced.Clear();

            int.TryParse(_skips.FirstOrDefault(), out int _skip);
            int.TryParse(_tops.FirstOrDefault(), out int _top);

            //todo set all properties
            return new UserContext()
            {
                AppId = _appId,
                BoundByInst = _boundByInst,
                BoundByOrg = _boundByOrg,
                BoundByUser = _boundByUser,
                CurrentPersonaId = _personaId,
                InstId = _instId,
                OrgId = _orgId,
                UserId = _userId,
                UserEmail = _userEmail,
                SessionId = _sessionId,
                Session = _session,
                Scope = _scope,
                Id = _ids.FirstOrDefault(),
                OrderBy = _orrderbys.FirstOrDefault(),
                Skip = _skip,
                Top = _top == 0 ? 10 : _top,
                Search = _searches.FirstOrDefault(),
                Persist = string.IsNullOrEmpty(_persists.FirstOrDefault()) ? true : false,
                ShortName = _shortName,
                SmsSenderId = _smsSenderId,
                Token = (string)payload["EncryptedToken"],
                Topic = string.IsNullOrEmpty(_topic) ? "topic.unknown" : _topic 
            };
        }
        /// <summary>
        /// Is End Point In Scope
        /// </summary>
        /// <param name="end_point"></param>
        /// <returns></returns>
        public bool IsInScope(string end_point)
        {
            //expand scopes to get & post
            List<string> scope_allowed, scope_denied;
            expand_scope(Scope, out scope_allowed, out scope_denied);
            var result = isInScope(scope_allowed, scope_denied, end_point);
            return result;
        }
        static void expand_scope(List<string> scope, out List<string> scope_allowed, out List<string> scope_denied)
       {
           scope_allowed = new List<string>();
           scope_denied = new List<string>();
           var deny = 0;
           var get = 1;
           var post = 2;

           foreach (var sc in scope ?? new List<string>())
           {
               if (string.IsNullOrEmpty(sc)) continue;
               var arr = sc.Split(":".ToCharArray());
               var dgp = Convert.ToInt16(arr.Last()); //deny | get | post

               var _scope = arr.First().ToLower();
               _scope = Regex.Replace(_scope, @"\s+", ""); //remove all whitespace
               var get_scope = "^get/" + Regex.Escape(_scope).Replace("\\*", ".*") + "$"; //Convert to regular expression
               var post_scope = "^post/" + Regex.Escape(_scope).Replace("\\*", ".*") + "$"; //Convert to regular expression

               if (dgp == deny)
               {
                   scope_denied.Add(get_scope);
                   scope_denied.Add(post_scope);
                   scope_allowed.Remove(get_scope);
                   scope_allowed.Remove(post_scope);
               }
               if ((dgp & get) > 0)
               {
                   scope_allowed.Add(get_scope);
                   scope_denied.Remove(get_scope);
               }
               if ((dgp & post) > 0)
               {
                   scope_allowed.Add(post_scope);
                   scope_denied.Remove(post_scope);
               }
           }
       }
        static bool isInScope(List<string> scope_allowed, List<string> scope_denied, string end_point)
        {
            var result = false;
            if (end_point.Contains("*")) return result;
            end_point = Regex.Replace(end_point, @"\s+", ""); //remove all whitespace

            //if perm in deny_list result = false
            foreach (var d in scope_denied)
            {
                if (Regex.IsMatch(end_point, d)) return false;
            }
            //else if perm in allow_list result = true
            foreach (var a in scope_allowed)
            {
                if (Regex.IsMatch(end_point, a)) { result = true; break; }
                else if (Regex.IsMatch(end_point + "/", a)) { result = true; break; }
            }
            return result;
        }
    }

    /// <summary>
    ///     User Context (Typed) Factory
    /// </summary>
    /// Ref: http://autofaccn.readthedocs.io/en/latest/faq/injecting-configured-parameters.html
    public class UserContextProvider : IUserContextProvider
    {
        private readonly IPostgresSession _session;
        private readonly IConfiguration _configuration;
        IHttpContextAccessor _httpContextAccessor;
        AppService _appService;
        /// <summary>
        /// App
        /// </summary>
        public App App { get; private set; }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="store"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="appService"></param>
        public UserContextProvider(PostgresDocumentStore store, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, AppService appService)
        {
            _session = store.OpenSession();
            _httpContextAccessor = httpContextAccessor;
            var _request = _httpContextAccessor.HttpContext.Request;
            _configuration = configuration;
            _appService = appService;

            IHeaderDictionary headersDictionary = _httpContextAccessor.HttpContext.Request.Headers;
            string urlReferrer = headersDictionary[_Constants.REFERER].ToString().ToLower().TrimEnd('/');
            var end_point = _httpContextAccessor.HttpContext.Request.Path.Value.ToLower().TrimEnd('/');
            if (urlReferrer.EndsWith("swagger") || end_point.Contains("swagger") || end_point.EndsWith("confirmregistration"))
            {
                headersDictionary.Add(_Constants.ACCESS_CODE, _Constants.ANON_ACCESS_CODE);
            }
            var _accessCode = Helper.GetHeader<string>(_request, _Constants.ACCESS_CODE);
            App = _appService.GetAppFromAccessCode(_accessCode);
            if (App == null || App.Status != AppStatusEnum.Active) { throw new InvalidAccessCodeException(nameof(InvalidAccessCodeException)); }
        }
        /// <summary>
        /// Current User Context
        /// </summary>
        /// <returns></returns>
        public dynamic Current()
        {
            var _request = _httpContextAccessor.HttpContext.Request;
            var _encryptedToken = Helper.GetHeader<string>(_request, _Constants.ACCESS_TOKEN);
            var _topic = Helper.GetHeader<string>(_request, _Constants.TOPIC);
            if (string.IsNullOrEmpty(_encryptedToken)) _encryptedToken = Helper.GetHeader<string>(_request, _Constants.AUTHORIZATION);
            if (string.IsNullOrEmpty(_encryptedToken)) throw new InvalidAccessTokenException(nameof(InvalidAccessTokenException));

            var _key = _configuration["Ct:Key"];
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
            var dict = decoder.DecodeToObject<IDictionary<string, object>>(_encryptedToken, _key, verify: true);

            //use json path navigation 
            dynamic jo = JObject.Parse((string)dict["token"]);
            var _userId = jo.GetValue("UserId").ToString();
            _session.Advanced.ExternalState.Add(new KeyValuePair<string, object>("UserId", _userId));

            dict["Session"] = _session;
            dict["QueryCollection"] = _httpContextAccessor.HttpContext.Request.Query;
            dict["EncryptedToken"] = _encryptedToken;
            dict["Topic"] = _topic;

            return dict;
        }
    }
}
