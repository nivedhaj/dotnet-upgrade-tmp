using ct._Core.Repo.Db.Command;
using ct._Core.Repo.Db.Query;
using ct._Core.Repo.Resource;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ct.stu.tests
{
    [TestClass]
    public class PingTests : TestBase
    {
        [ClassInitialize]
        public static void init(TestContext context)
        {
            Configure();
            ConfigureStore();
        }

        [TestMethod]
        [TestCategory("Cmd")]
        public void can_create_ping()
        {

            using (var session = Store.OpenSession())
            {
                var cmd = new PingCmd(session, Config);
                var result = cmd.Create("ping-1");
                Assert.IsNotNull(result);
            }
        }
        [TestMethod]
        [TestCategory("Qry")]
        public void can_get_ping()
        {
            string serviceName = Config.GetSection("Service:Name").Value;
            using (var session = Store.OpenSession())
            {
                var qry = new PingQry(session, Config);
                var pings = qry.Get();
                foreach (var ping in pings ?? new List<Ping>())
                {
                    if(ping==null) continue;
                    Assert.AreEqual(ping.Service, serviceName);
                }
            }
        }
    }
}
