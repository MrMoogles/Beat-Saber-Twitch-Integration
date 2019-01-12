# Beat Saber Twitch Integration Plugin
For all you streamers out there who want song requests in your chat!

## Installation
1. Download the [latest release](https://github.com/MrMoogles/Beat-Saber-Twitch-Integration/releases) and extract to the Plugins directory in your Beat Saber installation directory.
2. Edit the twitch config (located in `UserData/AsyncTwitchConfig.json`)  
3. Edit the bot config (located in `UserData/TwitchIntegrationConfig.json`) 

Add your Twitch username, an OAuth token that you can get here: https://twitchapps.com/tmi **(this does require the `oauth:` prefix)**  

Lastly, add the channel to monitor. (This doesn't have to be the same as your twitch login, ie if you're using a bot to do requests)

## Requirements
[Beatsaber Custom UI](https://github.com/brian91292/BeatSaber-CustomUI)   
[AsyncTwitch](https://github.com/Soliel/AsyncTwitch)  
[SongLoader](https://github.com/xyonico/BeatSaberSongLoader)
[BeatSaverDownloader](https://github.com/andruzzzhka/BeatSaverDownloader)

Thanks to all of you for making it a bit easier to update this

## Usage
Available Commands:

**[Everyone]**  

| Command | Usage                                                                                                                                                         |
|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| !qhelp       | Shows all available commands                                                                                                                                  |
| !add <song>  | Add a song to queue using the BeatSaver ID/Search Text.                                                                                                       |
| !bsr <song>  | Add a song to queue using the BeatSaver ID/Search Text.                                                                                                       |
| !queue       | Shows the current queue                                                                                                                                       |
| !pat <name>  | Sender pats Receiver                                                                                                                                          |

**[Moderator Only]**  

| Command     | Usage                                                                                                                                |
|-------------|--------------------------------------------------------------------------------------------------------------------------------------|
| !next                 | Removes the current song from queue and displays the next   song.                                                          |
| !clearall             | Clears the queue.                                                                                                          |
| !block                | Adds the song to the banlist (File and array)                                                                              |
| !unblock <id>         | Removes song from the banlist (File and array)                                                                             |
| !close                | Closes the queue.                                                                                                          |
| !open                 | Opens the queue.                                                                                                           |
| !saveq                | Saves the current queue to JSON in UserData Directory                                                                      |
| !randomize            | Randomizes current Queue & uses value set in "RandomizeLimit"                                                              |
| !clearusers           | Works in conjuction with Randomize, It'll clear users of the already chosen list so they can readd songs to future queues. |
| !mtt <id or username> | This will move all of a users requests to the top of the queue, or will move a single request (by ID) to the top of queue. |

To use the bot, place all files in the same location or in the same folder.  
You'll be using the following files:  

BeatSaberBot.exe
Files located in "Config" folder

Note: Blacklist File is a file that can have songs added to during stream while the shadow banlist is meant more for songs that are banned for another reason, maybe your starter song, ending song, etc. Not displayed to chat.  

Edit AsyncTwitchConfig.json to have your channel and Oauth token of yourself or a bot you have setup for your channel
Edit TwitchIntegrationConfig.json for the queue bot options  
Here are the options:  

| Property                    | Usage                                                                                                          |
|-----------------------------|----------------------------------------------------------------------------------------------------------------|
| ModOnly                     | This allows only Moderators to add to the queue as well as perform any commands.                               |
| SubOnly                     | This allows Moderators & Subscribers only to add to the queue.                                                 |
| ViewerLimit                 | *Default: 1*, but this is the number of requests a user can have. Moderators don't have a limit.               |
| SubLimit                    | *Default: 3*, but this is the number of requests a subscriber can have. Moderators don't have a limit.         |
| ContinueQueue               | *Default: false*, If set to true, requests will be kept track of.                                              |
| Randomize                   | *Default: false*, This takes the current queue and chooses the randomize limit and creates a new queue.        |
| RandomizeLimit              | Limit of songs chosen from current queue to recreate new randomized queue.                                     |
| BlockUserMultiRandomQueue   | *Default: false*, If a user is chosen for the random queue, they won't be able to request again until cleared. |
| OverrideSongInMultipleQueue | *Default: false*, If set to true, users can request songs already played as long as not currently in queue.    |
| EnableShadowQueueButton     | *Default: false*, Not quite implemented yet... Ignore for now                                                  |

Example blacklist file. With new API changes, the format usually has both numbers with Hyphen, but can be blocked using just the first set.
Similar setup in the shadow_blacklist.txt file.

{
    "_bannedSongs": [
        "1121",
        "1120",
        "1118"
    ]
}

Incoming Features:
Implement Original Soundtrack for Requests
Implement Shadow Ban list
Implement Configuration option to return Chat messages when enabled
Fix bug when it looks like SongDownloader didn't download last song in queue
Add Refresh at the end of song when going back to the level select screen
  
I stream sometimes, catch me on twitch @ twitch.tv/mr_moogles :)
