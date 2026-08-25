using System;
using System.Collections.Generic;
using ct._Core.Repo.Dto;
using ct._Core.Repo.Resource;

namespace Repo.Resource
{

    /// <summary>
    /// Baseline data form
    /// </summary>
    public class BaselineDataForm
    {
        public BaselinePTOT BaselinePTOT { get; set; }
        public BaselineSpeechLanguage BaselineSpeechLanguage { get; set; }
        public BaselineSpecialEducation BaselineSpecialEducation { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        public BaselineDataForm()
        {
            BaselinePTOT = new BaselinePTOT();
            BaselineSpeechLanguage = new BaselineSpeechLanguage();
            BaselineSpecialEducation = new BaselineSpecialEducation();
        }
    }

    /// <summary>
    /// Student IEP
    /// </summary>
    public class BaselineData : Entity
    {
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public string AgeAsOnDOOb { get; set; }
        public GenderEnum Gender { get; set; }
        public string TherapistName { get; set; }
        public DateTime DateOfObservation { get; set; }
        public string Language { get; set; }
        public BaselinePTOT BaselinePTOT { get; set; }
        public BaselineSpeechLanguage BaselineSpeechLanguage { get; set; }
        public BaselineSpecialEducation BaselineSpecialEducation { get; set; }
        public bool ConsultedForPTOT { get; set; }
        public bool ConsultedForSpeechLanguage { get; set; }
        public bool ConsultedForSpecialEducation { get; set; }
        public BaselineData()
        {
            BaselinePTOT = new BaselinePTOT();
            BaselineSpeechLanguage = new BaselineSpeechLanguage();
            BaselineSpecialEducation = new BaselineSpecialEducation();
        }
    }

    /// <summary>
    /// Baseline speech language
    /// </summary>
    public class BaselineSpeechLanguage
    {
        public List<BaselineItem> ActivityFocusTopic { get; set; }
        public List<BaselineItem> Speech { get; set; }
        public List<BaselineItem> Condition { get; set; }
        public List<BaselineItem> EyeContact { get; set; }
        public List<BaselineItem> SittingTolerance { get; set; }
        public List<BaselineItem> ActivityCompliance { get; set; }
        public List<BaselineItem> ReceptiveLanguage { get; set; }
        public List<BaselineItem> ExpressiveLanguage { get; set; }
        public List<BaselineItem> SocialCommunication { get; set; }
        public List<BaselineItem> PragmaticsSkills { get; set; }
        public List<BaselineItem> Intelligibility { get; set; }
        public List<BaselineItem> Behavior { get; set; }
        public List<BaselineItem> Literacy { get; set; }
        public List<BaselineItem> Recommendation { get; set; }
        public BaselineSpeechLanguage()
        {
            ActivityFocusTopic = new List<BaselineItem>();
            Speech = new List<BaselineItem>();
            Condition = new List<BaselineItem>();
            EyeContact = new List<BaselineItem>();
            SittingTolerance = new List<BaselineItem>();
            ActivityCompliance = new List<BaselineItem>();
            ReceptiveLanguage = new List<BaselineItem>();
            ExpressiveLanguage = new List<BaselineItem>();
            SocialCommunication = new List<BaselineItem>();
            PragmaticsSkills = new List<BaselineItem>();
            Intelligibility = new List<BaselineItem>();
            Behavior = new List<BaselineItem>();
            Literacy = new List<BaselineItem>();
            Recommendation = new List<BaselineItem>();
        }
    }

    /// <summary>
    /// PT/OT (Physical Therapy / Occupational Therapy)
    /// </summary>
    public class BaselinePTOT
    {
        public List<BaselineItem> Condition { get; set; }
        public List<BaselineItem> Speech { get; set; }
        public List<BaselineItem> NameCallResponse { get; set; }
        public List<BaselineItem> ActivityFocusAttention { get; set; }
        public List<BaselineItem> EyeContact { get; set; }
        public List<BaselineItem> SittingTolerance { get; set; }
        public List<BaselineItem> InstructionFollowing { get; set; }
        public List<BaselineItem> BodyBuilt { get; set; }
        public string Posture { get; set; }
        public List<BaselineItem> MuscleTone { get; set; }
        public string MuscleTightness { get; set; }
        public string Grasp { get; set; }
        public string Precision { get; set; }
        public List<BaselineItem> Proprioception { get; set; }
        public List<BaselineItem> StaticBalance { get; set; }
        public List<BaselineItem> DynamicBalance { get; set; }
        public string Coordination { get; set; }
        public string Gait { get; set; }
        public string SensoryProfile { get; set; }
        public List<BaselineItem> Behavior { get; set; }
        public BaselinePTOT()
        {
            Condition = new List<BaselineItem>();
            Speech = new List<BaselineItem>();
            NameCallResponse = new List<BaselineItem>();
            ActivityFocusAttention = new List<BaselineItem>();
            EyeContact = new List<BaselineItem>();
            SittingTolerance = new List<BaselineItem>();
            InstructionFollowing = new List<BaselineItem>();
            BodyBuilt = new List<BaselineItem>();
            MuscleTone = new List<BaselineItem>();
            Proprioception = new List<BaselineItem>();
            StaticBalance = new List<BaselineItem>();
            DynamicBalance = new List<BaselineItem>();
            Behavior = new List<BaselineItem>();
        }
    }

    public class BaselineSpecialEducation
    {
        public List<BaselineItem> Condition { get; set; }
        public List<BaselineItem> Speech { get; set; }
        public List<BaselineItem> ActivityFocusAttention { get; set; }
        public List<BaselineItem> EyeContact { get; set; }
        public List<BaselineItem> SittingTolerance { get; set; }
        public List<BaselineItem> InstructionsComprehension { get; set; }
        public List<BaselineItem> ActivityCompliance { get; set; }
        public List<BaselineItem> Behavior { get; set; }
        public List<BaselineItem> BehaviorsHinderingLearning { get; set; }
        public string Writing { get; set; }
        public string Literacy { get; set; }
        public string NumberConcept { get; set; }
        public string CommunicationConversationalSkill { get; set; }
        public string Cognition { get; set; }
        public string EVS { get; set; }
        public GroupSession GroupSession { get; set; }
        public BasicLifeSkills BasicLifeSkillsObservations { get; set; }
        public string AdditionalComments { get; set; }
        public BaselineSpecialEducation()
        {
            Condition = new List<BaselineItem>();
            Speech = new List<BaselineItem>();
            ActivityFocusAttention = new List<BaselineItem>();
            EyeContact = new List<BaselineItem>();
            SittingTolerance = new List<BaselineItem>();
            InstructionsComprehension = new List<BaselineItem>();
            ActivityCompliance = new List<BaselineItem>();
            Behavior = new List<BaselineItem>();
            BehaviorsHinderingLearning = new List<BaselineItem>();
            GroupSession = new GroupSession();
            BasicLifeSkillsObservations = new BasicLifeSkills();
        }
    }

    public class GroupSession
    {
        public string Appearance { get; set; }
        public string GruopSittingBehavior { get; set; }
        public string BehaviorObserved { get; set; }
        public string FollowingInstructions { get; set; }
        public string WaitingForTurn { get; set; }
        public string ParticipationInActivity { get; set; }
    }

    public class BasicLifeSkills
    {
        public string EatingHabits { get; set; }
        public string ToiletTraining { get; set; }
        public string KnowingBelongings { get; set; }
        public string DressingSkills { get; set; }
    }
}