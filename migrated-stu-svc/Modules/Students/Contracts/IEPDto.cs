using System;
using System.Collections.Generic;
using Repo.Resource;

namespace Repo.Dto
{
    public class CreateIEPDto
    {
        public string StudentId { get; set; }
        public List<IEPTerm> Terms { get; set; }
    }

    public class IEPReportDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime? DOB { get; set; }
        public string DOBAsString { get { return string.Format("{0:dd MMM yyyy}", DOB); } }
        public string Age { get; set; }
        public IndividualEducationPlan IEP { get; set; }
    }

    public class DomainDto
    {
        public string domain { get; set; }
        public List<GoalsPerformanceDto> goalsperformance { get; set; }
        public DomainDto()
        {
            goalsperformance = new List<GoalsPerformanceDto>();
        }
    }

    public class GoalsPerformanceDto
    {
        public string goal { get; set; }
        public decimal performance { get; set; }
    }
}