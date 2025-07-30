Title: Sprinkling Some Devops  
Date: 2017-11-09  
Published: true  
Category: Programming  

DevOps ... it's something that I have a love/hate relationship with. When I'm working on a project
that has a really nice devops pipeline (testing/deployment), it's pure bliss. However, when I'm 
working on a project in which devops is my responsibility, I procrastinate like crazy in setting it
up.

This blog is run by a [small custom program](https://github.com/joelmartinez/Static-Site-Generator),
which takes an old wordpress content export file (previous CMS), and some markdown content (for new 
stuff), and generates static HTML. Up until recently, publishing a new post involved:

1. opening visual studio
2. authoring the post
3. running the program
4. connecting via FTP or SSH to the linux server
5. transferring the content
6. committing the changes to git.

It was pretty tedious, and if you look at [the archives](/#archives), you'll note that there's 
been a dearth of content over the past four years. 

But no more! Nowadays, publishing a new post involves

1. Opening Visual Studio Code
2. Authoring the Post
3. Commiting the changes to git

So much nicer! I'm using [Visual Studio Team Services](https://www.visualstudio.com/team-services/) 
(VSTS) to watch my github repo, automatically run this [msbuild file](https://github.com/joelmartinez/Static-Site-Generator/blob/master/build.proj),
and upload the results via SSH to my server. Completely automated deployment, and it's beautiful!

If you're working on a project, and there are **any manual steps at all** in: building, testing, and deployment ... then I would recommend that you spend a little bit of time, and get your DevOps on.