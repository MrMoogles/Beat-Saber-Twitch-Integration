using AsyncTwitch;
using System;
using System.Text.RegularExpressions;
using TwitchIntegrationPlugin.Misc;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin.Commands
{
    class UnbanCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] { "unban", "unblock" };
        private readonly Regex _songIdrx = new Regex(@"^[0-9\-]+$", RegexOptions.Compiled);

        public override void Run(TwitchMessage msg)
        {
            if (!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;

            string queryString = msg.Content.Remove(0, msg.Content.IndexOf(' ') + 1);
            bool isTextSearch = !_songIdrx.IsMatch(queryString);

            if (StaticData.BanList.GetBanList().Count >= 1)
            {
                //QueuedSong request = ApiConnection.GetSongFromBeatSaver(isTextSearch, queryString, "unbanboi");
                SongDownloader.Instance.RequestSongByQuery(queryString, isTextSearch, msg.Author.DisplayName, request =>
                {
                    if (String.IsNullOrEmpty(request.hash) || String.IsNullOrEmpty(request.id))
                    {
                        TwitchConnection.Instance.SendChatMessage("Could not locate song on beatsaver.");
                        return;
                    }
                    for (int i = 0; i < StaticData.BanList.GetBanList().Count; i++)
                    {
                        if (StaticData.BanList.IsBanned(request.id))
                        {
                            StaticData.BanList.GetBanList().RemoveAt(i);
                            TwitchConnection.Instance.SendChatMessage("Removed \"" + request.songName + "\" from the banlist");
                        }
                    }
                });                
            }
        }
    }
}
