using AsyncTwitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchIntegrationPlugin.Serializables;
using BeatSaverDownloader.Misc;
using System.IO;
using SimpleJSON;
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
                Serializables.Song song = StaticData.SongQueue.GetSongList()[0];
                StaticData.AlreadyPlayed.AddSongToQueue(song);
                string remSong = song.songName;
                StaticData.SongQueue.GetSongList().RemoveAt(0);

                if (StaticData.SongQueue.GetSongList().Count != 0)
                {
                    TwitchConnection.Instance.SendChatMessage("Removed \"" + remSong + "\" from the queue, next song is \"" + StaticData.SongQueue.GetSongList()[0].songName + "\" requested by " + song.requestedBy);
                }
                else
                    TwitchConnection.Instance.SendChatMessage("Queue is now empty");
            }
            else
                TwitchConnection.Instance.SendChatMessage("BeatSaber queue was empty.");
        }
    }
}
