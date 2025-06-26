Static-Site-Generator

=====================


CLI tool to generate a static site from a Wordpress export file and markdown content.
Templates are rendered using [RazorLight](https://github.com/toddams/RazorLight) and the project targets **.NET 8**.
Markdown is parsed using the [Markdig](https://github.com/xoofx/markdig) library with advanced extensions enabled so GitHub style tables and other syntax are supported out of the box.

This was originally written primarily to support the move of my blog (http://codecube.net) off of Wordpress. However this tool could be generalized into a more broadly useful tool given time and attention :)

In the meantime, it can be easily customized.

* The content model is driven by a WordPress export file (content.xml)
* `Program.GetContent` loads the file and uses linq-to-xml to build a list of post objects that serve as the model.
* `Program.BuildSite` drives the creation of files. In the current iteration of this app, it's just an index page, and a list of posts.
* The content templates use Razor syntax and are stored in /Templates folder.
* Any non-template content such as images and css can be put in the /out folder. Just make sure you set the file properties to "copy if newer".

The static site will be output into the output location's 'out' folder (ie. `/bin/debug/out`).

### Building locally

Ensure you have the [.NET 8 SDK](https://dotnet.microsoft.com/) installed and then run:

```bash
dotnet run --project CodeCubeConsole
```
