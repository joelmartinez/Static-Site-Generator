Title: Mapping My Own Content
Date: 2025-07-31
Published: true
Hero: /posts/linkmap-hero.png
Category: AI

I wrote [*This Post is Worthless*](https://codecube.net/2025/7/worthless-post/) about how the old web worked, back when links weren’t just SEO fuel but the actual connective tissue of the internet. If I’m going to talk about that, I figured I should put my site where my mouth is and actually build something to explore it.

So I did. The [Article Link Map](https://codecube.net/map/) is a little experiment that takes every post I’ve written and drops it into a network graph. Each post connects to the year it was written and to any other post it links to. You can pan, zoom, click around, and hopefully, fall into a weird little rabbit hole of my own making.

The idea isn’t new to me. Back when I was the docs engineer for Xamarin, I had a half-built prototype of this for the documentation. We never quite shipped it because it wasn’t necessarily “commercially useful.” This is my site, though. I don’t have to ask permission or justify the ROI 😅

## A Step Forward In Metadata, Thanks In Part to AI

After getting the map up and running, I decided it needed another layer: categories. My blog software didn’t even have them, _technically_ ... some of the content did behind the scenes. See, this blog software started life as a way for me to get out of Wordpress. So the initial implementation (which is still there) is actually processing the XML of all the content from Wordpress (the markdown content support came later). And so I wrote a custom category system, pushed it live, and was ready to start the long, boring job of tagging a decade-plus of posts.

Instead, I thought to myself ... hey, let Copilot do it! It went through my entire archive and added categories.

Now the map isn’t just posts and years. It’s three overlapping layers:
- Chronology (posts by year)
- Interlinking (posts referencing each other)
- Thematic clusters (AI-generated categories)

There’s something weird and a little thrilling about having AI re-examine your own writing corpus. 

Now ... why bother with something like this? Partly because it’s fun, and partly because I’m trying to experiment with what a personal site can be in 2025 and beyond. The default blog/archive model hasn’t changed much in more than two decades. Everything flows linearly, and search is a bolt-on. But what happens when you give your content a spatial dimension? What happens when exploration isn’t just scrolling backward in time, but literally moving through a graph of your ideas?

This is the first step. The next thing I’m looking at is [GraphRAG](https://www.microsoft.com/en-us/research/project/graphrag/) — using a graph as the backbone for retrieval and navigation. The map you see now is a static version of that concept. I want to see what happens when it becomes dynamic, queryable, maybe even conversational.

This doesn’t have to be useful to anyone but me for now.  But if you find yourself wandering the map and discovering something you wouldn’t have clicked otherwise, maybe that’s enough to make it *useful* after all.
