using System;
using System.Linq;
using System.Text.RegularExpressions;
using AsyncTwitch;
using JetBrains.Annotations;
using SongLoaderPlugin;
using TwitchIntegrationPlugin.Misc;
using TwitchIntegrationPlugin.Serializables;
using TwitchIntegrationPlugin.UI;

namespace TwitchIntegrationPlugin.Commands
{
    [UsedImplicitly]
    public class AddToQueueCommand : IrcCommand
    {
        public override string[] CommandAlias => new [] {"bsr", "add", "kloudr", "gib", "gibsong", "tomato", "awoo"};
        private readonly Regex _songIdrx = new Regex(@"^[0-9\-]+$", RegexOptions.Compiled);

        public override void Run(TwitchMessage msg)
        {
            if (!StaticData.TwitchMode && !msg.Author.IsMod && !msg.Author.IsBroadcaster)
            {
                TwitchConnection.Instance.SendChatMessage("The Queue is currently closed.");
                return;
            }

            string queryString = msg.Content.Remove(0, msg.Content.IndexOf(' ') + 1);
            bool isTextSearch = !_songIdrx.IsMatch(queryString);

            // Checking to see if user was chosen for previous Random Queue
            if (StaticData.Config.Randomize && StaticData.Config.BlockMultiRandomQueue)
            {
                foreach (string user in StaticData.UserPickedByRandomize)
                {
                    if (user.Contains(msg.Author.DisplayName))
                    {
                        TwitchConnection.Instance.SendChatMessage(msg.Author.DisplayName + " you were already chosen for previous queue.");
                        return;
                    }
                }
            }

            SongDownloader.Instance.RequestSongByQuery(queryString, isTextSearch, msg.Author.DisplayName, request =>
            {
                // Make sure request returns with valid information before going further
                if (String.IsNullOrEmpty(request.hash) || String.IsNullOrEmpty(request.id))
                {
                    TwitchConnection.Instance.SendChatMessage("Invalid Request.");
                    return;
                }

                // Check to see if song exists on Ban List
                if (StaticData.BanList.IsBanned(request.id))
                {
                    TwitchConnection.Instance.SendChatMessage("Song is currently banned.");
                    return;
                }

                // Check to see if song exists on Shadow Ban List
                if (StaticData.ShadowBanList.IsBanned(request.id))
                {
                    return;
                }


                // Check to see if song has already been played in previous Queue
                if (StaticData.Config.OverrideSongInMultiQueue)
                {
                    foreach (Song s in StaticData.AlreadyPlayed.SongQueueList)
                    {
                        if (s.songName.Equals(request.songName))
                        {
                            TwitchConnection.Instance.SendChatMessage(request.requestedBy + ", \"" + request.songName + "\" was chosen for previous queue.");
                            return; 
                        }
                    }
                }

                // Request # Check before adding to queue
                // Added Skip of this check for Broadcasters and Moderators
                if (StaticData.UserRequestCount.ContainsKey(msg.Author.DisplayName))
                {
                    int requestLimit = msg.Author.IsSubscriber
                        ? StaticData.Config.SubLimit
                        : StaticData.Config.ViewerLimit;

                    if (!msg.Author.IsMod && !msg.Author.IsBroadcaster)
                    {
                        if (StaticData.UserRequestCount[msg.Author.DisplayName] >= requestLimit)
                        {
                            TwitchConnection.Instance.SendChatMessage(
                                msg.Author.DisplayName + " you're making too many requests. Slow down.");
                            return;
                        }
                    }

                    if (AddToQueue(request))
                        StaticData.UserRequestCount[msg.Author.DisplayName]++;
                }
                else
                {
                    if (AddToQueue(request))
                        StaticData.UserRequestCount.Add(msg.Author.DisplayName, 1);
                }

                SongDownloader.Instance.DownloadSong(request);
            });

            // Removed for now, The behavior this has when inside the song is a bit finnicky
            // TwitchIntegrationUi.Instance.RefreshandResetLevelView();
        }

        private bool AddToQueue(Song song)
        {
            if (StaticData.SongQueue.IsSongInQueue(song))
            {
                TwitchConnection.Instance.SendChatMessage("Song already in queue.");
                return false;
            }

            StaticData.SongQueue.AddSongToQueue(song);
            TwitchConnection.Instance.SendChatMessage($"{song.requestedBy} added \"{song.songName}\", uploaded by: {song.authorName} to queue!");
            return true;
        }
    }
}
