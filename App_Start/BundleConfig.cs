using System.Web;
using System.Web.Optimization;

namespace PAV_PF_AlejandroRodriguezQ
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Disable default bundling transforms (minification) to avoid WebGrease parser crashes with modern JS syntax
            BundleTable.EnableOptimizations = false;

            // Include the pre-minified jQuery to avoid runtime minifier crashes with newer jQuery syntax
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.7.0.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.unobtrusive.min.js"));

            // Use modernizr minified file
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-2.8.3.js"));

            // Use the minified bootstrap bundle (includes Popper) to avoid minification issues
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
