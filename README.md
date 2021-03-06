PartialScripts
==============

Manages scripts and stylesheets for **ASP.NET MVC** partial views and layouts.

With this script you don't need to reference your bundles inside your views, but you can if you wish. Instead you simply state which of your scripts you require with a given view or partial, and then your configured bundles and extra files can be rendered on your layout.

***Note:*** This isn't a replacement for bundling, although it may well be in the future. Currently, when using bundling, you will still need to tell the bundling system which files you wish to include and provide names to the bundles.

Adding the code to a default MVC5 project is relatively simple, put the `HtmlHelperExtentions.cs` somewhere in your project (anywhere sensible), and combine the contents of the ***App_Code*** directory with your own project's ***App_Code***

Usage
=====

Usage is simple, inside any view or partial, reference your scripts:

```csharp
@Scripts.Include("~/Scripts/YourFileHere.js")
```
    
Similarly, for stylesheets:

```csharp
@Styles.Include("~/Content/Stylesheets/YourFileHere.css")
```
    
As with bundling, the system supports version and wildcard style syntax like so:

```csharp    
@Scripts.Include("~/Scripts/jquery-{version}.js")
```

Then inside your ***_Layout.cshtml***:

```csharp
@Styles.RenderAll()
@Scripts.RenderAll()
```

and it'll render tags for all previously included scripts in the location of your `RenderAll()` calls.

Quirks and Limitations
======

I've collected a few of the quirks i've found while using this module. There's only a couple, and they aren't particularly unpredictable or unexpected.

Razor Helpers
-------------
A simple limitation of Razor's helpers means that you must make calls to `Scripts.Include()` using the `@` style syntax rather than `@{ }`. Helpers found within a `@{ }` tag will ***not be hit***.

Razor Execution Order
---------------------

Take the simple example ***_Layout.cshtml*** below:

```html
<head>
    @Styles.Include("~/Content/Stylesheets/site.css")

    @Scripts.Include("~/Scripts/jquery-{version}.js")
    @Scripts.Include("~/Scripts/jquery.signalR-{version}.js")
    @Scripts.Include("~/Scripts/Handlebars.min.js")

    @Styles.RenderAll()
    @Scripts.RenderAll()
</head>
<body>
    @Html.Partial("Top", Model)
    <hr>
    @RenderBody()
    <hr>
    @Html.Partial("Bottom", Model)
</body>
```

The Body of your page is rendered prior to your ***_Layout.cshtml*** and later inserted into the page at `RenderBody()`. meaning any `Scripts.Include()` calls will be hit before your `Script.RenderAll()` calls.

The partials ***Top*** and ***Bottom*** are rendered during your ***_Layout.cshtml***'s executing, meaning any `Scripts.Include()` calls will be hit after the `Script.RenderAll()` call is hit. 

There is a simple solution to this, always include your scripts before the ```Script.RenderAll()``` call.

In Practice, this means you must do one of two things. Either include all scripts from partials found in ***_Layout.cshtml*** in the head, before calling `Scripts.RenderAll()` or Call `Scripts.RenderAll()` at the bottom of the body, after all the views have had a chance to render.

`Scripts.RenderAll()` will handle multiple calls correctly though, so you can put one call at the top, and one at the bottom of your layout. Scripts added inside your ***_Layout.cshtml*** body will then be called.

Extras
======

Debugging Bundle Matching
-------------------------

While `BundleTable.EnableOptimizations` is false and `useBundling` is true (See below), html comments will be inserted into the rendered page indicating what has been rendered. One comment will be inserted for each matched bundle, with the name of the bundle. while a comment will be inserted at the bottem, before any remaining files are rendered.

Don't like bundling?
--------------------

By default, the rendering looks at your bundle collection to identify which bundles you have included in the page, and it will choose to render a bundle if you have included all the files in a bundle.

Similarly, if you include a bundle with it's virtual path, it will then render that instead using the bundling.

Should you wish to disable this, you can call `Scripts.RenderAll()` as:

```csharp
@Scripts.RenderAll(useBundles: false)
```

and the system will ignore any bundling, passing the virual path directly into Content Mapping.
