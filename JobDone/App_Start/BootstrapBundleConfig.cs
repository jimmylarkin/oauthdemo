using System.Web.Optimization;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(JobDone.BootstrapBundleConfig), "RegisterBundles")]

namespace JobDone
{
    public class BootstrapBundleConfig
    {
        public static void RegisterBundles()
        {
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap*"));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap").Include("~/Content/bootstrap.css", "~/Content/bootstrap-responsive.css", "~/Content/site.css"));
        }
    }
}
