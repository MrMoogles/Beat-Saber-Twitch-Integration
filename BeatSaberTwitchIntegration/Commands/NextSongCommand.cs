using AsyncTwitch;
using TwitchIntegrationPlugin.Serializables;
using Logger = TwitchIntegrationPlugin.Misc.Logger;

namespace TwitchIntegrationPlugin.Commands
{
    class NextSongCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] { "next" };
        public override void Run(TwitchMessage msg)
        {
            if (!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;
            if (StaticData.SongQueue.GetSongList().Count >= 1)
            {
                Song song = StaticData.SongQueue.SongQueueList[0];
                StaticData.AlreadyPlayed.AddSongToQueue(song);
                string remSong = song.songName;
                StaticData.SongQueue.GetSongList().RemoveAt(0);

                if (StaticData.UserRequestCount.ContainsKey(song.requestedBy))
                    StaticData.UserRequestCount[song.requestedBy]--;

                song = StaticData.SongQueue.SongQueueList[0];

                if (StaticData.SongQueue.GetSongList().Count != 0)
                {
                    if (StaticData.Config.AllowTwitchResponses)
                        TwitchConnection.Instance.SendChatMessage("Removed \"" + remSong + "\" from the queue, next song is \"" + song.songName + "\" requested by " + song.requestedBy);
                }
                else
                {
                    if (StaticData.Config.AllowTwitchResponses)
                        TwitchConnection.Instance.SendChatMessage("Queue is now empty");
                }

                if (StaticData.Config.ContinueQueue)
                    StaticData.SongQueue.SaveSongQueue();
            }
            else
            {if(StaticData.Config.AllowTwitchResponses)
                TwitchConnection.Instance.SendChatMessage("BeatSaber queue was empty.");
            }
        }
    }
}
