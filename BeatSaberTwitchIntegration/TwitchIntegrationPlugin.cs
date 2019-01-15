using IllusionPlugin;
using System;
using TwitchIntegrationPlugin.UI;
using UnityEngine.SceneManagement;
using Logger = TwitchIntegrationPlugin.Misc.Logger;

namespace TwitchIntegrationPlugin
{
    public class TwitchIntegrationPlugin : IPlugin
    {
        public string Name => "Beat Saber Twitch Integration";
        public string Version => "3.0.2";
        private static BeatBotNew _bot;
        
        public void OnApplicationStart()
        {
            StaticData.TwitchMode = false;
            
            _bot = new BeatBotNew();
            SceneManager.sceneLoaded += HandleSceneManagerOnSceneLoaded;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene from, Scene to)
        {
            Logger.Log($"Active scene changed from \"{from.name}\" to \"{to.name}\"");

            if (from.name == "EmptyTransition" && to.name.Contains("Menu"))
            {
                try
                {
                    Logger.Log("Start Loading UI Objects");
                    TwitchIntegrationUi.OnLoad();
                    RequestUIController.Instance.OnLoad();
                }
                catch (Exception e)
                {
                    Logger.Exception("Exception on scene change: " + e);
                }
            }
        }

        private void HandleSceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.Log($"Loaded scene \"{scene.name}\"");
            //if (scene.name != "Menu") return;
        }

        public void OnApplicationQuit()
        {
            if(StaticData.Config.ContinueQueue)
                StaticData.SongQueue.SaveSongQueue();

            StaticData.BanList.SaveBanList();
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
    }
}