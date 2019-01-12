using AsyncTwitch;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Logger = TwitchIntegrationPlugin.Misc.Logger;

namespace TwitchIntegrationPlugin.Serializables
{
    [Serializable]
    public class BanList
    {
        [SerializeField]
        private List<string> _bannedSongs;
        private readonly Regex _songIdValidationRegex = new Regex(@"^[0-9\-]+$");

        public BanList()
        {
            _bannedSongs = new List<string>();
        }

        public void AddToBanList(string songId)
        {
            if (!_songIdValidationRegex.IsMatch(songId)) throw new FormatException("songId is not in the valid format.");
            if (songId.Contains("-"))
            {
                songId = songId.Split('-')[0];
            }

            _bannedSongs.Add(songId);
        }

        public void RemoveFromBanList(string songId)
        {
            if (!_songIdValidationRegex.IsMatch(songId)) throw new FormatException("songId is not in the valid format");
            if (songId.Contains("-"))
            {
                songId = songId.Split('-')[0];
            }

            _bannedSongs.Remove(songId);
        }

        public bool IsBanned(string songId)
        {
            if (!_songIdValidationRegex.IsMatch(songId)) throw new FormatException("songId is not in the valid format.");
            if (songId.Contains("-"))
            {
                songId = songId.Split('-')[0];
            }

            return _bannedSongs.Contains(songId);
        }

        public void SaveBanList()
        {
            try
            {
                using (FileStream fs = new FileStream("UserData/TwitchIntegrationBans.json", FileMode.OpenOrCreate,
                    FileAccess.ReadWrite))
                {
                    if (File.Exists("UserData/TwitchIntegrationBans.json"))
                    {
                        byte[] readBuffer = new byte[fs.Length];
                        fs.Read(readBuffer, 0, readBuffer.Length);

                        // Added a check to Length to avoid the mod hanging when trying to read an Empty File
                        if (fs.Length != 0)
                        {
                            List<string> tempList = new List<string>();
                            string bannedSongString = Encoding.ASCII.GetString(readBuffer);
                            tempList = JsonUtility.FromJson<BanList>(bannedSongString).GetBanList(); ;
                            foreach (string bannedSong in tempList)
                            {
                                if (!_bannedSongs.Contains(bannedSong))
                                {
                                    _bannedSongs.Add(bannedSong);
                                }
                            }
                        }
                    }

                    // We Set length to zero to wipe the file clean.
                    // There was an issue with duplicates being written and I couldn't debug correctly at 2AM
                    fs.SetLength(0);
                    byte[] buffer = Encoding.ASCII.GetBytes(JsonUtility.ToJson(this, true));
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush();
                }
            } catch (Exception e)
            {
                Logger.Error("Error saving Banlist. " + e.Message);
            }
        }

        public void LoadBanList(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite))
                {
                    //Lets reset our list before we load.
                    _bannedSongs = new List<string>();

                    //It didn't exist or there are no bans.
                    if (fs.Length == 0) return;
                    byte[] bannedSongBytes = new byte[fs.Length];

                    //This imposes a limit of 2,147,483,647 characters. Enough to not worry hopefully.
                    fs.Read(bannedSongBytes, 0, (int)fs.Length);

                    string bannedSongString = Encoding.ASCII.GetString(bannedSongBytes);
                    _bannedSongs = JsonUtility.FromJson<BanList>(bannedSongString).GetBanList();
                    fs.Flush();
                }
            } catch(Exception e)
            {
                Logger.Error("Error loading Banlist(" + filePath + "). " + e.Message);
            }
        }

        public List<string> GetBanList()
        {
            return _bannedSongs;
        }
    }
}
