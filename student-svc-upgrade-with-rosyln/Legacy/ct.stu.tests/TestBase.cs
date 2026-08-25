using Autofac;
using ct._Core.Policy;
using ct._Core.Service;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace ct.stu.tests
{
    public class TestBase
    {
        protected static IConfiguration Config { get; private set; }
        protected static IDocumentStore Store { get; private set; }
        protected IContainer _autofacContainer;
        protected IContainer AutofacContainer
        {
            get
            {
                if (_autofacContainer == null)
                {
                    var builder = new ContainerBuilder();

                    builder.RegisterType<DefaultPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.Default);
                    builder.RegisterType<VVSGCSPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSGCS);
                    builder.RegisterType<VVSPHSPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSPHS);
                    builder.RegisterType<VVSSPUPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSSPU);
                    builder.RegisterType<VVSPUCPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSPUC);
                    builder.RegisterType<PolicyEngineProvider>().As<IPolicyEngineProvider>();
                    builder.RegisterType<BTLCOLPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.BTLCOL);
                    builder.RegisterType<AKSHFNPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.AKSHFN);
                    // Other wireups....
                    var container = builder.Build();

                    _autofacContainer = container;
                }

                return _autofacContainer;
            }
        }
        //GCS
        public static string id_vvs_gcs_18_19 = "1a6a3000-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_gcs_18_19_kg1 = "1a6a3021-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_gcs_19_20 = "1a6a3000-1920-45ec-9500-f37f54f82d5f";
        public static string id_vvs_gcs_20_21 = "1a6a3000-2021-45ec-9500-f37f54f82d5f";
        public static string id_vvs_gcs_21_22 = "1a6a3000-2122-45ec-9500-f37f54f82d5f";
        public static string id_vvs_gcs_22_23 = "1a6a3000-2223-45ec-9500-f37f54f82d5f";
        public static string id_vvs_gcs_23_24 = "1a6a3000-2324-45ec-9500-f37f54f82d5f";

        public static string id_vvs_gcs_24_25 = "1a6a3000-2425-45ec-9500-f37f54f82d5f";
        public static string id_vvs_gcs_25_26 = "1a6a3000-2526-45ec-9500-f37f54f82d5f";
        public static string id_vvs_gcs_26_27 = "1a6a3000-2627-45ec-9500-f37f54f82d5f";
        //SPHS
        public static string id_vvs_sphs_18_19 = "1a6a4000-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sphs_20_21 = "1a6a4000-2021-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sphs_21_22 = "1a6a4000-2122-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sphs_23_24 = "1a6a4000-2324-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sphs_18_19_8th = "1a6a4020-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sphs_18_19_9th = "1a6a4021-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sphs_18_19_Xth = "1a6a4022-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sphs_20_21_Xth = "1a6a4022-2021-45ec-9500-f37f54f82d5f";


        //SPPU
        public static string id_vvs_sppu_18_19 = "1a6a5000-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sppu_19_20 = "1a6a5000-1920-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sppu_20_21 = "1a6a5000-2021-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sppu_21_22 = "1a6a5000-2122-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sppu_18_19_puc1_pcmb = "1a6a5020-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sppu_18_19_puc1_pcme = "1a6a5022-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sppu_18_19_puc2_pcmb = "1a6a5021-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_sppu_18_19_puc2_pcme = "1a6a5023-1819-45ec-9500-f37f54f82d5f";

        //VVSPU
        public static string id_vvs_pu_18_19 = "1a6a6000-1819-45ec-9500-f37f54f82d5f";
        public static string id_vvs_pu_19_20 = "1a6a6000-1920-45ec-9500-f37f54f82d5f";
        public static string id_vvs_pu_20_21 = "1a6a6000-2021-45ec-9500-f37f54f82d5f";
        public static string id_vvs_pu_21_22 = "1a6a6000-2122-45ec-9500-f37f54f82d5f";
        public static string id_vvs_pu_20_21_puc2_pcmb = "1a6a6021-2021-45ec-9500-f37f54f82d5f";


        //BTL
        public static string id_btl_col_18_19 = "0c250000-50bf-11e7-89e2-1cbdb9e7fd04";
        public static string id_btl_col_19_20 = "0c250000-1920-11e7-89e2-1cbdb9e7fd04";
        public static string id_btl_col_20_21 = "0c250000-2021-11e7-89e2-1cbdb9e7fd04";
        public static string id_btl_col_23_24 = "0c250000-2324-11e7-89e2-1cbdb9e7fd04";
        //PPECKN
        public static string id_ppec_18_19 = "0c251000-1819-11e7-ff75-d17d371ad608";

        public static string id_vvs_bms_26_27 = "1a6a7000-2627-45ec-9500-f37f54f82d5f";

        public TestBase()
        {

        }
        public static void Configure()
        {
            if (Config == null)
            {
                var config = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .AddUserSecrets<TestBase>()
                                .Build();
                Config = config;

                RedisCache.Url = Config["Redis:Url"];
                RedisCache.Port = Convert.ToInt16(Config["Redis:Port"]);
                RedisCache.Password = Config["Redis:Password"];
            }
        }

        public static void ConfigureStore()
        {
            if (Store == null)
            {
                var _urls = new string[] { Config["Raven:Urls:0"] };
                var _database = Config["Raven:Database"];
                var cert_path = Config["Ct:SSL.ClientCert"];
                var cert_pwd = Config["Ct:SSL.ClientCertPwd"];
                var clientCertificate = new X509Certificate2(cert_path, cert_pwd);
                IDocumentStore store = new DocumentStore
                {
                    Urls = _urls,
                    Database = _database,
                    Certificate = clientCertificate
                };
                store.Initialize(); // initializes document store, by connecting to server and downloading various configurations
                Store = store;
            }
        }
    }
}