Pivot.Update
====================
A C# library for automatically updating your games.  Licensed under MIT. __This is still experimental and breaking changes might occur, so you should check back here often to ensure you have the latest version of the library.__

About and Usage
----------------
We've all been there; the issue of how do you get updates to your users in a nice, easy way.  You could update through digital distribution and publishers, but that doesn't always give you the flexibility you want.  Nightly builds and multiple build channels are often impossible.

Pivot.Update is a C# library you can embed in your C#-based game.  It has a very simple API:

```csharp
// Register the current working directory to be automatically updated
// from http://update.myserver.com/nameofmygame.
API.Register("http://update.myserver.com/nameofmygame");

// If we have updates, we should perform them before the game starts.
if (API.HasUpdates())
{
    // Schedule an update to occur 10 seconds from now, and when the
    // update is complete, automatically restart the game by running
    // the program at the specified path.  You can often detect the
    // executable path using .NET like so:
    API.ScheduleUpdate(10, Assembly.GetExecutingAssembly().Location);
    
    // You should terminate the game right away, since there's only 10
    // seconds until the update begins.  Between now and then, all locks
    // on files that need to be updated in the current directory should
    // have been released for the update to complete successfully.
    Game.Exit();
}
```

You will also need to set up and run the update server to deliver updates to your users.  The "Pivot.Update.Server" is the executable to run, and it by default runs on port 38080.  You can change the port by modifying the relevant code.

The server is taught what games and applications it offers through the "pvctrl" executable.  The server doesn't need to be running for this command to work, nor does it need to be restarted when you make changes with this command.  The command can be used like:

```bash
# Enter the directory which contains exactly and only the files you want
# to ship as part of your game.  Then run:
pvctrl create nameofmygame

# This will create the initial store for your game.  Whenever you update
# your game and want to ship a new version, you can do:
pvctrl flash nameofmygame

# Be careful!  These commands do not prompt for individual files and there
# is no mechanism to undo a "flash".
```

Important Note
-----------------
Whenever your game starts, the Pivot.Update library will check to see if the local Windows service is currently registered and running on the computer.  If it is not, it will automatically download the Windows service from http://update.redpointsoftware.com.au/pivot.update/ and attempt to install it, which may cause a UAC prompt.  It's probably a good idea to inform your players / users about this so they won't be surprised when the UAC dialog appears.

Contributing
-----------------
The most important missing features at the moment:
 * __A way for the Pivot service to update itself.__
 * `pvctrl flash` should be able to individually flash files and needs an undo mechanism.
 * `pvctrl` needs a pruning mechanism to delete old updates that are no longer needed.
See the issue tracker for more bugs / issues that need dealing with.

You can contribute by forking and submitting pull requests.