using System;
using System.IO;
using System.Text;
using UnityEngine;
using Logger = TwitchIntegrationPlugin.Misc.Logger;

namespace TwitchIntegrationPlugin.Serializables
{
    [Serializable]
    public class Config
    {
        public bool ModOnly;
        public bool SubOnly;
        public int ViewerLimit;
        public int SubLimit;
        public int ModLimit;
        public bool ContinueQueue;
        public bool Randomize;
        public int RandomizeLimit;
        public bool BlockMultiRandomQueue;
        public bool OverrideSongInMultiQueue;
        public bool EnableShadowQueue;
        public bool AllowTwitchResponses;
        public bool DisableModOverride;

        public Config(bool modonly, bool subonly, int viewerlimit, int sublimit, int modLimit, bool continuequeue, bool randomize, int randomizelimit, bool blockMultiRandomQueue, bool overrideSongInMultiQueue, bool enableShadowQueue, bool allowTwitchResponses, bool disableModOverride)
        {
            ModOnly = modonly;
            SubOnly = subonly;
            ViewerLimit = viewerlimit;
            SubLimit = sublimit;
            ModLimit = modLimit;
            ContinueQueue = continuequeue;
            Randomize = randomize;
            BlockMultiRandomQueue = blockMultiRandomQueue;
            OverrideSongInMultiQueue = overrideSongInMultiQueue;
            EnableShadowQueue = enableShadowQueue;
            AllowTwitchResponses = allowTwitchResponses;
            DisableModOverride = disableModOverride;
        }

        public void SaveJson()
        {
            using (FileStream fs = new FileStream("UserData/TwitchIntegrationConfig.json", FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(JsonUtility.ToJson(this, true));
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        public Config LoadFromJson()
        {
            if (File.Exists("UserData/TwitchIntegrationConfig.json"))
            {
                using (FileStream fs = new FileStream("UserData/TwitchIntegrationConfig.json", FileMode.Open, FileAccess.Read))
                {
                    byte[] loadBytes = new byte[fs.Length];
                    fs.Read(loadBytes, 0, (int)fs.Length);
                    Config tempConfig = JsonUtility.FromJson<Config>(Encoding.UTF8.GetString(loadBytes));

                    if (tempConfig.ModLimit == 0)
                    {
                        tempConfig.ModLimit = 5;
                        tempConfig.DisableModOverride = false;
                        tempConfig.AllowTwitchResponses = true;
                    }

                    return tempConfig;
                }
            }
            else
            {
                Config tempConfig = CreateDefaultConfig();
                tempConfig.SaveJson();
                return tempConfig;
            }
        }

        public static Config CreateDefaultConfig()
        {
            return new Config(false, false, 1, 3, 5, true, false, 3, false, false, false, true, false);
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
