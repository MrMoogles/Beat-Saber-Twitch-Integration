using System.Collections.Generic;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin
{
    public static class StaticData
    {
        public static bool TwitchMode = false;
        public static SongQueue SongQueue = new SongQueue();
        public static SongQueue AlreadyPlayed = new SongQueue();
        public static BanList BanList = new BanList();
        public static BanList ShadowBanList = new BanList();
        public static List<string> UserPickedByRandomize = new List<string>();
        public static Dictionary<string, int> UserRequestCount = new Dictionary<string, int>();
        public static Config Config = new Config(false, false, 1, 3, 5, true, false, 3, false, false, false, true, false);
        public static string BeatSaverURL = "https://beatsaver.com";
    }
}
