using AsyncTwitch;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TwitchIntegrationPlugin.Misc;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin.Commands
{
    class ReplaceCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] { "replace" };
        private readonly Regex _songIdrx = new Regex(@"^[0-9\-]+$", RegexOptions.Compiled);

        public override void Run(TwitchMessage msg)
        {
            string queryString = msg.Content.Remove(0, msg.Content.IndexOf(' ') + 1);
            //bool isTextSearch = !_songIdrx.IsMatch(queryString);

            if (!queryString.Contains(":")) return;

            string[] splitReplaced = queryString.Split(':');
            string[] splitId1 = splitReplaced[0].Split('-');
            string[] splitId2 = splitReplaced[1].Split('-');

            for(int i = 0; i < StaticData.SongQueue.SongQueueList.Count; i++)
            {
                Song s = StaticData.SongQueue.SongQueueList[i];

                if (s.id.Contains(splitReplaced[0]) || s.id.Contains(splitId1[0]))
                {
                    if (s.requestedBy.Equals(msg.Author.DisplayName))
                    {
                        SongDownloader.Instance.RequestSongByQuery(splitReplaced[1], false, msg.Author.DisplayName, request =>
                        {
                            // Make sure request returns with valid information before going further
                            if (String.IsNullOrEmpty(request.hash) || String.IsNullOrEmpty(request.id))
                            {
                                if (StaticData.Config.AllowTwitchResponses)
                                    TwitchConnection.Instance.SendChatMessage("Invalid Request.");
                                return;
                            }

                            // Check to see if song exists on Ban List
                            if (StaticData.BanList.IsBanned(request.id))
                            {
                                if (StaticData.Config.AllowTwitchResponses)
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
                                foreach (Song s2 in StaticData.AlreadyPlayed.SongQueueList)
                                {
                                    if (s2.songName.Equals(request.songName))
                                    {
                                        if (StaticData.Config.AllowTwitchResponses)
                                            TwitchConnection.Instance.SendChatMessage(request.requestedBy + ", \"" + request.songName + "\" was chosen for previous queue.");
                                        return;
                                    }
                                }
                            }

                            // Since we don't care about User Limits here, we go ahead and just Replace\
                            TwitchConnection.Instance.SendChatMessage(s.songName);
                            TwitchConnection.Instance.SendChatMessage(request.songName);
                            ReplaceInQueue(s, request);
                        });
                    }
                }
            }
        }

        private bool ReplaceInQueue(Song song1, Song song2)
        {
            if (StaticData.SongQueue.IsSongInQueue(song2))
            {
                if (StaticData.Config.AllowTwitchResponses)
                    TwitchConnection.Instance.SendChatMessage("Song already in queue.");
                return false;
            }

            StaticData.SongQueue.SongQueueList[StaticData.SongQueue.SongQueueList.FindIndex(ind => ind.Equals(song1))] = song2;
            song2.songQueueState = SongQueueState.Queued;
            if (StaticData.SongQueue.SongQueueList.Count(x => x.songQueueState == SongQueueState.Downloading) < 3)
            {
                SongDownloader.Instance.DownloadSong(song2);
            }

            if (StaticData.Config.AllowTwitchResponses)
                TwitchConnection.Instance.SendChatMessage($"{song1.requestedBy} replaced \"{song1.songName}\" with \"{song2.songName}\", uploaded by: {song2.authorName} to queue!");

            if (StaticData.Config.ContinueQueue)
                StaticData.SongQueue.SaveSongQueue();

            return true;
        }
    }
}
