using ct._Core.Repo.Dto;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Repo.Db.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using ct._Core.Policy;

namespace ct.stu.tests.EmailSppu
{
    [TestClass]
    public class EmailSppuTests : TestBase
    {

        List<string> emails_staff = new List<string>() { "vvssardarpatelpucollege@yahoo.com" };
        List<string> emails_test = new List<string>() { "vishal@bitloka.com", "abhay@bitloka.com", "nivedha@bitloka.com"};

        [ClassInitialize]
        public static void class_init(TestContext context)
        {
            Configure();
            ConfigureStore();
        }

        [TestMethod]
        [Ignore]
        public void sppu_190122_practical_exam_tt_II_pu()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "sppu@vidyavardhakasangha.com";

            var subject = "VVS SPPU: II PU Annual Lab Exam batch list and timetable for Physics and Chemistry";

            var baseDir = @"EmailSppu/sppu_190122_practical_exam_tt_pu_II/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Batch List and Timetable II PU Practical Examination.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
               rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_181220_prep_tt_pu_I()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "sppu@vidyavardhakasangha.com";

            var subject = "VVS SPPU: I PU Annual Lab Exam Dates and Preparatory Exam Schedule";

            var baseDir = @"EmailSppu/sppu_181220_prep_tt_pu_I/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Prep_tt_I_PU.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc1_pcmb,
                id_vvs_sppu_18_19_puc1_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
               rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_181204_prepIII_tt_pu_II()
        {
            var instId = id_vvs_sppu_18_19;

            var userId = "";

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: III Preparatory Time Table";

            var baseDir = @"EmailSppu/sppu_181204_prepIII_tt_pu_II/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Prep_III_tt_II_PU.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var id = Guid.NewGuid().ToString();
                var job = new Job() { id = id, title = "email", payload = email_obj, user_id = userId, client_id = instId };
                var rmq = new RabbitMQService(Config);
                // rmq.EnqueEmail(job);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_181105_prepII_tt_pu_II()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: II Preparatory Time Table";

            var baseDir = @"EmailSppu/sppu_181105_prepII_tt_pu_II/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Prep_II_tt_II_PU.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
               // rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_181105_prepI_tt_pu_II()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: I Preparatory Time Table";

            var baseDir = @"EmailSppu/sppu_181105_prepI_tt_pu_II/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Prep_I_tt_II_PU.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
               // rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180918_midterm_tt_pu_II()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: Mid Term Exam Time Table";

            var baseDir = @"EmailSppu/sppu_180918_midterm_tt_pu_II/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "midterm_tt_II_PU.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
               // rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180918_midterm_tt_pu_I()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: Mid Term Exam Time Table";

            var baseDir = @"EmailSppu/sppu_180918_midterm_tt_pu_I/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "midterm_tt_I_PU.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc1_pcmb,
                id_vvs_sppu_18_19_puc1_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
              //  rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180813_quarterly_tt_pu_II()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: Quarterly Exam Time Table";

            var baseDir = @"EmailSppu/sppu_180813_quarterly_tt_pu_II/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "quarterly_tt_II_PU.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
              //  rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180813_quarterly_tt_pu_I()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: Quarterly Exam Time Table";

            var baseDir = @"EmailSppu/sppu_180813_quarterly_tt_pu_I/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "quarterly_tt_I_PU.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc1_pcmb,
                id_vvs_sppu_18_19_puc1_pcme
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
              //  rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180803_tt()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: Master Time Table";

            var baseDir = @"EmailSppu/sppu_180803_tt/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Master-Timetable.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc1_pcmb,
                id_vvs_sppu_18_19_puc1_pcme,
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
              //  rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180730_tt()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: Time Table from 30th July 2018 to 04th August 2018";

            var baseDir = @"EmailSppu/sppu_180730_tt/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Time table from 30-7-18 to 04-08-18.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc1_pcmb,
                id_vvs_sppu_18_19_puc1_pcme,
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
              //  rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180705_tt_pu1()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: PUC I Timetable for Monthly Test (July)";

            var baseDir = @"EmailSppu/sppu_180705_tt/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "sppu-monthly-test-july-puc-1.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc1_pcmb,
                id_vvs_sppu_18_19_puc1_pcme,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
              //  rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180705_tt_pu2()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: PUC II Timetable for Monthly Test (July)";

            var baseDir = @"EmailSppu/sppu_180705_tt/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "sppu-monthly-test-july-puc-2.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
              //  rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void sppu_180623_parent_mentor_meet()
        {
            var instId = id_vvs_sppu_18_19;

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "VVS SPPU: Parent-mentor meet will be held on 23rd June 2018";

            var baseDir = @"EmailSppu/sppu_180623_parent_mentor_meet/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));


            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_sppu_18_19_puc2_pcmb,
                id_vvs_sppu_18_19_puc2_pcme,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            emails_staff = send_to_all ? emails_staff.Distinct().ToList() : new List<string>();
            var emails = emails_parents
                            .Concat(emails_staff)
                            .Concat(emails_test)
                            .Chunk(128);

            foreach (var email_chunk in emails)
            {
                Console.WriteLine(string.Join(",", email_chunk));
                var email_obj = new EmailJobDto()
                {
                    subject = subject,
                    from = fromAddress,
                    to = email_chunk.ToList(),
                    html = message
                };
                var rmq = new RabbitMQService(Config);
              //  rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        public void can_get_emails()
        {
            var inst_id = id_vvs_sppu_18_19;
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var qry = new StudentQry(uc, rmq);
                var stus = qry.GetAll().Where(o => o.CourseName.StartsWith("PUC II")).Select(o => new { o.Name, o.Emails }).ToList();
                Console.WriteLine(stus.Count);
                foreach (var stu in stus.OrderBy(o => o.Name))
                {
                    Console.WriteLine($"{stu.Name}\t{stu.Emails}");
                }
                //File.WriteAllText(Path.Combine(Path.GetTempPath(), "sppu-stu-emails.txt"), JsonConvert.SerializeObject(stus));
            }
        }

        public List<string> get_recipients(string instId, List<string> limitCoursesTo)
        {
            var inst_id = instId;
            //ToDo
            var uc = new UserContext(null)
            {
                InstId = inst_id
            };

            var emails = new List<string>();
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var qry = new StudentQry(uc, rmq, pep);
                var parentEmails = qry.GetParentEmails(validOnly: true, activeOnly: true, limitToCourses: limitCoursesTo)
                                        .Distinct()
                                        .ToList();
                Console.WriteLine("Parent emails: " + parentEmails.Count);

               // var courses = qry.GetCourses(limitCoursesTo).Select(o => o.Name).ToList();
                var inst = RedisCache.GetClientDto(inst_id);

                //TBD:
                //var setToRemove = new HashSet<string>(inst.BlockedContacts.Where(o => o.Contains("@")));
                //parentEmails.RemoveAll(x => setToRemove.Contains(x));
                emails = parentEmails;
                emails.Sort();
                emails = emails.GroupBy(x => x).Select(g => g.First()).ToList(); //remove duplicates

                //TBD:
                //Console.WriteLine("Emails to send (excl blocked/duplicates): " + emails.Count);

               // Console.WriteLine(string.Format("{0} - {1}", inst.Name, string.Join(",", courses)));

            }

            return emails;
        }
    }
}
