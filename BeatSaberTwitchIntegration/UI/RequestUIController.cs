using SongLoaderPlugin;
using System;
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

        private LevelCollectionSO _levelCollection;
        private BeatmapCharacteristicSO[] _beatmapCharacteristics;
        private BeatmapCharacteristicSO _lastCharacteristic;
        private readonly FlowCoordinator _freePlayFlowCoordinator;
        private LevelListViewController _levelListViewController;
        private StandardLevelDetailViewController _detailViewController;

        private Button _requestButton;

        public void OnLoad()
        {
            initialized = false;
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

            initialized = true;
        }

        private void RequestsButtonPressed()
        {
            StaticData.SongQueue.SongQueueList.ForEach(x => {
                if (x.level == null)
                {
                    x.level = SongLoader.CustomLevels.FirstOrDefault(y => (y.customSongInfo.path.Contains(x.id)) || (string.IsNullOrEmpty(x.hash) ? false : y.levelID.StartsWith(x.hash)));
                }
            });

            SetLevels(_lastCharacteristic);

            foreach(Song s in StaticData.SongQueue.SongQueueList)
            {
                Logger.Log(s.songName);
            }
        }

        public void SetLevels(BeatmapCharacteristicSO characteristic)
        {
            LevelSO[] levels = null;
            if (StaticData.SongQueue.SongQueueList != null)
            {
                levels = StaticData.SongQueue.SongQueueList.Where(x => x.level != null && x.level.beatmapCharacteristics.Contains(characteristic)).Select(x => x.level).ToArray();
            }
            else
            {
                levels = _levelCollection.GetLevelsWithBeatmapCharacteristic(characteristic);
            }

            _levelListViewController.SetLevels(levels);
            PopDifficultyAndDetails();
        }

        private void PopDifficultyAndDetails()
        {
            bool isSolo = (_freePlayFlowCoordinator is SoloFreePlayFlowCoordinator);

            if (isSolo)
            {
                SoloFreePlayFlowCoordinator soloCoordinator = _freePlayFlowCoordinator as SoloFreePlayFlowCoordinator;
                int controllers = 0;
                if (soloCoordinator.GetPrivateField<BeatmapDifficultyViewController>("_beatmapDifficultyViewControllerViewController").isInViewControllerHierarchy)
                {
                    controllers++;
                }
                if (soloCoordinator.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController").isInViewControllerHierarchy)
                {
                    controllers++;
                }
                if (controllers > 0)
                {
                    soloCoordinator.InvokePrivateMethod("PopViewControllersFromNavigationController", new object[] { soloCoordinator.GetPrivateField<DismissableNavigationController>("_navigationController"), controllers, null, false });
                }
            }
            else
            {
                PartyFreePlayFlowCoordinator partyCoordinator = _freePlayFlowCoordinator as PartyFreePlayFlowCoordinator;
                int controllers = 0;
                if (partyCoordinator.GetPrivateField<BeatmapDifficultyViewController>("_beatmapDifficultyViewControllerViewController").isInViewControllerHierarchy)
                {
                    controllers++;
                }
                if (partyCoordinator.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController").isInViewControllerHierarchy)
                {
                    controllers++;
                }
                if (controllers > 0)
                {
                    partyCoordinator.InvokePrivateMethod("PopViewControllersFromNavigationController", new object[] { partyCoordinator.GetPrivateField<DismissableNavigationController>("_navigationController"), controllers, null, false });
                }
            }
        }
    }
}
