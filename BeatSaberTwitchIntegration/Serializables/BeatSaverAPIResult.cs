using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;

namespace TwitchIntegrationPlugin.Serializables
{
    public enum SongQueueState { Queued, Downloading, Downloaded, Error };

    [Serializable]
    public class Song
    {
        public string id;
        public string beatname;
        public string ownerid;
        public string downloads;
        public string upvotes;
        public string plays;
        public string beattext;
        public string uploadtime;
        public string songName;
        public string songSubName;
        public string authorName;
        public string beatsPerMinute;
        public string downvotes;
        public string coverUrl;
        public string downloadUrl;
        public string img;
        public string hash;
        public string requestedBy;
        public LevelSO level;

        public string path;

        public SongQueueState songQueueState = SongQueueState.Queued;

        public float downloadingProgress = 0f;

        public Song()
        {
        }
 
        public Song(JSONNode jsonNode, string userRequesting)
        {
            id = jsonNode["key"];
            beatname = jsonNode["name"];
            ownerid = jsonNode["uploaderId"];
            downloads = jsonNode["downloadCount"];
            upvotes = jsonNode["upVotes"];
            downvotes = jsonNode["downVotes"];
            plays = jsonNode["playedCount"];
            beattext = jsonNode["description"];
            uploadtime = jsonNode["createdAt"];
            songName = jsonNode["songName"];
            songSubName = jsonNode["songSubName"];
            authorName = jsonNode["authorName"];
            beatsPerMinute = jsonNode["bpm"];
            coverUrl = jsonNode["coverUrl"];
            downloadUrl = jsonNode["downloadUrl"];
            hash = jsonNode["hashMd5"];
            hash = hash.ToUpper();
            requestedBy = userRequesting;
            level = null;
        }

        public JSONNode ToJsonNode()
        {
            JSONNode node = new JSONObject();
            node["songname"] = songName;
            node["beatname"] = beatname;
            node["authname"] = authorName;
            node["id"] = id;
            node["bpm"] = beatsPerMinute;
            node["songsubname"] = songSubName;
            node["downloadurl"] = downloadUrl;
            node["coverurl"] = coverUrl;
            node["songhash"] = hash;
            node["requestedby"] = requestedBy;
            return node;
        }

        public static Song FromSearchNode(JSONNode mainNode)
        {
            Song buffer = new Song
            {
                id = mainNode["id"],
                beatname = mainNode["beatname"],
                songName = mainNode["songname"],
                songSubName = mainNode["songsubname"],
                authorName = mainNode["authname"],
                beatsPerMinute = mainNode["bpm"],
                coverUrl = mainNode["coverurl"],
                downloadUrl = mainNode["downloadurl"],
                hash = mainNode["songhash"],
                requestedBy = mainNode["requestedby"]
            };

            return buffer;
        }

        public bool Compare(Song compareTo)
        {
            if (compareTo != null && songName == compareTo.songName)
            {
                return (songSubName == compareTo.songSubName && authorName == compareTo.authorName);
            }
            else
            {
                return false;
            }
        }
    }

    [Serializable]
    public class RootObject
    {
        public Song[] songs;
    }
}