using AsyncTwitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin.Commands
{
    class RandomizeCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] { "randomize" };
        public override void Run(TwitchMessage msg)
        {
            if (!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;

            if (StaticData.Config.Randomize && (StaticData.Config.RandomizeLimit > 0))
            {
                Random randomizer = new Random();
                List<Song> RandomizedList = new List<Song>();

                for (int i = 0;
                (i < StaticData.Config.RandomizeLimit) && (StaticData.SongQueue.GetSongList().Count > 0); i++)
                {
                    int randomIndex = randomizer.Next(StaticData.SongQueue.GetSongList().Count);
                    Song randomSong = StaticData.SongQueue.SongQueueList[randomIndex];

                    RandomizedList.Add(randomSong);
                    StaticData.SongQueue.SongQueueList.RemoveAt(randomIndex);
                }

                StaticData.SongQueue.SongQueueList = new List<Song>();
                StaticData.SongQueue.SongQueueList.AddRange(RandomizedList);
                RandomizedList.Clear();
                if (StaticData.Config.AllowTwitchResponses)
                    TwitchConnection.Instance.SendChatMessage(StaticData.Config.RandomizeLimit + " songs were randomly chosen from queue!");

                PrintQueueCommand pqc = new PrintQueueCommand();
                TwitchMessage queueMessage = new TwitchMessage
                {
                    Content = "queue",
                    Author = msg.Author
                };
                pqc.Run(queueMessage);

                if (StaticData.Config.BlockMultiRandomQueue)
                {
                    foreach (Song bsong in StaticData.SongQueue.SongQueueList)
                    {
                        StaticData.UserPickedByRandomize.Add(bsong.requestedBy);
                    }
                }
            }
        }
    }
}
