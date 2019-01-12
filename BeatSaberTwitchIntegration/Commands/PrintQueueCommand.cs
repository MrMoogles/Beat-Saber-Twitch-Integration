using System.Collections.Generic;
using System.Linq;
using AsyncTwitch;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin.Commands
{
    public class PrintQueueCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] {"queue", "songlist", "q", "gimmesongies", "awooque", "showmehell"};
        public override void Run(TwitchMessage msg)
        {
            List<Song> songList = StaticData.SongQueue.GetSongList();

            if (songList.Count() >= 1)
            {
                string msgString = "[Current Songs in Queue]: ";
                foreach (Song song in songList)
                {
                    if (msgString.Length + song.songName.Length + 2 > 496)
                    {
                        TwitchConnection.Instance.SendChatMessage(msgString);
                        msgString = "";
                    }
                    msgString += song.songName + ", ";
                }

                if (msgString.Length > 0)
                {
                    TwitchConnection.Instance.SendChatMessage(msgString);
                }
            } else
                TwitchConnection.Instance.SendChatMessage("No Songs in Queue Currently");
        }
    }
}
