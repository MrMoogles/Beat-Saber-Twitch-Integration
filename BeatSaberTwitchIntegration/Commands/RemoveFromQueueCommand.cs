using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AsyncTwitch;
using JetBrains.Annotations;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin.Commands
{
    [UsedImplicitly]
    public class RemoveFromQueueCommand :  IrcCommand
    {
        public override string[] CommandAlias => new[] {"remove", "rem", "rm", "yeet"};
        private readonly Regex _songIdrx = new Regex(@"\d+-\d+", RegexOptions.Compiled);

        public override void Run(TwitchMessage msg)
        {
            if(!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;
            List<Song> songList = StaticData.SongQueue.GetSongList();

            string queryString = msg.Content.Remove(0, msg.Content.IndexOf(' ') + 1);
            bool isTextSearch = !_songIdrx.IsMatch(queryString);
            Song remSong = songList.FirstOrDefault(x => isTextSearch ? x.songName == queryString : x.id == queryString);
            StaticData.SongQueue.RemoveSongFromQueue(remSong);

            TwitchConnection.Instance.SendChatMessage($"Song: {remSong.songName}, removed from queue");
        }
    }
}
