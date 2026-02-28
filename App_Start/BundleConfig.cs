using System.Web;
using System.Web.Optimization;

namespace PAV_PF_AlejandroRodriguezQ
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Mantener comportamiento por ambiente y evitar minificación de WebGrease en JS.
            BundleTable.EnableOptimizations = false;

            // Use plain Bundle (instead of ScriptBundle) to avoid WebGrease JS minification/parsing.
            bundles.Add(new Bundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.7.0.min.js"));

            bundles.Add(new Bundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.min.js",
                        "~/Scripts/jquery.validate.unobtrusive.min.js"));

            // Use modernizr minified file
            bundles.Add(new Bundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-2.8.3.js"));

            // Use the minified bootstrap bundle (includes Popper) and skip JS transforms.
            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }

        private static ScriptBundle CreateScriptBundleWithoutMinification(string virtualPath, params string[] includes)
        {
            var bundle = new ScriptBundle(virtualPath).Include(includes);
            bundle.Transforms.Clear();
            bundle.Transforms.Add(new PreserveJsTransform());
            return bundle;
        }

        private sealed class PreserveJsTransform : IBundleTransform
        {
            public void Process(BundleContext context, BundleResponse response)
            {
                response.ContentType = "text/javascript";
            }
        }
    }
}
