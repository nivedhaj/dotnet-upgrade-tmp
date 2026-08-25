using System;
using System.Collections.Generic;
using System.Linq;

namespace Repo.Resource
{
    /// <summary>
    /// Student IEP
    /// </summary>
    public class IndividualEducationPlan
    {
        public List<IEPTerm> Terms { get; set; }
        public IndividualEducationPlan()
        {
            Terms = new List<IEPTerm>();
        }
    }

    public class IEPTerm
    {
        public string Title { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int WorkingDays { get; set; }
        public int DaysPresent { get; set; }

        public List<Domain> Domains { get; set; }
        public string InstId { get; set; }
        public IEPTerm()
        {
            Domains = new List<Domain>();
        }
    }

    public class Domain
    {
        public string Title { get; set; }
        public string Educators { get; set; }
        // public string Performance { get; set; }
        // public string Goals { get; set; }
        public List<GoalsPerformance> GoalsPerformance { get; set; }
        public string Evaluations { get; set; }
        public string Remarks { get; set; }
        public Domain()
        {
            GoalsPerformance = new List<GoalsPerformance>();
        }

        public void AddGoalsPerformance(string goal, decimal performance)
        {
            if (string.IsNullOrEmpty(goal)) throw new ArgumentNullException("goal");
            var newId = this.getGoalsPerformanceId();
            var gp = new GoalsPerformance(newId)
            {
                Goal = goal,
                Performance = performance
            };
            this.GoalsPerformance.Add(gp);
        }

        private int getGoalsPerformanceId()
        {
            if (this.GoalsPerformance == null || this.GoalsPerformance.Count == 0) return 1;
            int lastId = this.GoalsPerformance.OrderByDescending(o => o.Id).Take(1).Select(o => o.Id).SingleOrDefault();
            return lastId + 1;
        }

        public void Sanitize()
        {
            var installmentsCopy = this.GoalsPerformance;
            this.GoalsPerformance = new List<GoalsPerformance>();
            foreach (var instlmnt in installmentsCopy ?? new List<GoalsPerformance>())
            {
                if (!string.IsNullOrEmpty(instlmnt.Goal))
                {
                    this.AddGoalsPerformance(instlmnt.Goal, instlmnt.Performance);
                }
            }
        }
    }

    public class GoalsPerformance
    {
        public int Id { get; set; }
        public string Goal { get; set; }
        public decimal Performance { get; set; }
        public GoalsPerformance() { }

        public GoalsPerformance(int id)
        : this()
        {
            if (id <= 0) throw new ArgumentNullException("id");
            Id = id;
        }
    }
}