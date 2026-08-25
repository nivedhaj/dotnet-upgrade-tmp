using Autofac;
using ct._Core.Policy;
using ct._Core.Repo.Dto;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repo.Db.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ct.stu.tests.Email
{
    [TestClass]
    public class EmailTests : TestBase
    {
        string id_vvs_gcs_ph = "1a6a3020-1819-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_kg1 = "1a6a3021-2627-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_kg2 = "1a6a3022-2627-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_std1 = "1a6a3023-1819-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_std2 = "1a6a3024-1819-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_std3 = "1a6a3025-1819-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_std4 = "1a6a3026-1819-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_std5 = "1a6a3027-1819-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_std6 = "1a6a3028-1819-45ec-9500-f37f54f82d5f";
        string id_vvs_gcs_std7 = "1a6a3029-1819-45ec-9500-f37f54f82d5f";
        List<string> emails_staff_kg = new List<string>() { "vvsgc.nursery@gmail.com" };
        List<string> emails_staff_ph_to_7 = new List<string>() { "vvsgcsph@gmail.com", "vvsgc_nursery@yahoo.com", "vvsgcslp@gmail.com", "vvs_gcs@yahoo.co.in", "vengita302@gmail.com" };
        List<string> emails_staff_1234 = new List<string>() { "vvsgcslp@gmail.com", "vengita302@gmail.com" };
        List<string> emails_staff_567 = new List<string>() { "vvs_gcs@yahoo.co.in", "vengita302@gmail.com" };
        List<string> emails_test = new List<string>() { "nivedha@bitloka.com" };

        [ClassInitialize]
        public static void class_init(TestContext context)
        {
            Configure();
            ConfigureStore();
        }

        [TestMethod]
        [Ignore]
        public void gcs_190124_School_Timings_V_to_VII()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School Timings on Friday";

            var baseDir = @"Email/190124_School_Timings_V_to_VII/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190121_Holiday_Declaration()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Declaration of Holiday";

            var baseDir = @"Email/190121_Holiday_Declaration/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_ph,
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_ph_to_7.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190118_Instructions_To_Parents()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Instructions to parents";

            var baseDir = @"Email/190118_Instructions_To_Parents/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190117_Golden_Jubilee_Celebrations_V_to_VII()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Golden Jubilee Celebration Details";

            var baseDir = @"Email/190117_Golden_Jubilee_Celebrations_V_to_VII/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190117_Golden_Jubilee_Celebrations_Nursery()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Holiday Declaration";

            var baseDir = @"Email/190117_Golden_Jubilee_Celebrations_Nursery/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                // id_vvs_gcs_kg1,
                // id_vvs_gcs_kg2
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190117_Golden_Jubilee_Celebrations_III_IV()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Golden Jubilee Celebration Details";

            var baseDir = @"Email/190117_Golden_Jubilee_Celebrations_III_IV/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_std3,
                id_vvs_gcs_std4
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190116_II_Semester_II_Unit_Test_Timetable_HP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: II Semester II Unit Test Time Table ( Std V to VII )";

            var baseDir = @"Email/190116_II_Semester_II_Unit_Test_HP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "II Semester II Unit Test Timetable.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_std5,id_vvs_gcs_std6,id_vvs_gcs_std7
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
        public void gcs_090726_kg2_june_programme_sheet()
        {
            var instId = id_vvs_gcs_26_27;

            var send_to_all = true;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Programme Sheet - June 2026-27 - KG II";

            //var baseDir = @"Email/090726_KG2_June_Programme_Sheet/";
            var baseDir = Path.Combine(Path.GetTempPath(), "090726_KG2_June_Programme_Sheet");
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "June-Programme Sheet-26-27-KG2.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_kg2
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
        public void gcs_190114_II_Semester_II_Unit_Test_Timetable_LP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: II Semester II Unit Test Time Table ( Std I to IV )";

            var baseDir = @"Email/190114_II_Semester_II_Unit_Test_LP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "II Semester II Unit Test 2018-19.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_std1,id_vvs_gcs_std2,id_vvs_gcs_std3,id_vvs_gcs_std4,
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
        public void gcs_190109_School_Timings_I_to_VII()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School Timings on Saturday";

            var baseDir = @"Email/190109_School_Timings_I_to_VII/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190109_School_Timings_Nursery()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School Timings on Saturday";

            var baseDir = @"Email/190109_School_Timings_Nursery/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
            {
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190107_Holiday_Declaration()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Holiday Declaration";

            var baseDir = @"Email/190107_Holiday_Declaration/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_ph,
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_ph_to_7.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_190104_KG_I_Programme_Dec_Jan()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: December and January month Programme sheet of K.G I";

            var baseDir = @"Email/190104_KG_I_Programme_Dec/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "KG I December and January Programme Sheet.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
        public void gcs_181221_Nur_7()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Holiday declaration";

            var baseDir = @"Email/181221_Nur_7/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_ph,
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_ph_to_7.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181219_Nur_Sports_Day()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Nursery Sports Day";

            var baseDir = @"Email/181219_Nur_Sports_Day/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181218_Winter_Vacation()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Winter Vacations";

            var baseDir = @"Email/181218_Winter_Vacation/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                rmq.EnqueEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181210_KG_II_Programme_Sep()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG II Programme Sheet - Sep-Oct-Nov 2018";

            var baseDir = @"Email/181210_KG_II_Programme_Sep/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Programme_Sheet_Sept_Nov_KGII.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg2,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void test_email_without_attachment()
        {
            var instId = id_btl_col_18_19;
            var senderId = "BITLOK";

            var userId = "";

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "Test Email";

            var baseDir = @"Email/181204_Test_Email/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
            {
            };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                    html = message,
                    sender_id = senderId
                };
                var id = Guid.NewGuid().ToString();
                var job = new Job() { id = id, title = "email", payload = email_obj, user_id = userId, client_id = instId };
                var rmq = new RabbitMQService(Config);
                // rmq.PushEmail(job);
            }
        }

        [TestMethod]
        [Ignore]
        public void test_email_with_attachment()
        {
            var instId = id_btl_col_18_19;
            var senderId = "BITLOK";

            var userId = "";

            var send_to_all = false;

            var fromAddress = "support@campustrack.net";

            var subject = "Test Email";

            var baseDir = @"Email/181206_Test_Email_Attachment/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Test_Email_Attachment.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>() { };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            // File.WriteAllText(Path.Combine(Path.GetTempPath(), "3.html"), string.Join(",",emails_parents));
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                    html = message,
                    sender_id = senderId,
                    inst_id = instId
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
                // rmq.PushEmail(job);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181129_II_Semester_I_Unit_Test_Timetable_HP()
        {
            var instId = id_vvs_gcs_18_19;
            var senderId = "VVSGCS";

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: II Semester I Unit Test Time Table ( Std V to VII )";

            var baseDir = @"Email/181129_II_Semester_I_Unit_Test_Timetable_HP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "181129_II_Semester_I_Unit_Test_Timetable_HP.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,id_vvs_gcs_std6,id_vvs_gcs_std7,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                    html = message,
                    sender_id = senderId,
                    inst_id = instId
                };
                email_obj.attachments.Add(new EmailAttachmentDto
                {
                    file_path = pdfFile,
                    mime_type = "application",
                    mime_sub_type = "pdf",
                    data = pdf_data
                });
                var rmq = new RabbitMQService(Config);
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181129_II_Semester_I_Unit_Test_Timetable_LP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: II Semester I Unit Test Time Table ( Std I to IV )";

            var baseDir = @"Email/181129_II_Semester_I_Unit_Test_Timetable_LP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "181129_II_Semester_I_Unit_Test_Timetable_LP.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,id_vvs_gcs_std2,id_vvs_gcs_std3,id_vvs_gcs_std4,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            // File.WriteAllText(Path.Combine(Path.GetTempPath(), "3.html"), string.Join(",",emails_parents));
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                //rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180918_KG_I_Programme_Nov()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = true;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG I Programme Sheet & Portion - October-November 2018";

            var baseDir = @"Email/181121_KG_I_Programme_Nov/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "K.G. I  Oct - Nov programme sheet and portion.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181115_CD_Timings_Nursery()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Children's day timings for nursery";

            var baseDir = @"Email/181115_CD_Timings_Nursery/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181115_CD_Timings_HP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Children's Day Celebrations";

            var baseDir = @"Email/181115_CD_Timings_HP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181115_CD_Timings_LP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Children's Day Celebration 2018-19";

            var baseDir = @"Email/181115_CD_Timings_LP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181113_Childrens_Day()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Note on Children's Day Celebration";

            var baseDir = @"Email/181113_Childrens_Day/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_ph,
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Concat(emails_staff_kg).Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181029_School_Fee()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Information about school fee update";

            var baseDir = @"Email/181029_School_Fee/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_ph,
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Concat(emails_staff_kg).Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181025_II_PT_Meeting()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Parent - Teacher meet";

            var baseDir = @"Email/181025_II_PT_Meeting/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181004_School_Timings_Friday_HP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings on Friday";

            var baseDir = @"Email/181004_School_Timings_Friday_HP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181003_School_Timings_HP_Correction()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings for 6th Oct. Saturday";

            var baseDir = @"Email/181003_School_Timings_HP_Correction/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181003_School_Timings_HP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School Timings on 3rd, 4th & 6th";

            var baseDir = @"Email/181003_School_Timings_HP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181001_Annual_Sports_HP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Annual Sports Day 2018";

            var baseDir = @"Email/181001_Annual_Sports_HP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_181001_Annual_Sports_LP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Annual Sports 2018 - 19";

            var baseDir = @"Email/181001_Annual_Sports_LP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180918_KG_II_Appraisal_I_TT()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG II First Term Appraisal Time Table 2018";

            var baseDir = @"Email/180918_KG_II_Appraisal_I_TT/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "KGII_Appraisal_I_TT.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg2,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180918_KG_I_Appraisal_I_TT()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG I First Term Appraisal Time Table 2018";

            var baseDir = @"Email/180918_KG_I_Appraisal_I_TT/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "KGI_Appraisal_I_TT.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180918_KG_I_Programme_Aug_Sep()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG I Programme Sheet - August-September 2018";

            var baseDir = @"Email/180918_KG_I_Programme_Aug_Sep/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "KGI-August-September-Programme-Sheet.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180917_I_Semester_Exam_TimeTable_HP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: I Semester Exam Time Table ( Std V to VII )";

            var baseDir = @"Email/180917_I_Semester_Exam_Timetable_HP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "180917_I_Semester_Exam_Timetable_HP.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,id_vvs_gcs_std6,id_vvs_gcs_std7,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180917_I_Semester_Exam_Timetable_LP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: I Semester Exam Time Table September - 2018 ( Std I to IV )";

            var baseDir = @"Email/180917_I_Semester_Exam_Timetable_LP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "180917_I_Semester_Exam_Timetable_LP.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,id_vvs_gcs_std2,id_vvs_gcs_std3,id_vvs_gcs_std4,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180917_Moharrum()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Moharrum Holiday";

            var baseDir = @"Email/180917_Moharrum/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Concat(emails_staff_kg).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180911_5to7_Timings()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings on Saturday, 15-9-2018 for the students of std 5 to 7";

            var baseDir = @"Email/180911_5to7_Timings/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180911_KG_II_GK_Sheet_First_Term()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG II GK Sheet For First Term 2018-19";

            var baseDir = @"Email/180911_KG_II_GK_Sheet_First_Term/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "KG-II-GK-Sheet-First-Term.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg2,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180911_KG_II_Programme_Aug()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG II Programme Sheet - August 2018";

            var baseDir = @"Email/180911_KG_II_Programme_Aug/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "August-PS-KGII-18-19.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg2,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        // [Ignore]
        public void gcs_180829_Teachers_Day()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings on 5-9-2018";

            var baseDir = @"Email/180829_Teachers_Day/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180829_Diamond_Jubilee()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings on 3-9-2018";

            var baseDir = @"email/180829_Diamond_Jubilee/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180820_KG_I_Programme_Aug()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG I Programme Sheet and Portion - August 2018";

            var baseDir = @"email/180820_KG_I_Programme_Aug/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "KGI-August-Programme-Sheet-Portion.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180813_Whatsapp()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Note to the parents";

            var baseDir = @"email/180813_Whatsapp/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180808_DPT()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Circular regarding DPT vaccine";

            var baseDir = @"email/180808_DPT/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg2,
                id_vvs_gcs_std1,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Concat(emails_staff_kg).Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180808_Deworming()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: De-worming Programme";

            var baseDir = @"email/180808_Deworming/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Concat(emails_staff_kg).Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180806_KG_II_Programme_Jul()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG II Programme Sheet and Story Sheet - July 2018";

            var baseDir = @"email/180806_KG_II_Programme_Jul/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "JULY - Programme Sheet-18-19.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg2,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180803_I_Sem_II_Unit_Test_TimeTable_HP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = true;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: I Semester II Unit Test Time Table ( Std V to VII )";

            var baseDir = @"email/180803_567_I_SemII_Unit_Test_Timetable/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "I-Sem-II-Unit-Test-HP.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,id_vvs_gcs_std6,id_vvs_gcs_std7,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180802_I_Sem_II_Unit_Test_TimeTable_LP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: I Semester II Unit Test Time Table August - 2018 ( Std I to IV )";

            var baseDir = @"email/180802_I_Sem_II_Unit_Test_TimeTable_LP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "I Semester II Unit Test 2018-19.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,id_vvs_gcs_std2,id_vvs_gcs_std3,id_vvs_gcs_std4,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                // rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180730_1to7_Timings()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings on 31.7.2018 for the students of std 1 to 7";

            var baseDir = @"email/180730_1to7_Timings/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }


        [TestMethod]
        [Ignore]
        public void gcs_180726_KG12_Shlokas()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Shloka Sheet";

            var baseDir = @"email/180726_KG12_Shlokas/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Shlokas.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                id_vvs_gcs_kg2,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180725_1to7_PT()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: I Parents' meet";

            var baseDir = @"email/180725_1to7_PT/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180725_67_Inspire()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: INSPIRE Award by Department of Science and Technology";

            var baseDir = @"email/180725_67_Inspire/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180718_KG_I_Programme_Jul()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: July month Programme sheet and Portion of K.G I 2018";

            var baseDir = @"email/180718_KG_I_Programme_Jul/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "K.G I  July month programme sheet and  portions.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        public void gcs_180713_1to7_Scholarship()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Scholarship to students";

            var baseDir = @"email/180713_1to7_Scholarship/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180711_School_timings_hp_on_14_Jul_2018()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings on 14-7-2018";

            var baseDir = @"email/180711_School_timings_hp_on_14_Jul_2018/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180711_School_timings_on_14_Jul_2018()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings on 14-7-2018";

            var baseDir = @"email/180711_School_timings_on_14_Jul_2018/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180711_KG_II_Story_Of_the_Month()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Story for the month of June - KG II";

            var baseDir = @"email/180711_KG_II_Story_Of_the_Month/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Shepherd Boys and the Fox.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg2,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180706_KG_II_Programme_June()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: KG II Programme Sheet - June 2018";

            var baseDir = @"email/180706_KG_II_Programme_June/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "JUNE 2018-KG II.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg2,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_kg.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180702_67_Scholarship()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School:  Department of Posts Scholarship (Std VI and VII)";

            var baseDir = @"email/180702_67_Scholarship/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "DepartmentOfPosts-Scholarship.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std6,id_vvs_gcs_std7,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180627_Talent_Search()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Talent-Search programme (Std I to IV)";

            var baseDir = @"email/180627_Talent_Search/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Talent Search Programme - 2018-19.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,id_vvs_gcs_std2,id_vvs_gcs_std3,id_vvs_gcs_std4,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180627_Guidelines_To_Parents()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: Guidelines to Parents (Std I to IV)";

            var baseDir = @"email/180627_Guidelines_To_Parents/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "Guidelines to parents 2018 - 19.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,id_vvs_gcs_std2,id_vvs_gcs_std3,id_vvs_gcs_std4,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180626_I_SemI_Unit_Test_TimeTable_HP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: I Semester I Unit Test Time Table July - 2018 ( Std V to VII )";

            var baseDir = @"email/180626_I_SemI_Unit_Test_TimeTable_HP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "180626_I_SemI_Unit_Test_TimeTable_HP.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std5,id_vvs_gcs_std6,id_vvs_gcs_std7
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_567.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180626_I_SemI_Unit_Test_TimeTable_LP()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = true;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: I Semester I Unit Test Time Table July - 2018 ( Std I to IV )";

            var baseDir = @"email/180626_I_SemI_Unit_Test_TimeTable_LP/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "180626_I_SemI_Unit_Test_TimeTable_LP.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,id_vvs_gcs_std2,id_vvs_gcs_std3,id_vvs_gcs_std4,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180625_KG_I_Programme_June()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: June month Programme sheet and Portion of K.G I 2018";

            var baseDir = @"email/180625_KG_I_Programme_June/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            var pdfFile = "K.G.I  June month programme sheet and portion.pdf";
            var pdf_bytes = File.ReadAllBytes(Path.Combine(baseDir, pdfFile));
            var pdf_data = Convert.ToBase64String(pdf_bytes);

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_kg1,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                //  rmq.PushEmail(email_obj);
            }
        }

        [TestMethod]
        [Ignore]
        public void gcs_180618_School_timings_on_20_6_2018()
        {
            var instId = id_vvs_gcs_18_19;

            var send_to_all = false;

            var fromAddress = "no-reply@vvsgcs.org";

            var subject = "VVSGC School: School timings on 20-6-2018, Wednesday";

            var baseDir = @"email/180618_School_timings_on_20_6_2018/";
            var htmlFile = "content.html";
            var message = File.ReadAllText(Path.Combine(baseDir, htmlFile));

            //get recipients
            var limitCoursesTo = new List<string>()
                {
                id_vvs_gcs_std1,
                id_vvs_gcs_std2,
                id_vvs_gcs_std3,
                id_vvs_gcs_std4,
                id_vvs_gcs_std5,
                id_vvs_gcs_std6,
                id_vvs_gcs_std7,
                };

            List<string> emails_parents = get_recipients(instId, limitCoursesTo);
            emails_parents = send_to_all ? emails_parents : new List<string>();
            var emails_staff = send_to_all ? emails_staff_1234.Concat(emails_staff_567).Distinct().ToList() : new List<string>();
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
                 // rmq.PushEmail(email_obj);
            }
        }


        [TestMethod]
        public void can_export_parent_emails()
        {
            var inst_id = id_vvs_sppu_18_19;
            var short_name = "VVSSPU";
            var uc = new UserContext(null) { InstId = inst_id, ShortName = short_name };
            //get recipients
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var qry = new StudentQry(uc, rmq);
                var parent_emails = qry.GetParentEmails();
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "sppu_1819_parent_emails.txt"), JsonHelper.Serialize(parent_emails));
            }
        }

        List<string> get_recipients(string instId, List<string> limitCoursesTo, string shortName = "")
        {
            var inst_id = instId;
            //ToDo
            var uc = new UserContext(null)
            {
                InstId = inst_id,
                ShortName = shortName
            };

            var emails = new List<string>();
            using (var session = Store.OpenSession())
            {
                uc.Session = session;
                var rmq = new RabbitMQService(Config);
                var pep = AutofacContainer.Resolve<IPolicyEngineProvider>();
                var qry = new StudentQry(uc, rmq, policyEngineProvider: pep);
                var parentEmails = qry.GetParentEmails(validOnly: true, activeOnly: true, limitToCourses: limitCoursesTo)
                                        .Distinct()
                                        .ToList();
                Console.WriteLine("Parent emails: " + parentEmails.Count);

                //var courses = qry.GetCourses(limitCoursesTo).Select(o => o.Name).ToList();
                var inst = RedisCache.GetClientDto(inst_id);

                //TBD:
                //var setToRemove = new HashSet<string>(inst.BlockedContacts.Where(o => o.Contains("@")));
                //parentEmails.RemoveAll(x => setToRemove.Contains(x));
                emails = parentEmails;
                emails.Sort();
                emails = emails.GroupBy(x => x).Select(g => g.First()).ToList(); //remove duplicates
                emails = emails.Where(e => Helper.IsValidEmail(e)).ToList();

                //TBD:
                //Console.WriteLine("Emails to send (excl blocked/duplicates): " + emails.Count);

                //Console.WriteLine(string.Format("{0} - {1}", inst.Name, string.Join(",", courses)));

            }

            return emails;
        }
    }
}
