PartialScripts
==============

Manages scripts for partial views and layouts.

With this script you don't need to reference your bundles inside your views at all, but you can if you wish.
Instead you simply say which of your scripts you require in a given view or partial, and then your configured bundles and extra files can be rendered on your layout.

Note: This isn't a replacement for bundling, you will still need to tell the bundling system which files you wish to include
and provide names to the bundles.

Usage
=====

Usage is simple, inside any view or partial, reference your scripts.

    @{
       Html.ScriptFile("~/Scripts/YourFileHere.js");
    }


Then inside your layout, make a call to RenderScripts:

    @Html.RenderScripts()

and it'll render all the ScriptFile references in the location of RenderScripts()

Bonuses
=======

By default, the rendering looks at your bundle collection to identify which bundles you have included in the page, and it
will choose to render a bundle if you have included all the scripts in a bundle.

Similarly, if you include a bundle with it's virtual path, it will then render that instead using the bundling.

Should you wish to disable this, you can call renderScripts as:

    @Html.RenderScripts(useScripts: false)

and the system will ignore any bundling, passing the virual path directly into Content Mapping.
