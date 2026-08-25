using ct._Core.Repo.Dto;
using ct._Core.Repo.Resource;
using System.Collections.Generic;

namespace ct._Core.Policy
{
    /// <summary>
    /// Default Policy Engine
    /// </summary>
    public class AKSHFNPolicyEngine : IPolicyEngine
    {
        /// <summary>
        /// No of sessions for which attendance is taken. Can be same as no of periods but not necessarily.
        /// </summary>
        public int AttendanceSessions => 2;
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
        public bool AutoGenerateAdmissionNo => true;

        /// <summary>
        /// Admission number prefix
        /// </summary>
        /// <returns></returns>
        public string AdmissionNoPrefix(string acadYearFrom, string acadYearTo)
        {
            var str = "AK" + "/" + acadYearFrom + "-" + acadYearTo.Substring(2, 2) + "/";
            return str;
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

        public List<BaselineItem> DisabilityCondition()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Autism Spectrum Disorder(ASD)" },
                new BaselineItem() { Id = 2, Name = "ADHD (Attention Deficit Hyperactivity Disorder)" },
                new BaselineItem() { Id = 3, Name = "Learning disability (e.g., dyslexia, dysgraphia)" },
                new BaselineItem() { Id = 4, Name = "Intellectual disability"},
                new BaselineItem() { Id = 5, Name = "Emotional or behavioral disorder" },
                new BaselineItem() { Id = 6, Name = "GDD (Global Developmental Delay)" },
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> SpeechCondition()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Verbal" },
                new BaselineItem() { Id = 2, Name = "Non Verbal" },
                new BaselineItem() { Id = 3, Name = "Semi Verbal" },
                new BaselineItem() { Id = 99, Name = "" } // Id = 99, Other option, input is given by user
            };
            return result;
        }

