using AsyncTwitch;
using System;
using System.Collections.Generic;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin.Commands
{
    class MoveSongsToTopCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] { "mtt", "tothetop", "nukethestreamer"};
        public override void Run(TwitchMessage msg)
        {
            if (!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;

            string queryString = msg.Content.Remove(0, msg.Content.IndexOf(' ') + 1);
            if (!String.IsNullOrEmpty(queryString))
            {
                for (int i = 0; i < StaticData.SongQueue.GetSongList().Count; i++)
                {
                    Song song = StaticData.SongQueue.SongQueueList[i];
                    if (song.id.Contains(queryString) || song.requestedBy.Equals(queryString))
                    {
                        StaticData.SongQueue.GetSongList().RemoveAt(i);
                        StaticData.SongQueue.GetSongList().Insert(0, song);
                        TwitchConnection.Instance.SendChatMessage("Moved \"" + song.songName + "\" requested by " + song.requestedBy + " to top of queue.");
                    }
                }
            }
            else
            {
                List<Song> temporaryList = new List<Song>();
                List<Song> currentSongList = StaticData.SongQueue.GetSongList();

                int j = 0;
                for (int i = 0; i < currentSongList.Count; i++)
                {
                    var song = currentSongList[i];
                    if (!song.requestedBy.Equals(msg.Author.IsBroadcaster))
                    {
                        temporaryList.Insert(j, song);
                        j++;
                    }
                    else
                        temporaryList.Add(song);
                }

                StaticData.SongQueue.SongQueueList.Clear();
                StaticData.SongQueue.SongQueueList.AddRange(temporaryList);
                temporaryList.Clear();

                TwitchConnection.Instance.SendChatMessage("Moved all Non-Broadcaster Songs to top of queue.");
            }
        }
    }
}
