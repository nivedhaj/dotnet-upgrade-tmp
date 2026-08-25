using ct._Core.Repo.Dto;
using ct._Core.Repo.Resource;
using System.Collections.Generic;

namespace ct._Core.Policy
{
    /// <summary>
    /// Default Policy Engine
    /// </summary>
    public class DefaultPolicyEngine : IPolicyEngine
    {
        /// <summary>
        /// No of sessions for which attendance is taken. Can be same as no of periods but not necessarily.
        /// </summary>
        public int AttendanceSessions => 1;
        /// <summary>
        /// SMS Gateway
        /// </summary>
        public SMSGatewayEnum SMSGateway => SMSGatewayEnum.Infini;
        /// <summary>
        /// Message For Absentees
        /// </summary>
        public string MessageForAbsentees => "Your ward is absent today";
        /// <summary>
        /// Student Id Prefix
        /// </summary>
        /// <param name="courseName"></param>
        public string CoursePrefix(string courseName)
        {
            // throw new NotImplementedException();
            return string.IsNullOrEmpty(courseName) ? "Z" : courseName[0].ToString().ToUpper();
        }

        /// <summary>
        /// Institute Course Order
        /// </summary>
        /// <returns></returns>
        public List<string> CourseOrder() => new List<string>() { "Playhome", "KG I", "KG II", "I", "II", "III", "IV", "V", "VI", "VII" };

        /// <summary>
        /// Print options
        /// </summary>
        /// <returns></returns>
        public PrintOptions GetPrintOptions()
        {
            PrintOptions po = null;
            po = new PrintOptions() { RecordsPerPage = 3 };
            return po;
        }

        /// <summary>
        /// Auto generate admission number
        /// </summary>
        public bool AutoGenerateAdmissionNo => false;

        /// <summary>
        /// Admission number prefix
        /// </summary>
        /// <returns></returns>
        public string AdmissionNoPrefix(string acadYearFrom, string acadYearTo)
        {
            return string.Empty;
        }

        /// <summary>
        /// Disability Condition
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> DisabilityCondition()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Speech condition
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> SpeechCondition()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Name call response
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> NameCallResponse()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Focus and attention on activity
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> ActivityFocusAttention()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Eye contact
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> EyeContact()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Sitting tolerance
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> SittingTolerance()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Instruction following
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> InstructionFollowing()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Body built
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> BodyBuilt()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Muscle tone
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> MuscleTone()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Proprioception
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> Proprioception()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Static balance
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> StaticBalance()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Dynamic balance
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> DynamicBalance()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Behavior
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> Behavior()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Focus and attention on topic / content
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> TopicFocusAttention()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Disability condition Speech and Language
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> DisabilityConditionSL()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Eye contact speech and language
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> EyeContactSL()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Sitting tolerance for speech and language
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> SittingToleranceSL()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Activity compliance
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> ActivityCompliance()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Receptive language
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> ReceptiveLanguage()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Expressive language
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> ExpressiveLanguage()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Social communication
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> SocialCommunication()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Pragmatic skills
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> PragmaticsSkills()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Intelligibility
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> Intelligibility()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Behavior speech and language
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> BehaviorSL()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Literacy
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> Literacy()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        /// <summary>
        /// Recommendation
        /// </summary>
        /// <returns></returns>
        public List<BaselineItem> Recommendation()
        {
            var result = new List<BaselineItem>();
            return result;
        }

