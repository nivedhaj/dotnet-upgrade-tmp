using ct._Core.Policy;
using ct._Core.Repo.Dto;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Marten;
using Marten.Linq;
using Marten;
using Marten.Linq;
using Repo.Db.Index;
using Repo.Dto;
using Repo.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Marten.Compat;

namespace Repo.Db.Query
{
    /// <summary>
    /// Student Query
    /// </summary>
    public class StudentQry
    {
        private readonly IDocumentSession _session;
        private readonly IUserContext _uc;
        private readonly RabbitMQService _rmqService;
        private readonly IPolicyEngine _policyEngine;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="rmqService"></param>
        /// <param name="policyEngineProvider"></param>
        public StudentQry(IUserContext uc, RabbitMQService rmqService, IPolicyEngineProvider policyEngineProvider = null)
        {
            _uc = uc.Current();
            _session = _uc.Session;
            _rmqService = rmqService;
            _policyEngine = policyEngineProvider.For(_uc.ShortName);
        }

        /// <summary>
        /// Set Student Cache
        /// </summary>
        internal void SetStudentCache(bool reset = false)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var reids_key = RedisCache.GetKey_Student(inst_id);
            var redis = RedisCache.Connection.GetDatabase();
            if (!redis.KeyExists(reids_key) || reset)
            {
                var stu_list = GetAll();
                redis.StringSet(reids_key, JsonHelper.Serialize(stu_list));
            }
        }
        internal List<Student_Inst_Index.Result> GetAll(string courseId = "", string section = "")
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var stu_list = new List<Student_Inst_Index.Result>();

            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);
            Expression<Func<Student_Inst_Index.Result, bool>> filterByActiveStudents = o => o.EnrollmentStatus == EnrollmentStatusEnum.Active;
            Expression<Func<Student_Inst_Index.Result, bool>> filterByCourse = o => o.CourseId.Equals(courseId);
            Expression<Func<Student_Inst_Index.Result, bool>> filterBySection = o => o.SectionName.Equals(section);

            var applyFilterByCourse = !string.IsNullOrEmpty(courseId) && !courseId.Equals("all", StringComparison.CurrentCultureIgnoreCase);
            var applyFilterBySection = !string.IsNullOrEmpty(section) && !section.Equals("all", StringComparison.CurrentCultureIgnoreCase);

            var qry = _session
                        .Query<Student_Inst_Index.Result>()
                        .Where(filterByInst)
                        .Where(filterByActiveStudents);
            qry = applyFilterByCourse ? qry.Where(filterByCourse) : qry;
            qry = applyFilterBySection ? qry.Where(filterBySection) : qry;

            qry = qry
                    .OrderBy(o => o.CourseSortIndex)
                    .ProjectInto<Student_Inst_Index.Result>();

            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    if (stu == null) continue;
                    stu.Name = Helper.DeduplicatePattern(stu.Name);
                    stu_list.Add(stu);
                }
            }
            return stu_list;
        }

        internal dynamic GetAllFromCache(bool includeRTE = false, bool reset = false)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var redis_key = RedisCache.GetKey_Student(inst_id);
            var redis = RedisCache.Connection.GetDatabase();
            if (!redis.KeyExists(redis_key) || reset)
            {
                var stu_list = GetAll();
                redis.StringSet(redis_key, JsonHelper.Serialize(stu_list));
            }

            if (_uc.ShortName == "VVSPHS") includeRTE = true;

            var stus = RedisCache.GetStudentDetails(inst_id, includeRTE).Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active);
            var result = stus.Select(o => new { o.Id, o.Name, o.StudentId, o.CourseName, o.SectionName, o.CourseId }).ToList();
            return result;
        }


        /// <summary>
        /// Get Students
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activeStudentsOnly"></param>
        /// <param name="todaysAbsenteesOnly"></param>
        /// <param name="studentIds"></param>
        /// <param name="tcStudentsOnly"></param>
        /// <param name="per">Period No</param>
        /// <returns></returns>
        internal PagedResultDto<GetStudentDto> Get<T>(bool activeStudentsOnly = false, bool todaysAbsenteesOnly = false, List<string> studentIds = null, bool tcStudentsOnly = false, int per = 0) where T : Student_Inst_Index.Result
        {
            var result = new PagedResultDto<GetStudentDto>();
            var absentee_dict = GetAbsenteesOn();
            if (todaysAbsenteesOnly)
            {
                studentIds = new List<string>();
                var absentee_students = absentee_dict.SelectMany(o => o.Value);
                studentIds.AddRange(absentee_students);
                if (studentIds == null || studentIds.Count == 0) return result;
                studentIds = studentIds.Distinct().ToList();
            }
            Expression<Func<T, bool>> filterByInstId = o => o.InstId.Equals(_uc.InstId);
            Expression<Func<T, bool>> studentsAreActive = o => o.EnrollmentStatus == EnrollmentStatusEnum.Active;
            Expression<Func<T, bool>> studentsIssuedTC = o => o.EnrollmentStatus == EnrollmentStatusEnum.TC;
            Expression<Func<T, bool>> filterByStudentIds = o => o.Id.In(studentIds);
            Expression<Func<T, bool>> filterByParentEmail = o => o.EmailList.ContainsAny(new[] { _uc.UserEmail });

            string[] arr_order_by = _uc.OrderBy?.Split(":".ToCharArray());
            var orderBy = arr_order_by == null || arr_order_by.Length == 0 ? "RollNo" : arr_order_by[0].ToLower();
            var sortOrder = arr_order_by == null || arr_order_by.Length < 2 ? "asc" : arr_order_by[1].ToLower();
            var skip = _uc.Skip;
            var pageSize = _uc.Top;
            if (todaysAbsenteesOnly) pageSize = 1024;
            var students = new List<GetStudentDto>();

            List<Student> stus;
            int totalResults = 0;
            int currentPage = skip == 0 ? 1 : (int)Math.Ceiling((decimal)(skip / (double)pageSize));

            if (!string.IsNullOrEmpty(_uc.Search) && _uc.Search.Contains("course:"))
            {
                // Parse the course/section from the search string
                // Example: course:I-A or course:I-A,II-B
                var search = _uc.Search.Replace("course:", "").Trim();
                var courseSectionPairs = search.Split(',');
                var courses = _session.Query<Course>().Where(o => o.InstId == _uc.InstId && o.Status == CourseStatusEnum.Active).ToList();
                var pairs = new List<(string courseId, string section)>();
                foreach (var pair in courseSectionPairs)
                {
                    var parts = pair.Split('-');
                    if (parts.Length == 2)
                    {
                        var courseName = parts[0].Trim();
                        var section = parts[1].Trim();
                        var course = courses.FirstOrDefault(o => o.Name == courseName);
                        if (course != null)
                        {
                            pairs.Add((course.Id, section));
                        }
                    }
                }
                var allStudents = new List<Student>();
                int total = 0;
                foreach (var (courseId, section) in pairs)
                {
                    var queryable = _session.Query<T>()
                        .Where(filterByInstId)
                        .Where(x => x.CourseId == courseId && x.SectionName == section);
                    queryable = activeStudentsOnly ? queryable.Where(studentsAreActive) : queryable;
                    queryable = _uc.BoundByUser ? queryable.Where(filterByParentEmail) : queryable;
                    queryable = tcStudentsOnly ? queryable.Where(studentsIssuedTC) : queryable;
                    queryable = todaysAbsenteesOnly ? queryable.Where(filterByStudentIds) : studentIds != null && studentIds.Count > 0 ? queryable.Where(filterByStudentIds) : queryable;
                    if (orderBy.Equals("RollNo", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (sortOrder == "desc")
                            queryable = queryable.OrderBy(o => o.CourseSortIndex).ThenBy(o => o.Branch).ThenBy(o => o.SectionName).ThenByDescending(o => o.RollNo).ThenBy(o => o.Name);
                        else
                            queryable = queryable.OrderBy(o => o.CourseSortIndex).ThenBy(o => o.Branch).ThenBy(o => o.SectionName).ThenBy(o => o.RollNo).ThenBy(o => o.Name);
                    }
                    else
                    {
                        if (sortOrder == "desc")
                            queryable = queryable.OrderBy(o => o.CourseSortIndex).ThenBy(o => o.Branch).ThenBy(o => o.SectionName).ThenByDescending(o => o.Name).ThenBy(o => o.RollNo);
                        else
                            queryable = queryable.OrderBy(o => o.CourseSortIndex).ThenBy(o => o.Branch).ThenBy(o => o.SectionName).ThenBy(o => o.Name).ThenBy(o => o.RollNo);
                    }
                    QueryStatistics stats;
                    var studentsForPair = queryable
                        .Statistics(out stats)
                        .Skip(skip)
                        .Take(pageSize)
                        .OfType<Student>()
                        .ToList();
                    allStudents.AddRange(studentsForPair);
                    total += stats.TotalResults;
                }
                // Remove duplicates if any
                stus = allStudents.Distinct().ToList();
                totalResults = total;
            }
            else
            {
                // Standard RavenQueryable path
                var queryable = _session.Query<T>();
                if (!string.IsNullOrEmpty(_uc.Search))
                {
                    var searchTerm = _uc.Search.Contains(" ") ? $"\"{_uc.Search}\"" : _uc.Search;
                    queryable = queryable
                            .Search(o => o.StudentId, searchTerm)
                            .Search(o => o.Name, searchTerm)
                            .Search(o => o.FatherName, searchTerm)
                            .Search(o => o.MotherName, searchTerm);
                }
                queryable = queryable.Where(filterByInstId);
                queryable = activeStudentsOnly ? queryable.Where(studentsAreActive) : queryable;
                queryable = _uc.BoundByUser ? queryable.Where(filterByParentEmail) : queryable;
                queryable = tcStudentsOnly ? queryable.Where(studentsIssuedTC) : queryable;
                queryable = todaysAbsenteesOnly ? queryable.Where(filterByStudentIds) : studentIds != null && studentIds.Count > 0 ? queryable.Where(filterByStudentIds) : queryable;
                if (orderBy.Equals("RollNo", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (sortOrder == "desc")
                        queryable = queryable.OrderBy(o => o.CourseSortIndex).ThenBy(o => o.Branch).ThenBy(o => o.SectionName).ThenByDescending(o => o.RollNo).ThenBy(o => o.Name);
                    else
                        queryable = queryable.OrderBy(o => o.CourseSortIndex).ThenBy(o => o.Branch).ThenBy(o => o.SectionName).ThenBy(o => o.RollNo).ThenBy(o => o.Name);
                }
                else
                {
                    if (sortOrder == "desc")
                        queryable = queryable.OrderBy(o => o.CourseSortIndex).ThenBy(o => o.Branch).ThenBy(o => o.SectionName).ThenByDescending(o => o.Name).ThenBy(o => o.RollNo);
                    else
                        queryable = queryable.OrderBy(o => o.CourseSortIndex).ThenBy(o => o.Branch).ThenBy(o => o.SectionName).ThenBy(o => o.Name).ThenBy(o => o.RollNo);
                }
                QueryStatistics stats;
                stus = queryable
                    .Statistics(out stats)
                    .Skip(skip)
                    .Take(pageSize)
                    .OfType<Student>()
                    .ToList();
                totalResults = stats.TotalResults;
            }

            foreach (var stu in stus)
            {
                if (todaysAbsenteesOnly && per != 0)
                {
                    if (absentee_dict != null && absentee_dict.Count > 0)
                    {
                        var absentees_for_per = absentee_dict[per];
                        if (absentees_for_per != null && !absentees_for_per.Contains(stu.Id)) continue;
                    }
                }
                var dto = get_stu_dto(stu, absentee_dict);
                students.Add(dto);
            }

            result = new PagedResultDto<GetStudentDto>()
            {
                Data = students,
                RecordsPerPage = pageSize,
                TotalRecords = totalResults,
                CurrentPage = currentPage
            };
            return result;
        }

        internal GetStudentDto GetById(string id)
        {
            var stu = _session.Load<Student>(id);
            if (stu == null) return null;
            return get_stu_dto(stu);
        }

        internal List<string> GetParentEmails(bool validOnly = true, bool activeOnly = true, List<string> limitToCourses = null)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<string>();

            //qry
            var qry = _session.Query<Student_Inst_Index.Result>();

            //filter
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId == inst_id;
            Expression<Func<Student_Inst_Index.Result, bool>> filterByStudentStatus = o => o.EnrollmentStatus == EnrollmentStatusEnum.Active;
            Expression<Func<Student_Inst_Index.Result, bool>> filterByCourses = o => o.CourseId.In(limitToCourses);
            qry = qry.Where(filterByInst);
            qry = activeOnly ? qry.Where(filterByStudentStatus) : qry;
            qry = limitToCourses != null && limitToCourses.Count > 0 ? qry.Where(filterByCourses) : qry;

            using (var enumerator = _session.Stream(qry.OfType<Student>()))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    if (stu == null) continue;
                    var stu_dto = get_stu_dto(stu);
                    result.AddRange(stu_dto.ParentEmails.Where(o => !string.IsNullOrEmpty(o)).Distinct());
                }
            }
            return result;
        }

        internal string GetSMSSenderId()
        {
            var inst_id = _uc.InstId;
            var inst_dto = RedisCache.GetClientDto(inst_id);
            return inst_dto.SMSSenderId;
        }

        private GetStudentDto get_stu_dto(Student stu, Dictionary<int, List<string>> absentees = null)
        {
            var enrl = stu.Enrollments.First(o => o.InstId == _uc.InstId);
            var stu_att_arr = new List<string>();
            var attendance_session_count = _policyEngine.AttendanceSessions;
            stu_att_arr.AddRange(Enumerable.Repeat("Present", attendance_session_count));
            if (absentees != null && absentees.Count > 0)
            {
                foreach (var de in absentees)
                {
                    if (de.Value == null || de.Value.Count == 0) continue;
                    if (de.Value.Contains(stu.Id)) stu_att_arr[de.Key - 1] = "Absent";
                }
            }

            var dto = new GetStudentDto()
            {
                Id = stu.Id,
                Name = Helper.DeduplicatePattern(stu.Name),
                Gender = stu.Gender.ToString(),
                StudentId = enrl.StudentId,
                InstId = enrl.InstId,
                CourseId = enrl.CourseId,
                CourseName = enrl.CourseName,
                TermName = enrl.TermName,
                SectionName = enrl.SectionName,
                Branch = enrl.Branch,
                Status = enrl.Status,
                RollNo = enrl.RollNoAsInt,
                Tags = stu.Tags,
                AttendanceList = stu_att_arr,//Takes care of Attendance property as well.
                Email = stu.Email,
                Mobile = stu.Mobile,
                Father = stu.Father,
                Mother = stu.Mother,
                Guardian = stu.Guardian,
                ParentEmails = stu.GetEmails(),
                ParentMobiles = stu.GetMobiles(),
                AdmissionNo = enrl.AdmissionNo,
                ApplicationNo = enrl.ApplicationNo,
                AadharNumber = stu.AadharNumber,
                DOB = stu.DOB,
                DOA = enrl.DOA,
                VirtualId = stu.VirtualId,
                Domicile = stu.Domicile,
                IEP = stu.IEP,
                Address = stu.Addresses.Count > 0 ? stu.Addresses[0] : null
            };
            return dto;
        }

        /// <summary>
        /// Get today's absentees. Dictionary with Period No as Key and Value as ["student/1", "student/2", ...]
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="date"></param>
        /// <param name="periodNumber"></param>
        /// <returns>Dictionary with Period No as Key and Value as ["student/1", "student/2", ...]</returns>
        public Dictionary<int, List<string>> GetAbsenteesOn(string instId = "", DateTime? date = null, int periodNumber = 0)
        {
            var dict = new Dictionary<int, List<string>>();
            var attendance_session_count = _policyEngine.AttendanceSessions;
            foreach (var key in Enumerable.Range(1, attendance_session_count))
            {
                dict.Add(key, new List<string>());
            }
            if (instId == "") instId = _uc.InstId;
            var _dt = date.HasValue ? date.Value.Date : DateTime.Now.Date;
            var qry = _session.Query<AttendanceEvent_IX.Result>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where(o => o.Date == _dt)
                            .OfType<AttendanceEvent>();

            var att_events = new List<AttendanceEvent>();
            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var att = enumerator.Current.Document;
                    if (att == null) continue;
                    if (periodNumber != 0 && att.PeriodNo != periodNumber) continue;
                    att_events.Add(att);
                }
            }

            var group = att_events
                             .GroupBy(o => new { o.PeriodNo, o.StudentId })
                             .Select(g => new { PeriodNo = g.Key.PeriodNo, StudentId = g.Key.StudentId, Att = g.OrderByDescending(s => s.CreatedOn).First()?.Attendance });

            foreach (var item in group)
            {
                var absentees = dict[item.PeriodNo];
                if (item.Att == "Absent") absentees.Add(item.StudentId); else absentees.Remove(item.StudentId);
            }
            return dict;
        }

        private string getLucenePredicate(string searchText, bool activeOnly = false, string userEmail = "")
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return string.Empty;
            }
            var result = string.Empty;
            var lucene_predicates = new List<string>()
            {
                $"(InstId:{_uc.InstId})"
            };
            if (activeOnly) lucene_predicates.Add($"(EnrollmentStatus:{EnrollmentStatusEnum.Active})");
            //refer: https://stackoverflow.com/questions/206717/how-do-i-replace-multiple-spaces-with-a-single-space-in-c
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            searchText = regex.Replace(searchText, " ");
            var search_terms = searchText.Split(null);

            var courses = _session.Query<Course>().Where(o => o.InstId == _uc.InstId).ToList();
            courses = courses.Where(o => o.Status == CourseStatusEnum.Active).ToList();
            foreach (var s_t in search_terms)
            {
                if (s_t.Contains(":"))
                {
                    var tokens = s_t.Split(":".ToArray());
                    if (tokens == null || tokens.Count() < 2) continue;
                    if (tokens[0] == "course")
                    {
                        var cs = tokens[1].Split(",".ToCharArray()).ToList();
                        var lp_or = new List<string>(); ;
                        foreach (var c in cs ?? new List<string>())
                        {
                            if (string.IsNullOrEmpty(c)) continue;
                            var cbs = c.Split("-".ToCharArray());
                            var cn = cbs[0].Trim().Replace("_", " ");
                            var branch = cbs.Count() == 2 ? string.Empty : cbs[1].Trim().Replace("_", " ");
                            var section = cbs.Last();
                            Course course = null;
                            if (string.IsNullOrEmpty(branch))
                                course = courses.FirstOrDefault(o => o.Name.Equals(cn));
                            else
                                course = courses.FirstOrDefault(o => o.Name.Equals(cn) && o.Branch.Equals(branch));

                            var lp_part = $"(CourseId:{course.Id} AND SectionName:{section})";
                            if (!string.IsNullOrEmpty(lp_part)) lp_or.Add(lp_part);
                        }
                        var lp = string.Join(" OR ", lp_or.Where(s => !string.IsNullOrEmpty(s)));
                        if (!string.IsNullOrEmpty(lp)) lucene_predicates.Add($"({lp})");
                    }
                    else if (tokens[0].Equals("studentId", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(tokens[1]) || tokens[1] == "null")
                        {
                            var lp = "(-StudentId:[* TO *])";
                            lucene_predicates.Add(lp);
                        }
                    }
                    else if (tokens[0].Trim().Equals("tcStudentsOnly", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(tokens[1]) && tokens[1] == "true")
                        {
                            var lp = $"(EnrollmentStatus:{EnrollmentStatusEnum.TC})";
                            lucene_predicates.Add(lp);
                        }
                    }
                }
                else //token doesn't contain : hence a name search
                {
                    var lp = $"(Name:{s_t}* OR StudentId:{s_t}* OR FatherName:{s_t}* OR MotherName:{s_t}* OR VirtualId:{s_t}*)"; //Name or student id starts with
                    lucene_predicates.Add(lp);
                }
            }
            result = string.Join(" AND ", lucene_predicates.Where(s => !string.IsNullOrEmpty(s)));
            return result;
        }

        internal List<DomainDto> GetGoalsPerformanceByDomain(string id, string term)
        {
            var student = _session.Load<Student>(id);
            if (student == null) throw new ObjectNotFoundException(id);

            var _term = student.IEP.Terms.Where(o => o.Title == term).FirstOrDefault();
            if (_term == null) throw new ArgumentNullException("Term not found");

            var result = new List<DomainDto>();
            foreach (var domain in _term.Domains)
            {
                var domain_dto = new DomainDto();
                domain_dto.domain = domain.Title;

                var goalsPerformanceDto = new List<GoalsPerformanceDto>();
                foreach (var gp in domain.GoalsPerformance)
                {
                    var dto = new GoalsPerformanceDto();
                    dto.goal = gp.Goal;
                    dto.performance = gp.Performance;
                    goalsPerformanceDto.Add(dto);
                }
                domain_dto.goalsperformance = goalsPerformanceDto;
                result.Add(domain_dto);
            }

            return result;
        }

        internal List<GetStudentDto> GetStudentsByTag(string tags)
        {
            var instId = _uc.InstId;
            var tag_list = tags.ToLower().Split(',');
            var students = new List<GetStudentDto>();
            var qry = _session.Query<Student_Inst_Index.Result>()
                                .Where(o => o.InstId.Equals(instId))
                                .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                                .Where(o => o.Tags.Contains(tags))
                                .OfType<Student>();


            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    var dto = get_stu_dto(stu);
                    students.Add(dto);
                }
            }

            students = students
                           .OrderByDescending(o => Enumerable.Reverse(_policyEngine.CourseOrder()).ToList().IndexOf(o.CourseName))
                           .ThenBy(o => o.SectionName)
                           .ThenBy(o => o.Name)
                           .ToList();

            return students;
        }

        internal List<Facet> GetFacets(bool includeStudents = true)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<Facet>();

            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);
            Expression<Func<Student_Inst_Index.Result, bool>> filterByEnrollmentStatus = o => o.EnrollmentStatus == EnrollmentStatusEnum.Active;

            //qry
            var qry = _session
                            .Query<Student_Inst_Index.Result>()
                            .Where(o => o.InstId == inst_id)
                            .Where(filterByEnrollmentStatus)
                            .ProjectInto<Student_Inst_Index.Result>();


            var stus = new List<Student_Inst_Index.Result>();
            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    stus.Add(stu);
                }
            }

            var group = stus
                               .OrderBy(o => o.CourseSortIndex).ThenBy(o => o.SectionName)
                               .GroupBy(o => new { o.CourseId, o.CourseNameAndBranch })
                               .Select(g => new
                               {
                                   CourseId = g.Key.CourseId,
                                   CourseName = g.Key.CourseNameAndBranch,
                                   Sections = g.ToList()
                                                .GroupBy(s => s.SectionName)
                                                .Select(x => new { Section = x.Key, Students = x.ToList() })

                               });
            var courses = group.ToList();

            var parent_facet = new Facet() { ElemId = "Parent", Name = "Parent", Edit = false };
            var student_facet = new Facet() { ElemId = "Student", Name = "Student", Edit = false };
            foreach (var c in courses)
            {
                var course_facet = new Facet() { ElemId = c.CourseId, Name = c.CourseName, Edit = false };
                foreach (var sec in c.Sections)
                {
                    var sec_facet = new Facet() { ElemId = sec.Section, Name = sec.Section, Edit = false };
                    var stu_list = sec.Students.OrderBy(o => o.Name).Select(o => new { o.Id, o.Name }).ToList();
                    if (includeStudents)
                    {
                        foreach (var stu in stu_list)
                        {
                            var stu_facet = new Facet() { ElemId = stu.Id, Name = stu.Name, Edit = false };
                            sec_facet.Children.Add(stu_facet);
                        }
                    }
                    course_facet.Children.Add(sec_facet);
                }
                parent_facet.Children.Add(course_facet);
                if (includeStudents) student_facet.Children.Add(course_facet);
            }

            result.Add(parent_facet);
            if (includeStudents) result.Add(student_facet);

            return result;
        }

        /// <summary>
        /// Get Dto with Data
        /// </summary>
        /// <returns></returns>
        internal List<Student_Inst_Index.Result> GetDtoWithData()
        {
            var instId = _uc.InstId;
            var qry = _session.Query<Student_Inst_Index.Result>()
                                .Where(o => o.InstId.Equals(instId));
            var students = new List<Student_Inst_Index.Result>();
            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var recpt = enumerator.Current.Document;
                    students.Add(recpt);
                }
            }
            return students;
        }

        /// <summary>
        /// Get Students For Exam Facets
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public dynamic GetStudentsForExamFacets(string courseId)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);

            var section = "";
            var stu_list = GetAll(courseId, section);

            var group = stu_list
                            .OrderBy(o => o.SectionName)
                            .GroupBy(o => o.SectionName)
                            .Select(g => new { Section = g.Key, Students = g.ToList() });

            var result = new ExamFacetStudentDto()
            {
                InstId = inst_id,
                CourseId = courseId,
            };
            foreach (var item in group)
            {
                var stus = new List<EF_StudentDetail>();
                foreach (var stu in item.Students)
                {
                    var stu_dto = new EF_StudentDetail()
                    {
                        StudentId = stu.Id,
                        Name = stu.Name
                    };
                    stus.Add(stu_dto);
                }
                var section_dto = new EF_SectionDetails()
                {
                    SectionName = item.Section,
                    StudentList = stus
                };
                result.Sections.Add(section_dto);
            }
            return result;
        }
        /// <summary>
        /// Get Student Data in flat format
        /// </summary>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        public List<StudentFlatDto> GetFlatStudentData(List<string> studentIds = null)
        {
            var instId = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(instId);

            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId == instId;
            Expression<Func<Student_Inst_Index.Result, bool>> filterByStudents = o => o.Id.In(studentIds);

            var qry = _session.Query<Student_Inst_Index.Result>();
            qry = qry.Where(filterByInst);
            qry = studentIds == null || studentIds.Count == 0 ? qry : qry.Where(filterByStudents);
            var stu_qry = qry.OfType<Student>();

            var students = new List<StudentFlatDto>();
            var courses = RedisCache.GetCourseDto(instId);
            using (var enumerator = _session.Stream(stu_qry))
            {
                while (enumerator.MoveNext())
                {
                    var stud = enumerator.Current.Document;
                    var enrl = stud.Enrollments.First(o => o.InstId == _uc.InstId);

                    var parent_mobiles = stud.GetMobiles();
                    var parent_emails = stud.GetEmails();

                    var stud_course = courses.Find(o => o.Id.Equals(enrl.CourseId));
                    var studentFlatDto = new StudentFlatDto()
                    {
                        Id = stud.Id,
                        InstId = enrl.InstId,
                        Name = stud.Name,
                        DOB = stud.DOB,
                        Gender = stud.Gender.ToString(),
                        CourseId = enrl.CourseId,
                        Course = stud_course.Name,
                        Term = enrl.TermName,
                        Section = enrl.SectionName,
                        Branch = enrl.Branch,
                        StudentRefId = enrl.StudentId,
                        StudentMobile = stud.Mobile,
                        FatherMobile = parent_mobiles[0],
                        MotherMobile = parent_mobiles[1],
                        GuardianMobile = parent_mobiles[2],
                        StudentEmail = stud.Email,
                        FatherEmail = parent_emails[0],
                        MotherEmail = parent_emails[1],
                        GuardianEmail = parent_emails[2],
                        VirtualId = stud.VirtualId,
                        UserId = stud.UserId,
                        Status = enrl.Status.ToString()
                    };
                    students.Add(studentFlatDto);
                }
            }
            return students;
        }

        internal List<StudentProfile> GetStudentProfile(string userEmail = "", string id = "")
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var student_list = new List<StudentProfile>();
            Expression<Func<Student_Inst_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_id);
            Expression<Func<Student_Inst_Index.Result, bool>> filterByActiveStudents = o => o.EnrollmentStatus == EnrollmentStatusEnum.Active;

            if (!string.IsNullOrEmpty(userEmail))
            {
                if (userEmail.EndsWith("@campustrack.net", StringComparison.CurrentCultureIgnoreCase) || userEmail.EndsWith("@vvssppuc.org", StringComparison.CurrentCultureIgnoreCase))
                {
                    //if virtual id, try and get it from cache.
                    // cache key deleted on /api/stu/Update and /api/stu/UpdateCourse

                    var stu_profiles = RedisCache.GetStudentProfiles(userEmail.ToLower().Trim());
                    if (stu_profiles != null && stu_profiles.Count > 0) return stu_profiles;
                }
                var qry = _session
                            .Query<Student_Inst_Index.Result>()
                            .Where(filterByInst)
                            .Where(filterByActiveStudents)
                            .Where(o => o.VirtualId == userEmail || o.EmailList.Contains(userEmail))
                            .OfType<Student>()
                            .ToList();
                foreach (var stu in qry)
                {
                    var stu_profile = create_student_profile_dto(stu);
                    student_list.Add(stu_profile);
                }
                //let's add the profiles to cache so that it can be retrieved from cache in the next request.
                RedisCache.SetStudentProfiles(userEmail.ToLower().Trim(), student_list);
                return student_list;
            }
            else if (!string.IsNullOrEmpty(id))
            {
                var student = _session.Load<Student>(id);
                if (student == null) return student_list;

                var stu_profile = create_student_profile_dto(student);
                student_list.Add(stu_profile);
                return student_list;
            }
            return student_list;
        }

        internal StudentProfile create_student_profile_dto(Student student)
        {
            var enrl = student.Enrollments.First(o => o.InstId == _uc.InstId);

            var student_profile_dto = new StudentProfile();
            student_profile_dto.Id = student.Id;
            student_profile_dto.Name = student.Name;
            student_profile_dto.CourseName = enrl.CourseName;
            student_profile_dto.CourseId = enrl.CourseId;
            student_profile_dto.SectionName = enrl.SectionName;
            student_profile_dto.Branch = enrl.Branch;

            return student_profile_dto;
        }

        internal List<CoursewiseStudentDto> GetGenderBreakupWidgetData(string courseId = "all")
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<CoursewiseStudentDto>();

            var qry = _session
                            .Query<Student_Coursewise_Count.Result>()
                            .Where(o => o.InstId == inst_id);

            if (!string.IsNullOrEmpty(courseId) && (!courseId.Equals("all", StringComparison.InvariantCultureIgnoreCase)))
                qry = qry.Where(o => o.CourseId.Equals(courseId));

            var stus = qry.ToList();

            var group = stus
                            .GroupBy(o => new { o.CourseId, o.CourseName })
                            .Select(g => new
                            {
                                CourseId = g.Key.CourseId,
                                CourseName = g.Key.CourseName,
                                Sections = g.GroupBy(s => new { s.SectionName })
                                .Select(b => new
                                {
                                    Section = b.Key.SectionName,
                                    Boys = b.First().Boys,
                                    Girls = b.First().Girls,
                                })
                            });

            foreach (var c in group)
            {
                var dto = new CoursewiseStudentDto()
                {
                    InstId = inst_id,
                    CourseId = c.CourseId,
                    CourseName = c.CourseName
                };
                var _sections = c.Sections.OrderBy(o => o.Section).ToList();
                foreach (var s in _sections)
                {
                    var sec_dto = new SectionDetail()
                    {
                        SectionName = s.Section,
                        NumberOfBoys = s.Boys,
                        NumberOfGirls = s.Girls,
                    };
                    dto.Sections.Add(sec_dto);
                }
                result.Add(dto);
            }

            result = result.OrderByDescending(o => Enumerable.Reverse(_policyEngine.CourseOrder()).ToList().IndexOf(o.CourseName)).ToList();
            return result;
        }

        public dynamic GetAttendanceWidgetData(string instId = "")
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);
            var qry = _session
                              .Query<AttendanceEvent_IX.Result>()
                              .Where(o => o.InstId == inst_id)
                              .ToList();

            var group = qry
                       .GroupBy(o => new { o.Date })
                       .Select(g => new
                       {
                           Date = g.Key.Date,
                           Absentees = g.Count()
                       });

            return group;
        }

        /// <summary>
        /// Get student count institute genderwise
        /// </summary>
        /// <param name="instituteIds"></param>
        /// <returns></returns>
        public dynamic GetStudentCountInstituteGenderwise(List<string> instituteIds = null)
        {
            if (instituteIds == null) throw new ArgumentNullException("Institute id list is empty");
            var qry = _session
                            .Query<Inst_Student_Count.Result>()
                            .Where(o => o.InstId.In(instituteIds));

            var students = qry.ToList();

            var group = students.GroupBy(o => new { o.InstId })
                                 .Select(g => new
                                 {
                                     InstId = g.Key.InstId,
                                     Boys = g.Select(o => o.StudentCountMale).FirstOrDefault(),
                                     Girls = g.Select(o => o.StudentCountFemale).FirstOrDefault()
                                 })
                                 .ToList();

            return group;
        }

        /// <summary>
        /// Total student count institutewise
        /// </summary>
        /// <param name="instituteIds"></param>
        /// <returns></returns>
        public dynamic GetStudentCountInstitutewise(List<string> instituteIds = null)
        {
            if (instituteIds == null) throw new ArgumentNullException("Institute id list is empty");
            var qry = _session
                            .Query<Inst_Student_Count.Result>()
                            .Where(o => o.InstId.In(instituteIds));

            var students = qry.ToList();

            var group = students.GroupBy(o => new { o.InstId })
                                 .Select(g => new
                                 {
                                     InstId = g.Key.InstId,
                                     TotalStudentCount = g.Select(o => o.TotalStudentCount),
                                 })
                                 .ToList();

            return group;
        }

        internal List<SubscriptionRecipientDto> GetSubscriptionRecipients(RecipientsFacet Recipients = null)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);
            var result = new List<SubscriptionRecipientDto>();
            //students
            var stu_recipients = Recipients.Student?.Count > 0 ? Recipients.Student : new List<List<string>>();
            if (stu_recipients.Count > 0)
            {
                string s_pred = get_predicate(stu_recipients);
                result.AddRange(GetRecipients(s_pred, studentMobiles: true));
            }
            //Parents
            var p_recipients = Recipients.Parent?.Count > 0 ? Recipients.Parent : new List<List<string>>();
            if (p_recipients.Count > 0)
            {
                var pred = get_predicate(p_recipients);
                result.AddRange(GetRecipients(pred, parentMobiles: true));
            }
            return result;
        }
        /// <summary>
        /// Get Predicate
        /// </summary>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public string get_predicate(List<List<string>> recipients)
        {
            var studentPredicates = new List<string>();
            var lucenePredicates = new List<string>();
            foreach (var plist in recipients)
            {
                if (plist == null || plist.Count != 4) break;
                if (plist.Last() != "*") //specific student
                {
                    studentPredicates.Add($"Id:{plist[3]}");
                    continue;
                }
                else if (plist.First().Equals("*")) //all courses & hence all students.
                {
                    studentPredicates.Clear();
                    lucenePredicates.Clear();
                    break;
                }
                else
                {
                    var x = plist;
                    var c = x[0];
                    var t = x[1];
                    var s = x[2];

                    var q = $"CourseId:{c}";
                    q = t == "*" ? q : q + $" AND TermName:{t}";
                    q = s == "*" ? q : q + $" AND SectionName:{s}";
                    lucenePredicates.Add($"({q})");
                }
            }
            var stuPred = studentPredicates.Count == 0 ? "" : "(" + string.Join(" OR ", studentPredicates) + ")";
            lucenePredicates.Add(stuPred);
            var pred = string.Join(" OR ", lucenePredicates.Where(s => !string.IsNullOrEmpty(s)));
            return pred;
        }
        internal List<SubscriptionRecipientDto> GetRecipients(string lucenePredicate = "", bool parentMobiles = false, bool studentMobiles = false, bool includeTags = false)
        {
            var inst_id = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(inst_id);

            var result = new List<SubscriptionRecipientDto>();

            lucenePredicate = lucenePredicate == "" ? lucenePredicate : $"({lucenePredicate})";
            var predicates = new List<string>() { lucenePredicate, $"(InstId:{inst_id})", $"(EnrollmentStatus:{EnrollmentStatusEnum.Active})" };

            if (includeTags) predicates.Add($"(Tags:(ios oc))");

            var predicate = string.Join(" AND ", predicates.Where(s => !string.IsNullOrEmpty(s)));
            var qry = _session.Advanced
                            .Query<Student_Inst_Index.Result>()
                            .WhereLucene("Name", predicate)
                            .OfType<Student>();

            using (var enumerator = _session.Stream(qry))
            {
                while (enumerator.MoveNext())
                {
                    var stu = enumerator.Current.Document;
                    if (stu != null)
                    {
                        var dto = new SubscriptionRecipientDto();
                        dto.Id = stu.Id;
                        dto.Name = stu.Name;
                        dto.Email = stu.Email;
                        dto.Mobile = stu.Mobile;
                        dto.Vemail = stu.VirtualId;
                        dto.Fname = stu.Father.Name;
                        dto.Femail = stu.Father.Email;
                        dto.Fmobile = stu.Father.Mobile;
                        dto.Mname = stu.Mother.Name;
                        dto.Memail = stu.Mother.Email;
                        dto.Mmobile = stu.Mother.Mobile;
                        dto.Gname = stu.Guardian.Name;
                        dto.Gemail = stu.Guardian.Email;
                        dto.Gmobile = stu.Guardian.Mobile;
                        dto.UserId = stu.UserId;
                        result.Add(dto);
                    }
                }
            }
            return result;
        }

        public List<IEPTerm> GetIEPDetails(string id)
        {
            var instId = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(instId);

            var student = _session.Load<Student>(id);
            if (student == null) throw new ObjectNotFoundException();

            var iep = student.IEP.Terms.Where(o => o.InstId == instId).ToList();

            return iep;
        }

        public bool StudentEssentialsExists(string studentId)
        {
            var recordExists = _uc.Session
                                    .Query<StudentEssentials>()
                                    .Where(o => o.StudentId == studentId)
                                    .FirstOrDefault();
            return recordExists == null ? false : true;
        }

        public StudentEssentialsDto GetStudentEssentials(string studentId)
        {
            var essentials = _uc.Session
                                   .Query<StudentEssentials>()
                                   .Where(o => o.StudentId == studentId)
                                   .FirstOrDefault();

            var result = GetStudentEssentialsDto(essentials);
            return result;
        }

        public StudentEssentialsDto GetStudentEssentialsDto(StudentEssentials json)
        {
            var dto = new StudentEssentialsDto();
            dto.Id = json.Id;
            dto.StudentId = json.StudentId;

            var itemsList = new List<ItemDto>();
            foreach (var item in json.Items)
            {
                var _item = new ItemDto();
                _item.Name = item.Name;
                _item.Amount = item.Amount;
                _item.Date = item.Date;
                _item.Id = item.Id;
                _item.InstId = item.InstId;
                _item.Status = item.Status;
                itemsList.Add(_item);
            }
            dto.Items = itemsList;

            return dto;
        }

        /// <summary>
        /// Get baseline data collection form
        /// </summary>
        /// <returns></returns>
        public BaselineDataForm GetBaselineDataCollectionForm()
        {
            var instId = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(instId);

            var baselineDataForm = new BaselineDataForm();

            var _pt_ot = baselineDataForm.BaselinePTOT;
            _pt_ot.Condition = _policyEngine.DisabilityCondition();
            _pt_ot.Speech = _policyEngine.SpeechCondition();
            _pt_ot.NameCallResponse = _policyEngine.NameCallResponse();
            _pt_ot.ActivityFocusAttention = _policyEngine.ActivityFocusAttention();
            _pt_ot.EyeContact = _policyEngine.EyeContact();
            _pt_ot.SittingTolerance = _policyEngine.SittingTolerance();
            _pt_ot.InstructionFollowing = _policyEngine.InstructionFollowing();
            _pt_ot.BodyBuilt = _policyEngine.BodyBuilt();
            _pt_ot.Posture = string.Empty;
            _pt_ot.MuscleTone = _policyEngine.MuscleTone();
            _pt_ot.MuscleTightness = string.Empty;
            _pt_ot.Grasp = string.Empty;
            _pt_ot.Precision = string.Empty;
            _pt_ot.Proprioception = _policyEngine.Proprioception();
            _pt_ot.StaticBalance = _policyEngine.StaticBalance();
            _pt_ot.DynamicBalance = _policyEngine.DynamicBalance();
            _pt_ot.Coordination = string.Empty;
            _pt_ot.Gait = string.Empty;
            _pt_ot.SensoryProfile = string.Empty;
            _pt_ot.Behavior = _policyEngine.Behavior();

            var _speech_language = baselineDataForm.BaselineSpeechLanguage;
            _speech_language.ActivityFocusTopic = _policyEngine.TopicFocusAttention();
            _speech_language.Speech = _policyEngine.SpeechCondition();
            _speech_language.Condition = _policyEngine.DisabilityConditionSL();
            _speech_language.EyeContact = _policyEngine.EyeContactSL();
            _speech_language.SittingTolerance = _policyEngine.SittingToleranceSL();
            _speech_language.ActivityCompliance = _policyEngine.ActivityCompliance();
            _speech_language.ReceptiveLanguage = _policyEngine.ReceptiveLanguage();
            _speech_language.ExpressiveLanguage = _policyEngine.ExpressiveLanguage();
            _speech_language.SocialCommunication = _policyEngine.SocialCommunication();
            _speech_language.PragmaticsSkills = _policyEngine.PragmaticsSkills();
            _speech_language.Intelligibility = _policyEngine.Intelligibility();
            _speech_language.Behavior = _policyEngine.BehaviorSL();
            _speech_language.Literacy = _policyEngine.Literacy();
            _speech_language.Recommendation = _policyEngine.Recommendation();

            var _special_education = baselineDataForm.BaselineSpecialEducation;
            _special_education.Condition = _policyEngine.Conditions();
            _special_education.Speech = _policyEngine.SpeechCondition();
            _special_education.ActivityFocusAttention = _policyEngine.ActivityFocusAttentionSE();
            _special_education.EyeContact = _policyEngine.EyeContactSE();
            _special_education.SittingTolerance = _policyEngine.SittingToleranceSL();
            _special_education.InstructionsComprehension = _policyEngine.InstructionsComprehension();
            _special_education.ActivityCompliance = _policyEngine.ActivityComplianceSE();
            _special_education.Behavior = _policyEngine.BehaviorSL();
            _special_education.BehaviorsHinderingLearning = _policyEngine.BehaviorsHinderingLearning();
            _special_education.Writing = string.Empty;
            _special_education.Literacy = string.Empty;
            _special_education.NumberConcept = string.Empty;
            _special_education.CommunicationConversationalSkill = string.Empty;
            _special_education.Cognition = string.Empty;
            _special_education.EVS = string.Empty;
            _special_education.GroupSession = new GroupSession();
            _special_education.BasicLifeSkillsObservations = new BasicLifeSkills();
            _special_education.AdditionalComments = string.Empty;

            return baselineDataForm;
        }

        /// <summary>
        /// Get baseline data collection
        /// </summary>
        /// <returns></returns>
        public List<BaselineData> GetBaselineDataCollection()
        {
            var instId = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(instId);

            var qry = _session.Query<BaselineData>()
                              .Where(o => o.OwnerId == instId)
                              .ToList();

            return qry;
        }

        /// <summary>
        /// Get baseline by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaselineData GetBaselineDataById(string id)
        {
            var instId = _uc.InstId;
            if (!_uc.BoundByInst) throw new NotInScopeException(instId);

            var qry = _session.Load<BaselineData>(id);
            return qry;
        }
    }
}