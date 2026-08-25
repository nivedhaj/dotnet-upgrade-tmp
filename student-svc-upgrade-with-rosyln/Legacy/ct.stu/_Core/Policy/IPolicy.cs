using Autofac.Features.Indexed;
using ct._Core.Repo.Dto;
using ct._Core.Repo.Resource;
using System;
using System.Collections.Generic;

namespace ct._Core.Policy
{
    /// <summary>
    /// Policy Engine Interface
    /// </summary>
    public interface IPolicyEngine
    {
        /// <summary>
        /// Course Prefix To be used while generating Student Id
        /// </summary>
        /// <param name="courseName"></param>
        string CoursePrefix(string courseName);
        /// <summary>
        /// No of sessions for which attendance is taken. Can be same as no of periods but not necessarily.
        /// </summary>
        int AttendanceSessions { get; }
        /// <summary>
        /// SMS Gateway to be used for the institute
        /// </summary>
        SMSGatewayEnum SMSGateway { get; }
        /// <summary>
        /// Message sent to absentees via SMS
        /// </summary>
        string MessageForAbsentees { get; }
        /// <summary>
        /// Course Order
        /// </summary>
        /// <returns></returns>
        List<string> CourseOrder();
        /// <summary>
        /// Get print options
        /// </summary>
        /// <param name="inst_short_name"></param>
        /// <returns></returns>
        PrintOptions GetPrintOptions();
        /// <summary>
        /// Auto Generate Admission No
        /// </summary>
        /// <value></value>
        bool AutoGenerateAdmissionNo { get; }
        /// <summary>
        /// Admission number prefix
        /// </summary>
        /// <param name="acadYearFrom"></param>
        /// <param name="acadYearTo"></param>
        /// <returns></returns>
        string AdmissionNoPrefix(string acadYearFrom, string acadYearTo);
        List<BaselineItem> DisabilityCondition();
        List<BaselineItem> SpeechCondition();
        List<BaselineItem> NameCallResponse();
        List<BaselineItem> ActivityFocusAttention();
        List<BaselineItem> EyeContact();
        List<BaselineItem> SittingTolerance();
        List<BaselineItem> InstructionFollowing();
        List<BaselineItem> MuscleTone();
        List<BaselineItem> BodyBuilt();
        List<BaselineItem> Proprioception();
        List<BaselineItem> StaticBalance();
        List<BaselineItem> DynamicBalance();
        List<BaselineItem> Behavior();
        List<BaselineItem> TopicFocusAttention();
        List<BaselineItem> DisabilityConditionSL();
        List<BaselineItem> EyeContactSL();
        List<BaselineItem> SittingToleranceSL();
        List<BaselineItem> ActivityCompliance();
        List<BaselineItem> ReceptiveLanguage();
        List<BaselineItem> ExpressiveLanguage();
        List<BaselineItem> SocialCommunication();
        List<BaselineItem> PragmaticsSkills();
        List<BaselineItem> Intelligibility();
        List<BaselineItem> BehaviorSL();
        List<BaselineItem> Literacy();
        List<BaselineItem> Recommendation();
        List<BaselineItem> Conditions();
        List<BaselineItem> ActivityFocusAttentionSE();
        List<BaselineItem> EyeContactSE();
        List<BaselineItem> InstructionsComprehension();
        List<BaselineItem> ActivityComplianceSE();
        List<BaselineItem> BehaviorsHinderingLearning();
    }
    /// <summary>
    /// Policy Engine enum
    /// </summary>
    public enum PolicyEngineEnum
    {
        /// <summary>
        /// Default
        /// </summary>
        Default,
        /// <summary>
        /// BTLCOL
        /// </summary>
        BTLCOL,
        /// <summary>
        /// VVSGCS
        /// </summary>
        VVSGCS,
        /// <summary>
        /// VVSPHS
        /// </summary>
        VVSPHS,
        /// <summary>
        /// VVSSPU
        /// </summary>
        VVSSPU,
        ///
        /// VVSPUC
        /// 
        VVSPUC,
        AKSHFN

    }
    /// <summary>
    /// Policy Engine Provider
    /// </summary>
    public interface IPolicyEngineProvider
    {
        /// <summary>
        /// Get the Policy Engine For Policy Id
        /// </summary>
        /// <returns></returns>
        IPolicyEngine For(string policyId);
    }

    /// <summary>
    /// Policy Engine (Typed) Factory
    /// </summary>
    /// Ref: http://autofaccn.readthedocs.io/en/latest/faq/injecting-configured-parameters.html
    public class PolicyEngineProvider : IPolicyEngineProvider
    {
        IIndex<PolicyEngineEnum, IPolicyEngine> _policyEngines;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="policyEngines"></param>
        public PolicyEngineProvider(IIndex<PolicyEngineEnum, IPolicyEngine> policyEngines)
        {
            _policyEngines = policyEngines;
        }
        /// <summary>
        /// Current Policy Engine for policy Id
        /// </summary>
        /// <returns></returns>
        public IPolicyEngine For(string policyId = "")
        {
            var policy_id = string.IsNullOrEmpty(policyId) ? "Default" : policyId;
            Enum.TryParse(policyId, out PolicyEngineEnum policy_engine_enum);
            var policy = _policyEngines[policy_engine_enum];
            return policy;
        }
    }
}