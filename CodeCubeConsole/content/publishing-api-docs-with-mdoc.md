Title: Publishing .NET API Docs with mdoc
Date: 2018-02-01
Published: true

<blockquote class="twitter-tweet" data-lang="en"><p lang="en" dir="ltr">When I ask myself &quot;what would have the most impact today?&quot; I sit down and write documentation.</p>&mdash; Miguel de Icaza (@migueldeicaza) <a href="https://twitter.com/migueldeicaza/status/675514897065709568?ref_src=twsrc%5Etfw">December 12, 2015</a></blockquote>
<script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>

Writing documentation is incredibly important. This post will show you how you can document and publish your own
documentation using the same open source tools we use at Microsoft.

## mdoc

Over the years, there have been a number of potential solutions for documenting your .NET codebase. 
The Mono project spawned the tool _mdoc_, and it is now being used as part of the publishing pipeline for docs.microsoft.com!

You can find _mdoc_ on github here: https://github.com/mono/api-doc-tools

Although _mdoc_ contains many features, for the purposes of this post, we're going to focus on the `update` subcommand.
This command will scan a managed assembly (.dll or .winmd), and create a set of XML files (known as _EcmaXML_). 
These XML files are a structured representation of the assembly's API surface area, and you can expand the XML with
your own written content.

## docfx

## Dependencies via Nuget

## msbuild script

### Compile the Sample/Library

### Build the EcmaXML

### "Export" API Docs

### Publish Site with DocFX

## Sample onn GitHub

https://github.com/joelmartinez/mdoc-docfx
