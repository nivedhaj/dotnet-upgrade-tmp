using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repo.Db.Index;

namespace ct.stu.tests
{
    [TestClass]
    public class IndexTests : TestBase
    {
        [ClassInitialize]
        public static void init(TestContext context)
        {
            Configure();
            ConfigureStore();
            Console.WriteLine("Store Config Done!");
        }

        [TestMethod]
        public void can_create_index()
        {
            // new AttendanceEvent_IX().Execute(Store);
            // new Ping_Index().Execute(Store);
             new Student_Inst_Index().Execute(Store);
           // new Student_Essentials_IX().Execute(Store);
            // new Student_Search().Execute(Store);
            // new Student_Coursewise_Count().Execute(Store);
            // new Coursewise_Sectionwise_Student_Index().Execute(Store);
            // new Course_Structure_Index().Execute(Store);
            //new Inst_Student_Count().Execute(Store);
        }
    }
}
