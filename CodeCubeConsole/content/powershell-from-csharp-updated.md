Title: Running PowerShell from C# in 2025
Date: 2025-07-16
Published: true
Prev: /2009/11/executing-powershell-scripts-via-c/

While checking my analytics recently, I noticed that an old post of mine from 2009 was still getting hits. It was a little helper class I had put together to run PowerShell scripts from a C# app. At the time, the approach was to just spin up a powershell.exe process, pass in a script path, and build the parameter list with some string interpolation. It worked well enough for simple automation, and the code was lightweight with no extra dependencies.

What I didn’t do back then was use the actual PowerShell SDK. It existed in some form, but to get access to `System.Management.Automation.dll`, you had to install the full Windows SDK or dig around in system folders to find it. There was no NuGet package, and it definitely wasn’t something you could just drop into a cross-platform app. Since I didn’t want to add that kind of dependency to the project, I stuck with shelling out to `powershell.exe`.

Things have changed since then. Today, .NET is fully cross-platform, and PowerShell is too. You can install PowerShell Core on Windows, macOS, and Linux, and you can grab the PowerShell SDK directly from NuGet without needing the Windows SDK at all. More importantly, you can host the PowerShell runtime in-process, which means you can run scripts with typed parameters and capture structured output, without ever building up a shell command by hand.

I figured I’d revisit the topic and show how to do this the right way in 2025.

To try this out, open a terminal and create a new console app. I made a folder called `pwsh-runner`, ran `dotnet new console` inside it, and then added the SDK with:

```bash
dotnet add package Microsoft.PowerShell.SDK
```

That’s it for setup.

Next, I dropped a small PowerShell script in the project directory. It’s just a simple file called example.ps1 with the following contents:

```pwsh
param(
  [string]$name,
  [int]$count
)

"Hello, $name! You’ve run this $count times."
```

Back in the C# app, I replaced the contents of `Program.cs` with this:

```csharp
using System.Management.Automation;

var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "example.ps1");

using var ps = PowerShell.Create();
ps.AddCommand(scriptPath);
ps.AddParameter("name", "Joel");
ps.AddParameter("count", 3);

var results = ps.Invoke();

foreach (var result in results)
{
    Console.WriteLine(result);
}
```

When I ran `dotnet run`, it printed:

```
Hello, Joel! You’ve run this 3 times.
```

No shell commands, no string interpolation, no fuss. Just calling into the PowerShell runtime from C# and getting clean output back. And this works the same way on Windows, macOS, or Linux, as long as PowerShell Core is installed on the system.

There are still valid reasons to run PowerShell via an external process in some scenarios, especially if you need process isolation or are doing impersonation. But for most automation and tooling use cases, hosting PowerShell directly like this is easier and much safer. You don’t have to worry about escaping quotes, validating user input against injection attacks, or dealing with platform-specific quirks in how the shell parses arguments.

It’s always interesting to look back at old solutions that got the job done but came with caveats that were just normal at the time. The tooling has come a long way, and if you’re still relying on the old `Process.Start` approach, it might be time to give this a fresh look.