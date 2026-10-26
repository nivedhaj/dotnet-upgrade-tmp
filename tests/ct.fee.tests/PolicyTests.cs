using Ct.Fee.Application.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

[TestClass]
public class PolicyTests
{
    [TestMethod]
    public void Default_policy_orders_tuition_first()
    {
        var p = new DefaultFeePolicyEngine();
        Assert.IsTrue(p.FeeOrderIndex("Tuition fees") < p.FeeOrderIndex("Transport"));
        Assert.IsTrue(p.IncludeRteStudents());
        Assert.IsTrue(p.GetFeeCollectionAccounts().Count > 0);
        Assert.IsFalse(p.SplitTuitionFees());
    }

    [TestMethod]
    public void Btl_policy_excludes_rte_and_has_accounts()
    {
        var p = new BtlColFeePolicyEngine();
        Assert.IsFalse(p.IncludeRteStudents());
        Assert.IsTrue(p.ExcludeFeesForRte(null).Any());
        Assert.IsTrue(p.GetFeeCollectionAccounts().Any(a => a.Contains("BTL")));
    }

    [TestMethod]
    public void Vvs_gcs_policy_classifies_fee_names()
    {
        var p = new VvsGcsFeePolicyEngine();
        Assert.IsTrue(p.SplitTuitionFees());
        var map = p.ClassifyFeeName("Tuition fee Std I");
        Assert.IsNotNull(map);
        Assert.AreEqual("TF", map!.FeeCode);
    }

    [TestMethod]
    public void Vvs_phs_and_bms_policies_have_accounts()
    {
        Assert.IsTrue(new VvsPhsFeePolicyEngine().GetFeeCollectionAccounts().Count > 0);
        Assert.IsTrue(new VvsBmsFeePolicyEngine().GetFeeCollectionAccounts().Count > 0);
        Assert.IsTrue(new DefaultFeePolicyEngine().CourseOrder().Count > 0);
    }
}
