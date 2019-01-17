using System.Collections.Generic;
using AsyncTwitch;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin.Commands
{
    public class ClearQueueCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] {"clear", "annhilate", "purge", "avadacadabra", "smile"};
        public override void Run(TwitchMessage msg)
        {
            if (!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;
            StaticData.SongQueue.SongQueueList = new List<Song>();
            StaticData.UserRequestCount = new Dictionary<string, int>();
            if (StaticData.Config.AllowTwitchResponses)
                TwitchConnection.Instance.SendChatMessage("Queue cleared!");

            if (StaticData.Config.ContinueQueue)
                StaticData.SongQueue.SaveSongQueue();
        }
    }
}
