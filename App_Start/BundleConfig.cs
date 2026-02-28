using System.Web;
using System.Web.Optimization;

namespace PAV_PF_AlejandroRodriguezQ
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Keep framework default (debug/release behavior) and disable only JS minification transforms per bundle.
            BundleTable.EnableOptimizations = false;

            var jqueryBundle = new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.7.0.min.js");
            jqueryBundle.Transforms.Clear();
            bundles.Add(jqueryBundle);

            var jqueryValBundle = new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.min.js",
                        "~/Scripts/jquery.validate.unobtrusive.min.js");
            jqueryValBundle.Transforms.Clear();
            bundles.Add(jqueryValBundle);

            // Modernizr can remain as delivered file; avoid WebGrease transforms as well.
            var modernizrBundle = new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-2.8.3.js");
            modernizrBundle.Transforms.Clear();
            bundles.Add(modernizrBundle);

            // Use the minified bootstrap bundle (includes Popper) and skip JS transforms.
            var bootstrapBundle = new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js");
            bootstrapBundle.Transforms.Clear();
            bundles.Add(bootstrapBundle);

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
