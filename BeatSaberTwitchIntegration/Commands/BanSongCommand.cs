﻿using System;
using System.Text.RegularExpressions;
using AsyncTwitch;
using TwitchIntegrationPlugin.Misc;

namespace TwitchIntegrationPlugin.Commands
{
    public class BanSongCommand : IrcCommand
    {
        public override string[] CommandAlias => new []{"ban", "wap", "block"};
        private readonly Regex _songIdrx = new Regex(@"^[0-9\-]+$", RegexOptions.Compiled);

        public override void Run(TwitchMessage msg)
        {
            if (!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;

            string queryString = msg.Content.Remove(0, msg.Content.IndexOf(' ') + 1);
            bool isTextSearch = !_songIdrx.IsMatch(queryString);

            Console.WriteLine(queryString);
            Console.WriteLine(isTextSearch.ToString());

            //QueuedSong request = ApiConnection.GetSongFromBeatSaver(isTextSearch, queryString, "banboi");
            SongDownloader.Instance.RequestSongByQuery(queryString, isTextSearch, msg.Author.DisplayName, request =>
            {
                if (String.IsNullOrEmpty(request.hash) || String.IsNullOrEmpty(request.id))
                {
                    TwitchConnection.Instance.SendChatMessage("Could not locate song on beatsaver.");
                    return;
                }

                if (StaticData.BanList.IsBanned(request.id))
                {
                    TwitchConnection.Instance.SendChatMessage("Song is already banned.");
                    return;
                }

                StaticData.BanList.AddToBanList(request.id);
                StaticData.BanList.SaveBanList();
                TwitchConnection.Instance.SendChatMessage($"\"{request.songName}\" was banned.");
            });
        }
    }
}
