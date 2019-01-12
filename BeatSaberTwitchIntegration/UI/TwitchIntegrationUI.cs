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
        private CustomMenu _customMenu;
        private CustomListViewController _customListViewController;
        private LevelListViewController _levelListViewController;
        private BeatmapCharacteristicSO[] _beatmapCharacteristics;
        private BeatmapCharacteristicSO _lastCharacteristic;

        // Buttons for UI
        private Button _twitchButton;
        private Button _nextBtn;
        private Button _randomizeBtn;
        private Button _moveToTopBtn;
        private Button _subChatBtn;
        private Button _clearQueueBtn;
        private Button _banBtn;

        // Variables for future processing
        private static TwitchMessage _internalTwitchMessage;
        private bool _twitchSubOnly = false;
        private string _lvlData = null;

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
            _levelListViewController = Resources.FindObjectsOfTypeAll<LevelListViewController>().FirstOrDefault();

            var trb1Menu = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Twitch Queue Bot", "MainMenu", "TRB1", null);
            trb1Menu.OnToggle += (value) =>
            {
                RequestQueueControllerMain();
            };

            _beatmapCharacteristics = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSO>();
            _lastCharacteristic = _beatmapCharacteristics.First(x => x.characteristicName == "Standard");
            _levelListViewController = Resources.FindObjectsOfTypeAll<LevelListViewController>().FirstOrDefault();
            _levelListViewController.didSelectLevelEvent += _levelListViewController_didSelectLevelEvent;
        }

        private void _levelListViewController_didSelectLevelEvent(LevelListViewController levelListView, IBeatmapLevel selectedLevel)
        {
            _lvlData = selectedLevel.levelID.Substring(0, Math.Min(32, selectedLevel.levelID.Length));
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

        private void RequestQueueControllerMain()
        {
            _customMenu = BeatSaberUI.CreateCustomMenu<CustomMenu>("Test");
            _customListViewController = BeatSaberUI.CreateViewController<CustomListViewController>();

            CreateNextButton();
            CreateRandomizeButton();
            CreateSongToTopButton();
            CreateSubOnlyButton();
            CreateClearQueueButton();
            CreateBanSongButton();

            _customMenu.SetLeftViewController(_customListViewController, true);
            _customMenu.Present();
        }

        private void CreateNextButton()
        {
            _nextBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(30f, 20f), new Vector2(30f, 15f));
            _nextBtn.SetButtonText("Next Song");
            _nextBtn.ToggleWordWrapping(false);
            _nextBtn.onClick.AddListener(delegate ()
            {
                NextSongCommand nsc = new NextSongCommand();
                nsc.Run(_internalTwitchMessage);
                RefreshandResetLevelView();
            });
        }

        private void CreateRandomizeButton()
        {
            _randomizeBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(30f, 0f), new Vector2(30f, 15f));
            _randomizeBtn.SetButtonText("Randomize Queue");
            _randomizeBtn.ToggleWordWrapping(false);
            _randomizeBtn.interactable = StaticData.Config.Randomize;
            _randomizeBtn.onClick.AddListener(delegate ()
            {
                RandomizeCommand rndmc = new RandomizeCommand();
                rndmc.Run(_internalTwitchMessage);
                RefreshandResetLevelView();
            });
        }

        private void CreateSongToTopButton()
        {
            _moveToTopBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(30f, -20f), new Vector2(30f, 15f));
            _moveToTopBtn.SetButtonText("Move Song to Top");
            _moveToTopBtn.ToggleWordWrapping(false);
            _moveToTopBtn.onClick.AddListener(delegate ()
            {
                string keyToMove = "";
                foreach (Song s in StaticData.SongQueue.SongQueueList)
                {
                    if (s.hash.Equals(_lvlData))
                    {
                        keyToMove = s.id;
                        break;
                    }
                }
                _internalTwitchMessage.Content = keyToMove;

                MoveSongsToTopCommand msttc = new MoveSongsToTopCommand();
                msttc.Run(_internalTwitchMessage);

                RefreshandResetLevelView();
            });
        }

        private void CreateSubOnlyButton()
        {
            _subChatBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(-30f, 20f), new Vector2(30f, 15f));
            _subChatBtn.SetButtonText("Sub Chat");
            _subChatBtn.ToggleWordWrapping(false);
            _subChatBtn.onClick.AddListener(delegate ()
            {
                if (!_twitchSubOnly)
                {
                    TwitchConnection.Instance.SendChatMessage("/subscribers");
                    _twitchSubOnly = true;
                }
                else
                {
                    TwitchConnection.Instance.SendChatMessage("/subscribersoff");
                    _twitchSubOnly = false;
                }

            });
        }

        private void CreateClearQueueButton()
        {
            _clearQueueBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(-30f, 0f), new Vector2(30f, 15f));
            _clearQueueBtn.SetButtonText("Clear Queue");
            _clearQueueBtn.ToggleWordWrapping(false);
            _clearQueueBtn.onClick.AddListener(delegate ()
            {
                ClearQueueCommand cqc = new ClearQueueCommand();
                cqc.Run(_internalTwitchMessage);
                _customListViewController.backButtonPressed();
            });
        }

        private void CreateBanSongButton()
        {
            _banBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(-30f, -20f), new Vector2(30f, 15f));
            _banBtn.SetButtonText("Ban Song");
            _banBtn.ToggleWordWrapping(false);
            _banBtn.onClick.AddListener(delegate ()
            {
                string keyToBan = "";
                int rowNum = 0;
                foreach (Song s in StaticData.SongQueue.SongQueueList)
                {
                    if (s.hash.Equals(_lvlData))
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

                RefreshandResetLevelView();
            });
        }

        public void RefreshandResetLevelView()
        {
            SongLoader.Instance.RefreshSongs();
            RequestUIController.Instance.SetLevels(_lastCharacteristic);
        }
    }
}