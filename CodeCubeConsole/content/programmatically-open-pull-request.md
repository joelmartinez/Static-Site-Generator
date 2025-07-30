Title: Programmatically Opening Pull Requests
Date: 2018-10-24  
Published: true 
Category: Programming 

When integrating github repositories into any automated workflow, a very common pattern is for the process to open a pull request against some repository. This lets a human step into the mix and review it, before accepting the change.

Over the past few years, while working on the Xamarin developer portal, I developed a small library to help me with a few common CI git related tasks called [GitmoSharp](https://github.com/joelmartinez/GitmoSharp/). Recently I had to need to implement exactly the kind of workflow I describe above, where I open a pull request after some program runs. So I added a small CLI companion tool to GitmoSharp which I've named, simply, [gitmo](https://www.nuget.org/packages/gitmo/) (available on Nuget).

Right now, it only has one subtask, though in the future I plan on exposing some of the other functionality from the library ... but for now, you can open a pull request on a github repository.

You can easily test this with the following steps. First, pull down a sample repository, I have one here ... please feel free to fork it to your own account:

    git clone https://github.com/joelmartinez/testrepo

then make a change in the file, and commit it to another branch

    git checkout -b somebranch
    # make changes to file
    git commit -a -m "committing changes"
    git push origin somebranch

You have to push the branch to GitHub, because the API call to open the pull request will need that to be on the server already.

Now, to make this tool easy to use from a CI agent, you can use Nuget to install it. For testing locally, you can easily install nuget client tools if it's not already available on your command line (it ships with Mono, for example):
https://docs.microsoft.com/en-us/nuget/install-nuget-client-tools#nugetexe-cli

Once you verify that `nuget` is available on your command line, run the following command

    nuget install gitmo -OutputDirectory packages -ExcludeVersion

Once you have that installed, you can 

    dotnet .\packages\gitmo\tools\gitmo.dll open-pr

Without any additional parameters, it will tell you which parameters you need to supply

    Required Values (set with `=`):
            -repopath: Path to the repository
            -repoowner: The user or org name of the repo where you're opening the PR
            -reponame: Name of the repository (it doesn't necessarily have to match the path/folder)
            -branch: The branch that you're merging (in the local repository)
            -name: Your full name
            -email: Your email
            -username: Your username
            -pass: Your password or personal access token
            -title: The title of the pull request
            -message: The message associated with the pull request. This can be markdown

From here, you can just invoke the cli tool ... note the path doesn't include the version, this is due to the nuget restore's `-ExcludeVersion` parameter. If you want to have a specific version, then feel free to remove that parameter from the call above, and even provide a specific version. But in my case:

     dotnet.exe ./packages/gitmo/tools/gitmo.dll open-pr -repopath=testrepo/ -repoowner=joelmartinez -reponame=testrepo -branch=somebranch -name=<your full name> -email=<your email address> -username=<your username> -pass=<your password or PAT> -title="PR Opened with gitmo" -message="This pull request was opened using gitmo cli, available on nuget!"

 Once that runs, in this case you can see the resulting pull request here:  
 https://github.com/joelmartinez/testrepo/pull/3

 Please let me know if you find this tool useful, as I'd like to continue developing it and increasing it's utility.