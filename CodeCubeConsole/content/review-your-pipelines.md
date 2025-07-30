Title: Review Your Pipelines
Date: 2018-05-21  
Published: true  
Category: Programming  

So everyone knows that [friends don't let friends right-click-publish](https://damianbrady.com.au/2018/02/01/friends-dont-let-friends-right-click-publish/).
I had previously posted about how I [created a continuous deployment](http://codecube.net/2017/11/sprinkling-devops/) system for this blog, using VSTS, 
and it had been going great. However, there was a **deep (but simple) problem** lurking in the pipeline that I didn't discover until last week!

VSTS makes it _really easy_ to integrate with github, so that it builds every time a pull request is made. Awesome right?

Well, the issue is that in a momentary judgement lapse, I added the last step to the build process (copying files via SSH to the server);
the deployment step. So if anyone had opened a pull request, it would have automatically built the project, 
and deployed that person's content to my production server, without my review. How embarrassing. 

Thankfully, the fix was really easy ... just use [Release Definitions](https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/release/what-is-release-management?view=vsts)!

Once I removed the deployment task from the build definition, and created a release definition, it plugged the hole I had in my 
release pipeline. Using the [continuous deployment trigger](https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/release/triggers?view=vsts#release-triggers) (limited in scope to the `master` branch),
my ssh copy task can easily reach into the build artifact that resulted from the build. That way, the PR can run the build, and I can 
manually inspect the build artifact to make sure it's all good, and then it automatically publishes when I merge the PR.

So remember that your build/release pipeline is just as important as your application's code ... so review it with the rigor.
