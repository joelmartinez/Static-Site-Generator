Title: Making My Site AI-Ready for NLWeb
Date: 2025-07-10
Published: true
Category: AI

I was somewhat intrigued when I learned about NLWeb at Microsoft's Build conference. It's an open-source project from Microsoft that introduces a protocol for making websites more accessible to AI agents. The idea is to help AI assistants understand and interact with your content directly—without having to scrape pages or guess at structure.

Now, this isn't the entire implementation of course ... but I wanted a low-effort way of starting to think towards this transition. Instead of building a chatbot or setting up middleware, I'm starting by updating the existing site with structured data like JSON-LD, a simple manifest file, and some semantic metadata. It's a low-friction way to prepare for the next phase of the internet, where AI tools might increasingly be the ones "browsing" on behalf of users.

You can read more about NLWeb here:  
[github.com/microsoft/NLWeb](https://github.com/microsoft/NLWeb)  
[Introducing NLWeb from Microsoft](https://news.microsoft.com/source/features/company-news/introducing-nlweb-bringing-conversational-interfaces-directly-to-the-web/)

Part of why I wanted to work on this was curiosity. I wanted to get hands-on with this emerging standard and see what it would take to make my content AI-friendly. But I also think it's a smart bet. The way we build and consume content is changing. Search is shifting. Agents are coming. This was my way of making sure I’m not caught flat-footed when that shift becomes the norm. I've always been a fan of microformats and the [Semantic Web](https://en.wikipedia.org/wiki/Semantic_Web) from the early days of the web ... adding just a bit of semantic sugar to your markup so that machines could understand your intent. NLWeb feels like the next step in that same spirit, but designed for a world of language models instead of feed parsers.

If my blog and project pages are going to be discovered, cited, and engaged with in that world, they need to speak the language of AI. NLWeb gives you a framework for doing that in a clean, standardized way.

## I built all of it using Copilot!

A secondary goal for me was continuing to gain experience working with the intelligent coding experiences that are increasingly being built. For this, every step of this process was done with GitHub Copilot. First, I asked it to explain the [NLWeb GitHub repo](https://github.com/microsoft/NLWeb) and walk me through what the spec required. It gave me a great summary and laid out the key pieces to get ready for NLWeb: a manifest file, structured data in the templates, updated `robots.txt`, and a few new meta tags.

I then asked Copilot to make the changes directly to my static site generator, which is a custom project I’ve had sitting around for years. It added:

- A new `nlweb.json` manifest file
- Schema.org metadata in each article using microdata and JSON-LD
- A `<link rel="nlweb-manifest">` reference in the HTML head
- Meta tags to declare AI access permissions
- A `robots.txt` file that allows bots like GPTBot and OpenAI-SearchBot

While I was in there, I figured I’d finally check something off my backlog as well, generating a proper `sitemap.xml` file. That’s another thing I asked Copilot to do—and it handled it cleanly.

I came away from this feeling like I'm slowly preparing for the future. The spec itself is straightforward, and I'd like to keep going here. With Copilot guiding me through each step, it felt more like collaborative problem solving than manual coding. I completely understand there's a lot of fear around how this massive change will manifest, but the whole experience reminded me that the barriers to participating in emerging tech are getting lower. With the right tools, it's easy to dip your toes in and get something meaningful done in just a few hours.

My site is now _more_ AI-readable. If and when AI agents start crawling the web for meaningful content, mine will be ready. If you’re running your own site—especially if you publish content regularly—I recommend giving NLWeb a look. You can start to add a few building blocks that make your intent and structure machine-readable, and then continue on to the full implementation like I plan to once you get there.

If you want help getting started or want to see the PR where I added structured and semantic metadata, it’s right here:  
[github.com/joelmartinez/Static-Site-Generator/pull/16](https://github.com/joelmartinez/Static-Site-Generator/pull/16)
