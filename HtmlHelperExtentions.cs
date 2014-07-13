/*
 * Obligatory warning:
 * Software provided as is, free and open, with no warrenty or assurances.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;

namespace System.Web.Mvc
{
    public static class HtmlHelperExtentions
    {
        private enum BundleType
        {
            Script,
            Style,
        }

        public const String ScriptsKey = "ScriptFiles";
        public const String StylesKey = "StyleFiles";
        public const String AddedScriptsKey = "AddedScripts";
        public const String AddedStylesKey = "AddedStyles";

        public static void ScriptFile(this HtmlHelper htmlHelper, String path)
        {
            BundleFile(htmlHelper.ViewContext.HttpContext.Items, path, BundleType.Script);
        }

        public static void StyleFile(this HtmlHelper htmlHelper, String path)
        {
            BundleFile(htmlHelper.ViewContext.HttpContext.Items, path, BundleType.Style);
        }

        public static void ScriptFile(IDictionary items, String path)
        {
            BundleFile(items, path, BundleType.Script);
        }

        public static void StyleFile(IDictionary items, String path)
        {
            BundleFile(items, path, BundleType.Style);
        }

        private static void BundleFile(IDictionary Items, String path, BundleType type)
        {
            String key = type == BundleType.Script ? ScriptsKey : StylesKey;
            String addedKey = type == BundleType.Script ? AddedScriptsKey : AddedStylesKey;
            Stack<DeferredBundleFile> files = Items[key] as Stack<DeferredBundleFile>;
            Stack<String> addedfiles = Items[addedKey] as Stack<String>;
            if (files == null)
            {
                files = new Stack<DeferredBundleFile>();
                addedfiles = new Stack<String>();
                Items[key] = files;
                Items[addedKey] = addedfiles;
            }

            if (!addedfiles.Contains(path))
            {
                addedfiles.Push(path);
                if (path.Contains('*') || path.Contains('{'))
                {
                    IEnumerable<String> paths = Files(path);
                    foreach (String file in paths)
                    {
                        files.Push(new DeferredBundleFile(file));
                    }
                }
                else
                {
                    files.Push(new DeferredBundleFile(path));
                }
            }
        }

        static String VersionToken = Regex.Escape("{version}");
        static String VersionRegEx = @"(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?";
        static String WildCardToken = Regex.Escape("*");
        static String WildCardRegEx = @".*";

        internal static IEnumerable<String> Files(String path)
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
            return RenderFiles(htmlHelper.ViewContext.HttpContext.Items, BundleType.Script, useBundles);
        }

        public static IHtmlString RenderStyles(this HtmlHelper htmlHelper, Boolean useBundles = true)
        {
            return RenderFiles(htmlHelper.ViewContext.HttpContext.Items, BundleType.Style, useBundles);
        }

        public static IHtmlString RenderScripts(IDictionary items, Boolean useBundles = true)
        {
            return RenderFiles(items, BundleType.Script, useBundles);
        }

        public static IHtmlString RenderStyles(IDictionary items, Boolean useBundles = true)
        {
            return RenderFiles(items, BundleType.Style, useBundles);
        }

        private static IHtmlString RenderFiles(IDictionary Items, BundleType type, Boolean useBundles = true)
        {
            String key = type == BundleType.Script ? ScriptsKey : StylesKey;
            Stack<DeferredBundleFile> files = Items[key] as Stack<DeferredBundleFile>;
            if (files != null)
            {
                StringBuilder builder = new StringBuilder();
                Int32 count = files.Count();

                if (useBundles)
                {
                    IEnumerable<String> notBundles = files.Where(file => !file.IsBundle).Select(file => file.Path);
                    BundleCollection bundles = BundleTable.Bundles;
                    foreach (Bundle bundle in bundles)
                    {
                        IEnumerable<String> bundleFiles = BundleResolver.Current.GetBundleContents(bundle.Path);

                        if (bundleFiles.Intersect(notBundles).Count() == bundleFiles.Count())
                        {
                            IHtmlString render = type == BundleType.Script ? Scripts.Render(bundle.Path) : Styles.Render(bundle.Path);
                            builder.AppendLine(render.ToString());
                            foreach(DeferredBundleFile file in files.Where(script => bundleFiles.Any(file => file == script.Path)))
                            {
                                file.Rendered = true;
                            }
                        }
                    }
                }

                if (files.Any(script => !script.Rendered))
                {
                    String[] renderfiles = files.Where(file => !file.Rendered).Select(file => file.Path).ToArray();
                    IHtmlString render = type == BundleType.Script ? Scripts.Render(renderfiles) : Styles.Render(renderfiles);
                    builder.AppendLine(render.ToString());
                }

                files.Clear();

                return new MvcHtmlString(builder.ToString());
            }

            return MvcHtmlString.Empty;
        }
    }

    public class DeferredBundleFile {
        public String Path { get; set; }
        public Boolean Rendered { get; set; }
        public Boolean IsBundle { get; set; }
        public DeferredBundleFile (String path)
        {
            Path = path;
            IsBundle = BundleResolver.Current.IsBundleVirtualPath(path);
            Rendered = false;
        }
    }
}