        public List<BaselineItem> EyeContactSE()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Does not make eye contact" },
                new BaselineItem() { Id = 2, Name = "Makes occasional, unintentional eye contact for a very short duration (fraction of a second)" },
                new BaselineItem() { Id = 3, Name = "Makes brief eye contact (1-2 seconds) during close interactions or when directly addressed" },
                new BaselineItem() { Id = 4, Name = "Makes more frequent eye contact during conversations, but for short durations"},
                new BaselineItem() { Id = 5, Name = "Makes regular eye contact for moderate durations" },
                new BaselineItem() { Id = 6, Name = "Makes consistent eye contact throughout most conversations"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> ActivityComplianceSE()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Resists participating in the activity" },
                new BaselineItem() { Id = 2, Name = "Complies with the activity with minimal effort" },
                new BaselineItem() { Id = 3, Name = "Follows instructions without resistance but shows minimal initiative or enthusiasm"},
                new BaselineItem() { Id = 4, Name = "Begins to participate in the activity"},
                new BaselineItem() { Id = 5, Name = "Participates in the activity with moderate effort and demonstrates some interest"},
                new BaselineItem() { Id = 6, Name = "Engages actively in the activity and completes tasks with increasing independence"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> BehaviorsHinderingLearning()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Fidgeting: Restlessness and excessive movement" },
                new BaselineItem() { Id = 2, Name = "Daydreaming: Frequent zoning out can lead to missed information and incomplete work" },
                new BaselineItem() { Id = 3, Name = "Short Attention Span: Difficulty focusing for extended periods can affect learning new concepts" },
                new BaselineItem() { Id = 4, Name = "Task Avoidance: Procrastination or reluctance to start tasks" },
                new BaselineItem() { Id = 5, Name = "Hyperactivity and Impulsivity: Difficulty controlling impulses" },
                new BaselineItem() { Id = 6, Name = "Tantrums or Meltdowns: Emotional outbursts that escalate and take time to resolve" },
                new BaselineItem() { Id = 7, Name = "Chronic Inattention: Inability to focus for extended periods, even with prompts" },
                new BaselineItem() { Id = 8, Name = "Defiance and Non-Compliance: Refusal to follow instructions" },
                new BaselineItem() { Id = 9, Name = "Severe Anxiety: Anxiety that interferes with daily functioning and participation in class can hinder learning progress" },
                new BaselineItem() { Id = 99, Name = ""}
            };
            return result;
        }

        public List<BaselineItem> ActivityFocusAttentionSE()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Unable to maintain attention on any stimuli for even a brief moment" },
                new BaselineItem() { Id = 2, Name = "Briefly attends to preferred activities" },
                new BaselineItem() { Id = 3, Name = "Maintains attention for a few minutes on highly engaging activities" },
                new BaselineItem() { Id = 4, Name = "Focuses for 2-3 minutes with some prompting" },
                new BaselineItem() { Id = 5, Name = "Concentrates for 5-10 minutes on engaging tasks" },
                new BaselineItem() { Id = 6, Name = "Sustains attention for 10-15 minutes with familiar tasks" },
                new BaselineItem() { Id = 7, Name = "Maintains focus for 15-20 minutes on assigned activities" },
                new BaselineItem() { Id = 8, Name = "Concentrates for 20-30 minutes on a variety of tasks" },
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> InstructionsComprehension()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Does not understand spoken instructions" },
                new BaselineItem() { Id = 2, Name = "Understands very simple, one-step instructions with gestures or demonstrations" },
                new BaselineItem() { Id = 3, Name = "Understood familiar, one or two-step instructions with clear verbal cues" },
                new BaselineItem() { Id = 4, Name = "Comprehends simple, multi-step instructions (2-3 steps)" },
                new BaselineItem() { Id = 5, Name = "Understands and follows most multi-step instructions (3-5 steps)" },
                new BaselineItem() { Id = 6, Name = "Comprehends a variety of multi-step instructions (5+ steps)" },
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> Conditions()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Autism Spectrum Disorder(ASD)" },
                new BaselineItem() { Id = 2, Name = "ADHD (Attention Deficit Hyperactivity Disorder)" },
                new BaselineItem() { Id = 3, Name = "Learning disability (e.g., dyslexia, dysgraphia)" },
                new BaselineItem() { Id = 4, Name = "Intellectual disability"},
                new BaselineItem() { Id = 5, Name = "Emotional or behavioral disorder" },
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }
    }
}
