using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;
using Logger = TwitchIntegrationPlugin.Misc.Logger;

namespace TwitchIntegrationPlugin.Serializables
{
    [Serializable]
    public class SongQueue
    {
        public List<Song> SongQueueList;

        public SongQueue()
        {
            SongQueueList = new List<Song>();
        }

        public void SaveSongQueue()
        {
            try
            {
                using (FileStream fs = new FileStream("UserData/TwitchIntegrationSavedQueue.json", FileMode.Create, FileAccess.Write))
                {
                    JSONNode arrayNode = new JSONArray();
                    foreach (Song queuedSong in SongQueueList)
                    {
                        arrayNode.Add(queuedSong.ToJsonNode());
                    }

                    byte[] buffer = Encoding.ASCII.GetBytes(arrayNode.ToString());
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush();
                }
            } catch(Exception e)
            {
                Logger.Error("Error saving SongQueue. " + e.Message);
            }
        }

        public void LoadSongQueue()
        {
            try
            {
                using (FileStream fs = new FileStream("UserData/TwitchIntegrationSavedQueue.json", FileMode.OpenOrCreate,
                    FileAccess.ReadWrite))
                {
                    if (fs.Length == 0) return;
                    byte[] readBuffer = new byte[fs.Length];
                    fs.Read(readBuffer, 0, (int)fs.Length);
                    string readString = Encoding.ASCII.GetString(readBuffer);
                    JSONNode node = JSON.Parse(readString);

                    foreach (var songNode in node.Values)
                    {
                        Song song = Song.FromSearchNode(songNode);
                        SongQueueList.Add(song);
                    }
                    fs.Flush();
                }
            } catch(Exception e)
            {
                Logger.Error("Error loading stored SongQueue. " + e.Message);
            }
        }

        public void AddSongToQueue(Song song)
        {
            SongQueueList.Add(song);
        }

        public Song PopQueuedSong()
        {
            Song returnSong = SongQueueList[0];
            SongQueueList.RemoveAt(0);
            try
            {
                if (StaticData.UserRequestCount.ContainsKey(returnSong.requestedBy))
                    StaticData.UserRequestCount[returnSong.requestedBy]--;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return returnSong;
        }

        public Song PeekQueuedSong()
        {
            return SongQueueList[0];
        }

        public void RemoveSongFromQueue(Song song)
        {
            if(StaticData.UserRequestCount.ContainsKey(song.requestedBy))
                StaticData.UserRequestCount[song.requestedBy]--;
            SongQueueList.Remove(song);
            Logger.Log("Removing song from Queue: " + song.songName);
        }

        public bool IsSongInQueue(Song song)
        {
            return SongQueueList.Exists(x => x.id == song.id);
        }

        public bool DoesQueueHaveSongs()
        {
            return SongQueueList.Count != 0;
        }

        public List<Song> GetSongList()
        {
            return SongQueueList;
        }
    }
}
