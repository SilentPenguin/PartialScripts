using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace System.Web.Mvc
{
    public static class HtmlHelperExtentions
    {
        public const String ScriptsKey = "ScriptContexts";

        public static void ScriptFile(this HtmlHelper htmlHelper, String path)
        {
            Stack<ScriptFile> scripts = htmlHelper.ViewContext.HttpContext.Items[ScriptsKey] as Stack<ScriptFile>;
            if (scripts == null)
            {
                scripts = new Stack<ScriptFile>();
                htmlHelper.ViewContext.HttpContext.Items[ScriptsKey] = scripts;
            }

            if (!scripts.Any(row => row.Path == path)){
                scripts.Push(new ScriptFile(path));
            }
        }

        public static IHtmlString RenderScripts(this HtmlHelper htmlHelper, Boolean useBundles = true)
        {
            Stack<ScriptFile> scripts = htmlHelper.ViewContext.HttpContext.Items[ScriptsKey] as Stack<ScriptFile>;
            if (scripts != null)
            {
                StringBuilder builder = new StringBuilder();
                UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection);
                Int32 count = scripts.Count();

                if (useBundles)
                {
                    BundleCollection bundles = BundleTable.Bundles;
                    BundleContext bundleContext = new BundleContext(htmlHelper.ViewContext.HttpContext, bundles, "/");
                    foreach (Bundle bundle in bundles)
                    {
                        IEnumerable<String> bundleFiles = bundle.EnumerateFiles(bundleContext).Select(file => file.VirtualFile.VirtualPath);

                        if (scripts.Any(script => script.Path == bundle.Path))
                        {
                            builder.AppendLine(Scripts.Render(bundle.Path).ToString());
                            scripts.Single(file => file.Path == bundle.Path).Rendered = true;
                        }
                        else if (bundleFiles.All(file => scripts.Any(script => script.Path == file)))
                        {
                            builder.AppendLine(Scripts.Render(bundle.Path).ToString());
                            foreach(ScriptFile script in scripts.Where(script => bundleFiles.Any(file => file == script.Path)))
                            {
                                script.Rendered = true;
                            }
                        }
                    }
                }

                for (Int32 i = 0; i < count; i++)
                {
                    ScriptFile context = scripts.Pop();
                    if (!context.Rendered)
                    {
                        builder.AppendLine("<script type='text/javascript' src='" + urlHelper.Content(context.Path) + "'></script>");
                    }
                }
                return new MvcHtmlString(builder.ToString());
            }

            return MvcHtmlString.Empty;
        }
    }

    public class ScriptFile {
        public String Path { get; set; }
        public Boolean Rendered { get; set; }
        public ScriptFile (String path)
        {
            Path = path;
            Rendered = false;
        }
    }
}
