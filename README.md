PartialScripts
==============

Manages scripts for **asp.net mvc** partial views and layouts.

With this script you don't need to reference your bundles inside your views at all, but you can if you wish. Instead you simply say which of your scripts you require in a given view or partial, and then your configured bundles and extra files can be rendered on your layout.

Note: This isn't a replacement for bundling, you will still need to tell the bundling system which files you wish to include and provide names to the bundles.

Adding the code to your project is simple, put the HtmlHelperExtentions somewhere in your project (anywhere sensible), and combine the contents of App_Code with your own project's App_Code

Usage
=====

Usage is simple, inside any view or partial, reference your scripts:

    @Scripts.Include("~/Scripts/YourFileHere.js");
    
Similar to bundling, the system also supports version and wildcard style syntax:
    
    @Scripts.Include("~/Scripts/jquery-{version}.js");

Then inside your layout, make a call to RenderScripts:

    @Scripts.RenderAll()

and it'll render tags for all previously included scripts in the location of your @Scripts.RenderAll()

Quirks
======

I've collected a few of the quirks i've found while using this module. There's only a couple, and they aren't particularly unpredictable or unexpected.

Outputting
----------
A simple limitation of Razor's helpers means that you must make calls to Scripts.Include using the @ style syntax rather than @{}. Helpers found within a @{ } tag will not be hit.

Razor Execution Order
---------------------

Take the simple example _Layout.cshtml below:

    <head>
        @Styles.Render("~/Content/site.css")

        @Scripts.Include("~/Scripts/jquery-{version}.js")
        @Scripts.Include("~/Scripts/jquery.signalR-{version}.js");
        @Scripts.Include("~/Scripts/Handlebars.min.js");

        @Scripts.RenderAll()
    </head>
    <body>
        @Html.Partial("Top", Model)
        <hr>
        @RenderBody()
        <hr>
        @Html.Partial("Bottom", Model)
    </body>

The Body of your page is rendered prior to your _layout.cshtml and later inserted into the page at RenderBody. meaning any Scripts.Include calls will be hit before your Script.RenderAll calls.

The partials "Top" and "Bottom" are rendered during your _layout.cshtml's executing, meaning any Scripts.Include calls will be hit after the Script.RenderAll call is hit. 

There is a simple solution to this, always include your scripts before the Script.RenderAll call.

In Practice, this means you must do one of two things. Either include all scripts from partials found in _Layout.cshtml in the head, before calling Scripts.RenderAll or Call Scripts.RenderAll at the bottom of the body, after all the views have had a chance to render.

RenderAll will handle multiple calls correctly though, so you can put one call at the top, and one at the bottom of your layout. Scripts added inside your _Layout.cshtml body will then be called. 

Bonuses
=======

Don't like bundling? By default, the rendering looks at your bundle collection to identify which bundles you have included in the page, and it will choose to render a bundle if you have included all the scripts in a bundle.

Similarly, if you include a bundle with it's virtual path, it will then render that instead using the bundling.

Should you wish to disable this, you can call renderScripts as:

    @Scripts.RenderAll(useBundles: false)

and the system will ignore any bundling, passing the virual path directly into Content Mapping.
