using System;
using System.Collections.Generic;
using System.Linq;
using AsyncTwitch;
using CustomUI.BeatSaber;
using CustomUI.GameplaySettings;
using JetBrains.Annotations;
using SongLoaderPlugin;
using SongLoaderPlugin.OverrideClasses;
using TwitchIntegrationPlugin.Commands;
using TwitchIntegrationPlugin.Serializables;
using UnityEngine;
using UnityEngine.UI;
using Logger = TwitchIntegrationPlugin.Misc.Logger;

namespace TwitchIntegrationPlugin.UI
{
    public class TwitchIntegrationUi : MonoBehaviour
    {
        public static TwitchIntegrationUi Instance;

        // UI Controllers/Coordinators
        private MainMenuViewController _mainMenuViewController;
        private RectTransform _mainMenuRectTransform;

        // Buttons for UI
        private Button _twitchButton;

        internal static void OnLoad()
        {
            if (Instance != null)
            {
                Instance.CreateUI();
                return;
            }
            new GameObject("TwitchIntegration UI").AddComponent<TwitchIntegrationUi>();
        }

        [UsedImplicitly]
        private void Awake()
        {
            if (Instance != this)
            {
                DontDestroyOnLoad(this);
                Instance = this;
                CreateUI();
            }
        }

        public void CreateUI()
        {
            try
            {
                _mainMenuViewController = Resources.FindObjectsOfTypeAll<MainMenuViewController>().First();
                _mainMenuRectTransform = _mainMenuViewController.transform as RectTransform;

                CreateTwitchButton();
            }
            catch (Exception e)
            {
                Logger.Exception($"Unable to create UI! Exception: {e}");
            }
        }

        private void CreateTwitchButton()
        {
            _twitchButton = BeatSaberUI.CreateUIButton(_mainMenuRectTransform, "QuitButton", new Vector2(20f, 70f), new Vector2(34f, 10f));
            (_twitchButton.transform as RectTransform).anchorMin = new Vector2(0f, 0f);
            (_twitchButton.transform as RectTransform).anchorMax = new Vector2(0f, 0f);

            _twitchButton.SetButtonText((StaticData.TwitchMode) ? "Twitch Mode: ON" : "Twitch Mode: OFF");
            _twitchButton.onClick.AddListener(delegate ()
            {
                StaticData.TwitchMode = !StaticData.TwitchMode;
                if (StaticData.TwitchMode)
                {
                    _twitchButton.SetButtonText("Twitch Mode: ON");
                }
                else
                {
                    _twitchButton.SetButtonText("Twitch Mode: OFF");
                }
            });
        }
    }
}