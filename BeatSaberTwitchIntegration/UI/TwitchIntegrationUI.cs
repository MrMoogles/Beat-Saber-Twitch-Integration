using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsyncTwitch;
using CustomUI.BeatSaber;
using CustomUI.GameplaySettings;
using CustomUI.MenuButton;
using CustomUI.Settings;
using CustomUI.Utilities;
using HMUI;
using JetBrains.Annotations;
using SongLoaderPlugin;
using SongLoaderPlugin.OverrideClasses;
using TMPro;
using TwitchIntegrationPlugin.Commands;
using TwitchIntegrationPlugin.Serializables;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private LevelListViewController _levelListViewController;
        private BeatmapCharacteristicSO[] _beatmapCharacteristics;
        private BeatmapCharacteristicSO _lastCharacteristic;

        // Buttons for UI
        private Button _twitchButton;

        // Variables for future processing
        private static TwitchMessage _internalTwitchMessage;
        private bool _twitchSubOnly = false;

        internal static void OnLoad()
        {
            if (Instance != null)
            {
                Instance.CreateUI();
                return;
            }
            new GameObject("TwitchIntegration UI").AddComponent<TwitchIntegrationUi>();
            _internalTwitchMessage = new TwitchMessage();
            _internalTwitchMessage.Author.IsBroadcaster = true;
            _internalTwitchMessage.Author.IsMod = true;
        }

        [UsedImplicitly]
        private void Awake()
        {
            if (Instance != this)
            {
                DontDestroyOnLoad(this);
                Instance = this;
                SongLoader.SongsLoadedEvent += SongsLoaded;
                CreateUI();
            }
        }

        public void SongsLoaded(SongLoader sender, List<CustomLevel> levels)
        {
            if (_twitchButton != null)
            {
                _twitchButton.interactable = true;
            }
            else
            {
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
                CreateSubMenuItems();

                _twitchButton.interactable = SongLoader.AreSongsLoaded;
            }
            catch (Exception e)
            {
                Logger.Exception($"Unable to create UI! Exception: {e}");
            }
        }

        private void CreateSubMenuItems()
        {
            var trb1Menu = GameplaySettingsUI.CreateSubmenuOption(GameplaySettingsPanels.PlayerSettingsRight, "Twitch Queue Bot", "MainMenu", "TRB1", "Twitch Queue Bot Options", null);

            CreateNextButton();
            CreateRandomizeButton();
            //CreateSongToTopButton();
            CreateSubOnlyButton();
            CreateClearQueueButton();
            CreateBanSongButton();

            _beatmapCharacteristics = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSO>();
            _lastCharacteristic = _beatmapCharacteristics.First(x => x.characteristicName == "Standard");
            _levelListViewController = Resources.FindObjectsOfTypeAll<LevelListViewController>().FirstOrDefault();
            _levelListViewController.didSelectLevelEvent += _levelListViewController_didSelectLevelEvent;
        }

        string lvlData = null;
        private void _levelListViewController_didSelectLevelEvent(LevelListViewController levelListView, IBeatmapLevel selectedLevel)
        {
            lvlData = selectedLevel.levelID.Substring(0, Math.Min(32, selectedLevel.levelID.Length));
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

        private void CreateNextButton()
        {
            var nextSongOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Next Song", "TRB1", "", null);
            nextSongOption.OnToggle += (value) =>
            {
                NextSongCommand nsc = new NextSongCommand();
                nsc.Run(_internalTwitchMessage);
            };

        }

        private void CreateRandomizeButton()
        {
            if (StaticData.Config.Randomize)
            {
                var randomizeOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Randomize List", "TRB1", "", null);
                randomizeOption.OnToggle += (value) =>
                {
                    RandomizeCommand rndmc = new RandomizeCommand();
                    rndmc.Run(_internalTwitchMessage);
                };
            }
        }

        //private void CreateSongToTopButton()
        //{
        //    _randomizeBtn = _customListViewController.CreateUIButton("QuitButton", new Vector2(-50f, 6f), new Vector2(20f, 6f));
        //    _randomizeBtn.SetButtonText("Move Song to Top");
        //    _randomizeBtn.SetButtonTextSize(2.5f);
        //    _randomizeBtn.ToggleWordWrapping(false);
        //    _randomizeBtn.interactable = StaticData.Config.Randomize;
        //    _randomizeBtn.onClick.AddListener(delegate ()
        //    {
        //        MoveSongsToTopCommand msttc = new MoveSongsToTopCommand();
        //        _internalTwitchMessage.Content = _selectedSongID;
        //        msttc.Run(_internalTwitchMessage);
        //        //_customListViewController.Data = RefreshEntireQueue();
        //        _customListViewController._customListTableView.ReloadData();
        //    });
        //}

        private void CreateSubOnlyButton()
        {
            var subOnlyOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Sub Only Chat", "TRB1", "", null);
            subOnlyOption.GetValue = _twitchSubOnly;
            subOnlyOption.OnToggle += (bool value) =>
            {
                if (!_twitchSubOnly)
                {
                    TwitchConnection.Instance.SendChatMessage("/subscribers");
                    _twitchSubOnly = value;
                }
                else
                {
                    TwitchConnection.Instance.SendChatMessage("/subscribersoff");
                    _twitchSubOnly = value;
                }
            };
        }

        private void CreateClearQueueButton()
        {
            var clearAllOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Clear All Songs", "TRB1", "", null);
            clearAllOption.OnToggle += (value) =>
            {
                ClearQueueCommand cqc = new ClearQueueCommand();
                cqc.Run(_internalTwitchMessage);
            };
        }

        private void CreateBanSongButton()
        {
            var banSongOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Ban Currently selected Song", "TRB1", "", null);
            banSongOption.OnToggle += (value) =>
            {
                string keyToBan = "";
                int rowNum = 0;
                foreach(Song s in StaticData.SongQueue.SongQueueList)
                {
                    if (s.hash.Equals(lvlData))
                    {
                        keyToBan = s.id;
                        break;
                    }
                    rowNum++;
                }

                _internalTwitchMessage.Content = keyToBan;
                StaticData.SongQueue.SongQueueList.RemoveAt(rowNum);
                BanSongCommand bsc = new BanSongCommand();
                bsc.Run(_internalTwitchMessage);

                SongLoader.Instance.RefreshSongs();
                RequestUIController.Instance.SetLevels(_lastCharacteristic);
            };
        }
    }
}