using System;
using ct._Core.Repo.Resource;
using Repo.Resource;

namespace Repo.Dto
{
    /// <summary>
    /// Create Baseline Data Dto
    /// </summary>
    public class CreateBaselineDataDto
    {
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public DateTime DOOb { get; set; }
        public string AgeAsOnDOOb { get; set; }
        public string TherapistName { get; set; }
        public string Language { get; set; }
        public GenderEnum Gender { get; set; }
        public bool ConsultedPTOT { get; set; }
        public bool consultedSpeechLanguage { get; set; }
        /// <summary>
        /// Baseline PTOT
        /// </summary>
        /// <value></value>
        public BaselinePTOT BaselinePTOT { get; set; }
        /// <summary>
        /// Baseline SpeechLanguage
        /// </summary>
        /// <value></value>
        public BaselineSpeechLanguage BaselineSpeechLanguage { get; set; }
    }
}