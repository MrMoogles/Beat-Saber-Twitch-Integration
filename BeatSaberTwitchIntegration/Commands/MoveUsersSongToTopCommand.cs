using AsyncTwitch;

namespace TwitchIntegrationPlugin.Commands
{
    class MoveUsersSongToTopCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] { "mutt" };
        public override void Run(TwitchMessage msg)
        {
            if (!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;
            string queryString = msg.Content.Remove(0, msg.Content.IndexOf(' ') + 1);
            for (int i = 0; i < StaticData.SongQueue.GetSongList().Count; i++)
            {
                var song = StaticData.SongQueue.GetSongList()[i];
                if (song.requestedBy.Equals(queryString) || song.id.Contains(queryString))
                {
                    StaticData.SongQueue.GetSongList().RemoveAt(i);
                    StaticData.SongQueue.GetSongList().Insert(0, song);
                    TwitchConnection.Instance.SendChatMessage("Moved \"" + song.songName + "\" requested by " + song.requestedBy + " to top of queue.");
                }
            }
        }
    }
}
