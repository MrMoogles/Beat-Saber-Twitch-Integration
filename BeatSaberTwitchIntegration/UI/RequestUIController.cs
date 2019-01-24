using SongLoaderPlugin;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SongLoaderPlugin.OverrideClasses;
using VRUI;
using CustomUI.BeatSaber;
using Logger = TwitchIntegrationPlugin.Misc.Logger;
using TwitchIntegrationPlugin.Serializables;
using AsyncTwitch;
using TwitchIntegrationPlugin.Commands;
using System;

namespace TwitchIntegrationPlugin.UI
{
    // Could not have done this without reviewing andruzzzhka's code, there are lots of modified features from their original code
    // Visit the original Repository here: https://github.com/andruzzzhka/BeatSaverDownloader
    class RequestUIController : MonoBehaviour
    {
        public bool initialized = false;

        private static RequestUIController _instance = null;
        public static RequestUIController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = new GameObject("RequestUIController").AddComponent<RequestUIController>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        // UI Controllers/Coordinators
        private LevelCollectionSO _levelCollection;
        private BeatmapCharacteristicSO[] _beatmapCharacteristics;
        private BeatmapCharacteristicSO _lastCharacteristic;
        //private readonly FlowCoordinator _freePlayFlowCoordinator;
        private LevelListViewController _levelListViewController;
        private StandardLevelDetailViewController _detailViewController;
        private CustomMenu _customMenu;
        private CustomListViewController _customListViewController;

        // Buttons for UI
        private Button _nextBtn;
        private Button _randomizeBtn;
        private Button _moveToTopBtn;
        private Button _subChatBtn;
        private Button _clearQueueBtn;
        private Button _refreshButton;
        private Button _banBtn;
        private Button _requestButton;

        // Variables for future processing
        private static TwitchMessage _internalTwitchMessage;
        private bool _twitchSubOnly = false;
        private string _lvlData = null;

        public void OnLoad()
        {
            initialized = false;
            _internalTwitchMessage = new TwitchMessage();
            _internalTwitchMessage.Author.IsBroadcaster = true;
            _internalTwitchMessage.Author.IsMod = true;
            _internalTwitchMessage.Author.DisplayName = "QueueBot";
            SetupUI();
        }

        private void SetupUI()
        {
            if (initialized) return;
            _beatmapCharacteristics = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSO>();
            _lastCharacteristic = _beatmapCharacteristics.First(x => x.characteristicName == "Standard");

            Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSelectionViewController>().First().didSelectBeatmapCharacteristicEvent += (BeatmapCharacteristicSelectionViewController sender, BeatmapCharacteristicSO selected) => { _lastCharacteristic = selected; };
            if (SongLoader.AreSongsLoaded)
            {
                _levelCollection = SongLoader.CustomLevelCollectionSO;
            }
            else
            {
                SongLoader.SongsLoadedEvent += (SongLoader sender, List<CustomLevel> levels) => 
                {
                    _levelCollection = SongLoader.CustomLevelCollectionSO;
                };
            }

            _levelListViewController = Resources.FindObjectsOfTypeAll<LevelListViewController>().FirstOrDefault();

            RectTransform _tableViewRectTransform = _levelListViewController.GetComponentsInChildren<RectTransform>().First(x => x.name == "TableViewContainer");
            _tableViewRectTransform.sizeDelta = new Vector2(0f, -35f);
            //_tableViewRectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            RectTransform _pageUp = _tableViewRectTransform.GetComponentsInChildren<RectTransform>(true).First(x => x.name == "PageUpButton");
            _pageUp.anchoredPosition = new Vector2(0f, -1f);

            RectTransform _pageDown = _tableViewRectTransform.GetComponentsInChildren<RectTransform>(true).First(x => x.name == "PageDownButton");
            _pageDown.anchoredPosition = new Vector2(0f, 1f);

            _requestButton = _levelListViewController.CreateUIButton("CreditsButton", new Vector2(0, 30.25f), new Vector2(20f, 6f), RequestsButtonPressed, "Requests");
            _requestButton.SetButtonTextSize(3f);
            _requestButton.ToggleWordWrapping(false);

            _detailViewController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First(x => x.name == "StandardLevelDetailViewController");

            //based on https://github.com/halsafar/BeatSaberSongBrowser/blob/master/SongBrowserPlugin/UI/Browser/SongBrowserUI.cs#L192
            var statsPanel = _detailViewController.GetComponentsInChildren<CanvasRenderer>(true).First(x => x.name == "LevelParamsPanel");
            var statTransforms = statsPanel.GetComponentsInChildren<RectTransform>();
            var valueTexts = statsPanel.GetComponentsInChildren<TextMeshProUGUI>().Where(x => x.name == "ValueText").ToList();

            foreach (RectTransform r in statTransforms)
            {
                if (r.name == "Separator")
                {
                    continue;
                }
                r.sizeDelta = new Vector2(r.sizeDelta.x * 0.85f, r.sizeDelta.y * 0.85f);
            }

            _levelListViewController.didSelectLevelEvent += _levelListViewController_didSelectLevelEvent;
            initialized = true;
        }
  
        private void RequestsButtonPressed()
        {
            //foreach(Song s in StaticData.SongQueue.SongQueueList)
            //{
            //    Logger.Log("Song name: " + s.songName + ", Level Info: " + s.level.levelID);
            //}

            RequestQueueControllerMain();
            RefreshSongList();
            SetLevels(_lastCharacteristic);
        }

        public void SetLevels(BeatmapCharacteristicSO characteristic)
        {
            LevelSO[] levels = null;
            if (StaticData.SongQueue.SongQueueList != null)
            {
                levels = StaticData.SongQueue.SongQueueList.Where(x => x.level != null).Select(x => x.level).ToArray();
            }
            else
            {
                levels = _levelCollection.GetLevelsWithBeatmapCharacteristic(characteristic);
            }

            _levelListViewController.SetLevels(levels);
        }


        private void _levelListViewController_didSelectLevelEvent(LevelListViewController levelListView, IBeatmapLevel selectedLevel)
        {
            _lvlData = selectedLevel.levelID.Substring(0, Math.Min(32, selectedLevel.levelID.Length));
        }

        private void RequestQueueControllerMain()
        {
            _customMenu = BeatSaberUI.CreateCustomMenu<CustomMenu>("QueueButtonOptions");
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
            _nextBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(5f, 20f), new Vector2(30f, 15f));
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
            _randomizeBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(5f, 0f), new Vector2(30f, 15f));
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
            _moveToTopBtn = _customListViewController.CreateUIButton("CreditsButton", new Vector2(5f, -20f), new Vector2(30f, 15f));
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
                RefreshandResetLevelView();
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

        private void RefreshandResetLevelView()
        {
            SetLevels(_lastCharacteristic);
        }

        private void RefreshSongList()
        {
            StaticData.SongQueue.SongQueueList.ForEach(x => {
                foreach (CustomLevel z in SongLoader.CustomLevels)
                {
                    Logger.Log("Custom Level Data: " + z);
                    string customlvl = z.levelID.Substring(0, 32);
                    if (x.hash.Equals(customlvl))
                    {
                        Logger.Log("Song: " + x.songName + "Hash Checks: [" + x.hash + "] [" + customlvl + "]");
                        x.level = z;
                    }
                }
            });
            SongLoader.Instance.RefreshSongs(false);
        }
    }
}