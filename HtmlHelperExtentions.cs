/*
 * Obligatory warning:
 * Software provided as is, free and open, with no warrenty or assurances.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
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
                if (path.Contains('*') || path.Contains('{'))
                {
                    IEnumerable<String> paths = Files(path);
                    foreach (String file in paths)
                    {
                        scripts.Push(new ScriptFile(file));
                    }
                }
                else
                {
                    scripts.Push(new ScriptFile(path));
                }
            }
        }

        static String VersionToken = Regex.Escape("{version}");
        static String VersionRegEx = @"(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?";
        static String WildCardToken = Regex.Escape("*");
        static String WildCardRegEx = @".*";

        private static IEnumerable<String> Files(String path)
        {
            String directoryPath = Path.GetDirectoryName(path);
            String fileName = Path.GetFileName(path);
            VirtualDirectory directory = BundleTable.VirtualPathProvider.GetDirectory(directoryPath);

            String pattern = Regex.Escape(path)
                .Replace(WildCardToken, WildCardRegEx)
                .Replace(VersionToken, VersionRegEx);
            Regex regex = new Regex(pattern);

            return directory.Files.Cast<VirtualFile>()
                .Where(file => regex.IsMatch("~" + file.VirtualPath))
                .Select(file => "~" + file.VirtualPath);
        }

        public static IHtmlString RenderScripts(this HtmlHelper htmlHelper, Boolean useBundles = true)
        {
            Stack<ScriptFile> scripts = htmlHelper.ViewContext.HttpContext.Items[ScriptsKey] as Stack<ScriptFile>;
            IEnumerable<String> notBundles = scripts.Where(script => !script.IsBundle).Select(file => file.Path);
            if (scripts != null)
            {
                StringBuilder builder = new StringBuilder();
                Int32 count = scripts.Count();

                if (useBundles)
                {
                    BundleCollection bundles = BundleTable.Bundles;
                    foreach (Bundle bundle in bundles)
                    {
                        IEnumerable<String> bundleFiles = BundleResolver.Current.GetBundleContents(bundle.Path);

                        if (bundleFiles.Intersect(notBundles).Count() == bundleFiles.Count())
                        {
                            builder.AppendLine(Scripts.Render(bundle.Path).ToString());
                            foreach(ScriptFile script in scripts.Where(script => bundleFiles.Any(file => file == script.Path)))
                            {
                                script.Rendered = true;
                            }
                        }
                    }
                }

                if (scripts.Any(script => !script.Rendered))
                {
                    builder.AppendLine(Scripts.Render(scripts.Where(script => !script.Rendered).Select(script => script.Path).ToArray()).ToString());
                }

                return new MvcHtmlString(builder.ToString());
            }

            return MvcHtmlString.Empty;
        }
    }

    public class ScriptFile {
        public String Path { get; set; }
        public Boolean Rendered { get; set; }
        public Boolean IsBundle { get; set; }
        public ScriptFile (String path)
        {
            Path = path;
            IsBundle = BundleResolver.Current.IsBundleVirtualPath(path);
            Rendered = false;
        }
    }
}
