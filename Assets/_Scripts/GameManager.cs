using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        #region Fields

        public static GameManager Instance { get;private set; }
        public LocalData CurrentLocalData { get; set; }
        public bool GameOver { get; set; }
        
        [SerializeField] private GridManager GridManager;
        [SerializeField] private RectTransform ControlButtons, MenuPanel, GameOverPanel;
        [SerializeField] private SwipeController Controller;
        [SerializeField] private Text ScoreText, BestScoreText;
        [SerializeField] private MessageBoxController messageBoxController;
        
        private bool FirstGameStarted;

        #endregion

        private void Awake()
        {
            Instance = this;

            LocalDataManager.ReadLocalData(data => CurrentLocalData = data ?? new LocalData());

            if (!PlayerPrefs.HasKey("GameWasPlayed")) PlayerPrefs.SetInt("DataWasReadFromCloud", 0);
            GPGSManager.Initialize();
            GPGSManager.SignIn( InitializeSignInOrOutGPGSButtonColor, dataName, OnDataRead);
        }

        private void Start()
        {
            SetTheme(PlayerPrefs.GetInt("theme"));
        }

        #region Game managment

        private void Update()
        {
            if (GameOver)
            {
                GameOverMethod();
                GameOver = false;
            }

            if (GridManager.isActive)
            {
                if (Controller.SwipeRight || Input.GetKeyUp(KeyCode.RightArrow)) GridManager.Turn(0);
                if (Controller.SwipeUp || Input.GetKeyUp(KeyCode.UpArrow)) GridManager.Turn(1);
                if (Controller.SwipeLeft || Input.GetKeyUp(KeyCode.LeftArrow)) GridManager.Turn(2);
                if (Controller.SwipeDown || Input.GetKeyUp(KeyCode.DownArrow)) GridManager.Turn(3);
            }

            Controller.Enabled = GridManager.isActive;
        }

        public void UpdateScoreText()
        {
            ScoreText.text = Data.Score.ToString();
            if (Data.Score > CurrentLocalData.BestScores[GridManager.SideLength - 3])
                BestScoreText.text = Data.Score.ToString();
        }

        private void GameOverMethod()
        {
            //ControlButtons.gameObject.SetActive(false);
            ControlButtons.localPosition = new Vector3(1500, 0,0);
            //GameOverPanel.gameObject.SetActive(true);
            GameOverPanel.localPosition = Vector3.zero;
            CurrentLocalData.StateIsSaved[GridManager.SideLength - 3] = false;
            if (Data.Score > CurrentLocalData.BestScores[GridManager.SideLength - 3])
                CurrentLocalData.BestScores[GridManager.SideLength - 3] = Data.Score;
            CurrentLocalData.LastScores[GridManager.SideLength - 3] = 0;
            GridManager.SaveDataToCloudAndToLocalMemory();
        }

        private void Restart()
        {
            GridManager.SaveDataToCloudAndToLocalMemory();
            GridManager.ClearGrid();
            CurrentLocalData.StateIsSaved[CurrentLocalData.LastGameMode - 3] = false;
                SetSizeAndStartGame(CurrentLocalData.LastGameMode);
        }

        #endregion

        #region UI

        public void ResumeGame()
        {
            switch (GridManager.GridState)
            {
                case 0:
                case 3:
                    //MenuPanel.SetActive(false);
                    MenuPanel.localPosition = new Vector3(1500, 0, 0);
                    SetSizeAndStartGame(CurrentLocalData.LastGameMode);
                    GridManager.GridState = 1;
                    GridManager.isActive = true;
                    break;
                case 1:
                case 2:
                    //MenuPanel.SetActive(false);
                    MenuPanel.localPosition = new Vector3(1500, 0, 0);
                    SetMapState(true);
                    GridManager.GridState = 1;
                    GridManager.isActive = true;
                    break;
            }
        }

        public void Pause()
        {
            GridManager.GridState = 2;
            //MenuPanel.SetActive(true);
            MenuPanel.localPosition = Vector3.zero;
            GridManager.isActive = false;
            GridManager.SaveDataToCloudAndToLocalMemory();
        }

        public void SetMapState(bool state)
        {
            //MainMap.isActive = state;
            GridManager.isActive = state;
        }

        public void SetSizeAndStartGame(int sideLenght)
        {
            MenuPanel.localPosition = new Vector3(1500, 0, 0);
            CurrentLocalData.LastGameMode = sideLenght;
            if (FirstGameStarted)
                GridManager.ClearGrid();
            else
                FirstGameStarted = true;
            if (CurrentLocalData.StateIsSaved[sideLenght - 3])
            {
                FirstGameStarted = true;
                BestScoreText.text = CurrentLocalData.BestScores[sideLenght - 3].ToString();
                GridManager.InitializeGridFromLocalData(sideLenght);
                //MenuPanel.SetActive(false);
                MenuPanel.localPosition = new Vector3(1500, 0, 0);
                //LocalDataManager.WriteLocalData(CurrentLocalData);
            }
            else
            {
                BestScoreText.text = CurrentLocalData.BestScores[sideLenght - 3].ToString();
                CurrentLocalData.LastGameMode = sideLenght;
                GridManager.InitializeGrid(sideLenght);
            }
        }

        public void ReloadGrid()
        {
            //ControlButtons.gameObject.SetActive(true);
            ControlButtons.localPosition = Vector3.zero;
            //GameOverPanel.gameObject.SetActive(false);
            GameOverPanel.localPosition = new Vector3(1500, 0, 0);
            SetSizeAndStartGame(CurrentLocalData.LastGameMode);
        }

        public void RestartButton()
        {
            messageBoxController.OpenNewMessageBox(
                () =>
                {
                    SetMapState(true);
                    Restart();
                },
                () => { SetMapState(true); },
                "Are you sure you want to restart?");
        }

        public void Undo()
        {
            GridManager.SaveDataToCloudAndToLocalMemory();
            GridManager.Undo();
        }

        public void TurnOnOffSound()
        {

        }

        public void ChangeTheme(int next)
        {
            Data.CurrentTheme += next;
            if (Data.CurrentTheme > 4)
            {
                Data.CurrentTheme = 0;
            }
            else if (Data.CurrentTheme < 0)
            {
                Data.CurrentTheme = 4;
            }

            PlayerPrefs.SetInt("theme", Data.CurrentTheme);
            GridManager.Drawer.ChangeTheme();
        }

        private void SetTheme(int theme)
        {
            Data.CurrentTheme = theme;
            GridManager.Drawer.ChangeTheme();
        }

        [SerializeField] private Material uiMaterial, signedInMaterial;
        [SerializeField] private Image gpgsButtonImage;
        private bool canSignInOrOut = true;
        public static string dataName => "bests";

        private void InitializeSignInOrOutGPGSButtonColor()
        {
            gpgsButtonImage.material = GPGSManager.Authenticated() ? signedInMaterial : uiMaterial;
        }

        public void SignInOrOutGPGS()
        {
            if (GPGSManager.Authenticated())
            {
                messageBoxController.OpenNewMessageBox(
                    () =>
                    {
                        GPGSManager.SignOut();
                        gpgsButtonImage.material = uiMaterial;
                    },
                    () => { },
                    "All your local data will be erased, are you sure you want to sign out?");
            }
            else
            {
                GPGSManager.SignIn(InitializeSignInOrOutGPGSButtonColor, dataName, OnDataRead);
            }
        }

        private void OnDataRead(byte[] data)
        {
            int[] dataInt = Converter.ToIntArray(data);
            bool dataIsEmpty = dataInt.All(element => element == 0);
            if (!dataIsEmpty)
            {
                //Debug.Log(dataInt.Aggregate("", (current, i) => current + (i + " ")));
                CurrentLocalData.BestScores = dataInt;
                LocalDataManager.WriteLocalData(CurrentLocalData);
            }
            else
            {
                bool localDataIsEmpty = CurrentLocalData.BestScores.All(element => element == 0);
                if (!localDataIsEmpty) GPGSManager.WriteDataToCloud(dataName, Converter.ToByteArray(CurrentLocalData.BestScores));
            }
        }


        public void ShowAchievementsUI()
        {
            GPGSManager.ShowAchievementsUI();
        }
        
        public void ShowLeaderboardUI()
        {
            GPGSManager.ShowLeaderboardUI();
        }

        #endregion
    }
}