        public List<BaselineItem> NameCallResponse()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Good" },
                new BaselineItem() { Id = 2, Name = "Fair" },
                new BaselineItem() { Id = 3, Name = "Poor" }
            };
            return result;
        }

        public List<BaselineItem> ActivityFocusAttention()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "The child is able to focus throughout the completion of activity" },
                new BaselineItem() { Id = 2, Name = "The child is able to complete the activity but occasionally gets distracted" },
                new BaselineItem() { Id = 3, Name = "The child is able to complete the activity with frequent distractions" },
                new BaselineItem() { Id = 4, Name = "Not able to focus towards the activity"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> EyeContact()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Does not make eye contact" },
                new BaselineItem() { Id = 2, Name = "Makes eye contact for fraction of a second" },
                new BaselineItem() { Id = 3, Name = "The child is able to sustain eye contact for 5-10 counts with distractions" },
                new BaselineItem() { Id = 4, Name = "The child is able to sustain eye contact for 5-10 counts without distractions"},
                new BaselineItem() { Id = 5, Name = "The child is able to make eye contact for more than 10 counts" },
                new BaselineItem() { Id = 6, Name = "Makes consistent eye contact"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> SittingTolerance()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Often adapts 'W' sitting" },
                new BaselineItem() { Id = 2, Name = "Sits for short durations (1-2 minutes) with prompting" },
                new BaselineItem() { Id = 3, Name = "Sits comfortably for 5-10 minutes during various activities" },
                new BaselineItem() { Id = 4, Name = "Sits for 15-20 minutes for a variety of tasks"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> InstructionFollowing()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Does not understand spoken instructions" },
                new BaselineItem() { Id = 2, Name = "Understands very simple, one-step instructions" },
                new BaselineItem() { Id = 3, Name = "Understands 2 step instructions" },
                new BaselineItem() { Id = 4, Name = "Understands multiple instructions"},
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

        public List<BaselineItem> BodyBuilt()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Ectomorphic" },
                new BaselineItem() { Id = 2, Name = "Mesomorphic" },
                new BaselineItem() { Id = 3, Name = "Endomorphic" }
            };
            return result;
        }

        public List<BaselineItem> MuscleTone()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Hypotonic" },
                new BaselineItem() { Id = 2, Name = "Normal" },
                new BaselineItem() { Id = 3, Name = "Hypertonic" },
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> Proprioception()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Good" },
                new BaselineItem() { Id = 2, Name = "Fair" },
                new BaselineItem() { Id = 3, Name = "Poor" }
            };
            return result;
        }

        public List<BaselineItem> StaticBalance()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Good" },
                new BaselineItem() { Id = 2, Name = "Fair" },
                new BaselineItem() { Id = 3, Name = "Poor" }
            };
            return result;
        }

        public List<BaselineItem> DynamicBalance()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Good" },
                new BaselineItem() { Id = 2, Name = "Fair" },
                new BaselineItem() { Id = 3, Name = "Poor" }
            };
            return result;
        }

        public List<BaselineItem> Behavior()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Frequent tantrums or meltdowns" },
                new BaselineItem() { Id = 2, Name = "Difficulty managing emotions" },
                new BaselineItem() { Id = 3, Name = "Hyperactivity or inattentiveness" },
                new BaselineItem() { Id = 4, Name = "Self-stimulatory behaviors (rocking, flapping hands, twirling)" },
                new BaselineItem() { Id = 5, Name = "Repetitive behaviors" },
                new BaselineItem() { Id = 6, Name = "Stimming (Visual / Hand)" },
                new BaselineItem() { Id = 99, Name = ""}
            };
            return result;
        }

        public List<BaselineItem> TopicFocusAttention()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Child is very distractable and can't attend to what you say" },
                new BaselineItem() { Id = 2, Name = "Child can attend to own choice of activity for a longer period of time but cut self off from everything else" },
                new BaselineItem() { Id = 3, Name = "Child can listen if he/she stops activity and looks at others" },
                new BaselineItem() { Id = 4, Name = "Child looks automatically when adult speak and can shift attention from task to speaker" },
                new BaselineItem() { Id = 5, Name = "Child know longer needs to look up when adult speak and can listen at the same time as working or playing" },
                new BaselineItem() { Id = 6, Name = "Child listens and attens well in class and his integrated attention is well established" },
                new BaselineItem() { Id = 99, Name = ""}
            };
            return result;
        }

        public List<BaselineItem> DisabilityConditionSL()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Autism Spectrum Disorder(ASD)" },
                new BaselineItem() { Id = 2, Name = "ADHD (Attention Deficit Hyperactivity Disorder)" },
                new BaselineItem() { Id = 3, Name = "Learning disability (e.g., dyslexia, dysgraphia)" },
                new BaselineItem() { Id = 4, Name = "Intellectual disability"},
                new BaselineItem() { Id = 5, Name = "Emotional or behavioral disorder" },
                new BaselineItem() { Id = 6, Name = "Syndromes" },
                new BaselineItem() { Id = 7, Name = "Feeding Difficulty" },
                new BaselineItem() { Id = 8, Name = "Sensory Issues" },
                new BaselineItem() { Id = 9, Name = "GDD" },
                new BaselineItem() { Id = 10, Name = "Speech and language delay" },
                new BaselineItem() { Id = 11, Name = "Speech sound disorder" },
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> EyeContactSL()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Does not make eye contact" },
                new BaselineItem() { Id = 2, Name = "Makes occasional, unintentional eye contact for a very short duration (fraction of a second)" },
                new BaselineItem() { Id = 3, Name = "Child may occasionally glance at others faces or may make peripheral eye contact( looking at someone face but not directly into their eyes)" },
                new BaselineItem() { Id = 4, Name = "Makes more frequent eye contact during conversations, but for short durations"},
                new BaselineItem() { Id = 5, Name = "Child makes eye contact appropriately in social situation to convey interest, gain attention or participate in reciprocal communication" },
                new BaselineItem() { Id = 6, Name = "Makes reciprocative and consistent eye contact throughout most conversations"},
                new BaselineItem() { Id = 99, Name = "" }
            };
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

        public List<BaselineItem> SittingToleranceSL()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Unable to maintain a seated position for even a brief moment" },
                new BaselineItem() { Id = 2, Name = "Briefly tolerates sitting for a few seconds, especially during preferred activities" },
                new BaselineItem() { Id = 3, Name = "Sits for short durations (1-2 minutes) with prompting" },
                new BaselineItem() { Id = 4, Name = "Manages to sit for 3-5 minutes"},
                new BaselineItem() { Id = 5, Name = "Sits for 5-10 minutes"},
                new BaselineItem() { Id = 6, Name = "Sits comfortably for 10-15 minutes during various activities"},
                new BaselineItem() { Id = 7, Name = "Sits for 15-20 minutes for a variety of tasks"},
                new BaselineItem() { Id = 8, Name = "Sits comfortably for 20-30 minutes"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> ActivityCompliance()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Resists participating in the activity" },
                new BaselineItem() { Id = 2, Name = "Complies with activity after being instructed 2 or 3 times" },
                new BaselineItem() { Id = 3, Name = "Complies with the activity with minimal effort" },
                new BaselineItem() { Id = 4, Name = "Follows instructions without resistance but shows minimal initiative or enthusiasm"},
                new BaselineItem() { Id = 5, Name = "Begins to participate in the activity"},
                new BaselineItem() { Id = 6, Name = "Participates in the activity with moderate effort and demonstrates some interest"},
                new BaselineItem() { Id = 7, Name = "Engages actively in the activity and completes tasks with increasing independence without any prompt"},
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

        public List<BaselineItem> ReceptiveLanguage()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Child responds to their name when called" },
                new BaselineItem() { Id = 2, Name = "Child understand basic vocalbulary words or common words" },
                new BaselineItem() { Id = 3, Name = "Understand different lexical categories like fruits, vegetables, animals etc" },
                new BaselineItem() { Id = 4, Name = "Child follows 1-2 step instruction"},
                new BaselineItem() { Id = 5, Name = "Child understand colour concept"},
                new BaselineItem() { Id = 6, Name = "Child understand shapes"},
                new BaselineItem() { Id = 7, Name = "Child understand same or different"},
                new BaselineItem() { Id = 8, Name = "Understand time concept( yesterday, today and tomorrow)"},
                new BaselineItem() { Id = 9, Name = "Understand \"wh\" questions"},
                new BaselineItem() { Id = 10, Name = "Comprehend special perpositions"},
                new BaselineItem() { Id = 11, Name = "Understand complex instruction"},
                new BaselineItem() { Id = 12, Name = "Understand no and not"},
                new BaselineItem() { Id = 13, Name = "Understand opposites"},
                new BaselineItem() { Id = 14, Name = "Understand verbs"},
                new BaselineItem() { Id = 15, Name = "Understand adjectives"},
                new BaselineItem() { Id = 16, Name = "Understand sequencing of events"},
                new BaselineItem() { Id = 17, Name = "Understand size concept"},
                new BaselineItem() { Id = 18, Name = "Understand use of objects"},
                new BaselineItem() { Id = 19, Name = "Understand emotions"},
                new BaselineItem() { Id = 20, Name = "Understand family members"},
                new BaselineItem() { Id = 21, Name = "Understand genders"},
                new BaselineItem() { Id = 22, Name = "Understand plurals"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> ExpressiveLanguage()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Child uses gestures to communicate" },
                new BaselineItem() { Id = 2, Name = "Child uses single words to communicate" },
                new BaselineItem() { Id = 3, Name = "Child combines two or more words to form sentences" },
                new BaselineItem() { Id = 4, Name = "Names familiar objects and pictures"},
                new BaselineItem() { Id = 5, Name = "Names family members"},
                new BaselineItem() { Id = 6, Name = "Tells stories that stick to the topic"},
                new BaselineItem() { Id = 7, Name = "Expresses different lexical categories like fruits, vegetables, animals,etc"},
                new BaselineItem() { Id = 8, Name = "Expresses emotions"},
                new BaselineItem() { Id = 9, Name = "Answer \"wh\" questions"},
                new BaselineItem() { Id = 10, Name = "Gives directions"},
                new BaselineItem() { Id = 11, Name = "Answer opposites"},
                new BaselineItem() { Id = 12, Name = "Uses verbs"},
                new BaselineItem() { Id = 13, Name = "Uses prepositions"},
                new BaselineItem() { Id = 14, Name = "Expresses adjectives"},
                new BaselineItem() { Id = 15, Name = "Uses \"no\" and \"not \""},
                new BaselineItem() { Id = 16, Name = "Engages in conversation at range of topics"},
                new BaselineItem() { Id = 17, Name = "Able to communicate basic personal needs"},
                new BaselineItem() { Id = 18, Name = "Names colours"},
                new BaselineItem() { Id = 19, Name = "Names shapes"},
                new BaselineItem() { Id = 20, Name = "Names gender"},
                new BaselineItem() { Id = 21, Name = "Ask for permission"},
                new BaselineItem() { Id = 22, Name = "Expresses plurals"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> SocialCommunication()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Makes eye contact and facial expression during interactions" },
                new BaselineItem() { Id = 2, Name = "Engages in simple social game or turn taking activities" },
                new BaselineItem() { Id = 3, Name = "Shows interest in interacting with peers and adult" },
                new BaselineItem() { Id = 4, Name = "Engages in pretend play"},
                new BaselineItem() { Id = 5, Name = "Understand jokes and simple humour"},
                new BaselineItem() { Id = 6, Name = "Says please and thank you"},
                new BaselineItem() { Id = 7, Name = "Plays rule based games"},
                new BaselineItem() { Id = 8, Name = "Has social skills like emapty and compassion"},
                new BaselineItem() { Id = 9, Name = "Handle emotions like angry, fear and rejections"},
                new BaselineItem() { Id = 10, Name = "Understand social norms, personal space and etiquette in various context"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> PragmaticsSkills()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Uses appropriate greetings and farewells" },
                new BaselineItem() { Id = 2, Name = "Maintain a topic of conversation for a short period of time" },
                new BaselineItem() { Id = 3, Name = "Adjust their language and communication style based on the listerns responses and reactions" },
                new BaselineItem() { Id = 4, Name = "Understand and response to simple social clues and requests"},
                new BaselineItem() { Id = 5, Name = "Corrects others"},
                new BaselineItem() { Id = 6, Name = "Response appropriately to compliments"},
                new BaselineItem() { Id = 7, Name = "Apologises and response to apology appropriately"},
                new BaselineItem() { Id = 8, Name = "Considered others feeling and thoughts"},
                new BaselineItem() { Id = 9, Name = "Has conflict resolution skills"},
                new BaselineItem() { Id = 10, Name = "Understand non verbal clues"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> Intelligibility()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Normal" },
                new BaselineItem() { Id = 2, Name = "Can be understood without difficulty, however feel the speech is not normal" },
                new BaselineItem() { Id = 3, Name = "Can be understood with a little effort but occasionally need to ask for repetition" },
                new BaselineItem() { Id = 4, Name = "Can be understood with concentration and effort especially by sympathetic listener"},
                new BaselineItem() { Id = 5, Name = "Can be understood with concentration and difficulty by family members but not by others"},
                new BaselineItem() { Id = 6, Name = "Can be understood with effort if content is known"},
                new BaselineItem() { Id = 7, Name = "Cannot be understood at all even when the content is known"},
                new BaselineItem() { Id = 99, Name = "" }
            };
            return result;
        }

        public List<BaselineItem> BehaviorSL()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Frequent tantrums or meltdowns" },
                new BaselineItem() { Id = 2, Name = "Difficulty managing emotions" },
                new BaselineItem() { Id = 3, Name = "Hyperactivity or inattentiveness" },
                new BaselineItem() { Id = 4, Name = "Self-stimulatory behaviors (rocking, flapping hands, twirling)" },
                new BaselineItem() { Id = 5, Name = "Repetitive behaviors" },
                new BaselineItem() { Id = 6, Name = "Engages actively in the activity and completes tasks with increasing independence" },
                new BaselineItem() { Id = 99, Name = ""}
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

        public List<BaselineItem> Literacy()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "No Recognition of letters" },
                new BaselineItem() { Id = 2, Name = "Recognizes familiar letters" },
                new BaselineItem() { Id = 3, Name = "Recognizes most letters" },
                new BaselineItem() { Id = 4, Name = "Reads sight words" },
                new BaselineItem() { Id = 5, Name = "Decodes simple words" },
                new BaselineItem() { Id = 6, Name = "Reads grammatically correct sentences" },
                new BaselineItem() { Id = 7, Name = "Understand basic punctuation and sentence structure"},
                new BaselineItem() { Id = 8, Name = "Understands Different Word Meanings"},
                new BaselineItem() { Id = 9, Name = "Writes/Reads Grammatically Correct Sentences and Paragraphs"},
                new BaselineItem() { Id = 99, Name = ""}
            };
            return result;
        }

        public List<BaselineItem> Recommendation()
        {
            var result = new List<BaselineItem>()
            {
                new BaselineItem() { Id = 1, Name = "Speech and Language Therapy" },
                new BaselineItem() { Id = 2, Name = "PT/OT Therapy" },
                new BaselineItem() { Id = 3, Name = "Special Education" },
                new BaselineItem() { Id = 4, Name = "Behavioural Therapy" },
                new BaselineItem() { Id = 5, Name = "Audiological Evaluation" },
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


    }
}
