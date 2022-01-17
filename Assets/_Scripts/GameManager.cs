using System.Linq;
using UnityEngine;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        #region FieldsAndProrerties

        public static GameManager Instance { get;private set; }
        public LocalData CurrentLocalData { get; set; }
        public int menuGameMode { get; set; } = 4;
        public static string dataName => "bests";
        
        [SerializeField] private GridManager GridManager;
        [SerializeField] private SwipeController Controller;
        [SerializeField] private AdsController adsController;
        [SerializeField] private UIManager uiManager;
        private bool FirstGameStarted;
        private const int themesAmount = 4 - 1;

        #endregion

        private void Awake()
        {
            Instance = this;

            LocalDataManager.ReadLocalData(data =>
            {
                CurrentLocalData = data ?? new LocalData();
                menuGameMode = CurrentLocalData.LastGameMode;
                uiManager.UpdateModeText();
            });

            if (!PlayerPrefs.HasKey("GameWasPlayed")) PlayerPrefs.SetInt("DataWasReadFromCloud", 0);
            GPGSManager.Initialize();
            GPGSManager.SignIn(InitializeSignInOrOutGPGSButtonColor, dataName, OnDataRead);
        }

        private void Start()
        {
            SetTheme(PlayerPrefs.GetInt("theme"));
            adsController.Initialize();
        }

        private void Update()
        {
            if (GridManager.IsActive && GridManager.MoveIsOver)
            {
#if UNITY_ANDROID
                if (Controller.SwipeRight) GridManager.Turn(Side.R);
                else if (Controller.SwipeUp) GridManager.Turn(Side.Up);
                else if (Controller.SwipeLeft) GridManager.Turn(Side.L);
                else if (Controller.SwipeDown) GridManager.Turn(Side.Down);
                #endif
#if UNITY_EDITOR
                if (Input.GetKeyUp(KeyCode.RightArrow)) GridManager.Turn(Side.R);
                else if (Input.GetKeyUp(KeyCode.UpArrow)) GridManager.Turn(Side.Up);
                else if (Input.GetKeyUp(KeyCode.LeftArrow)) GridManager.Turn(Side.L);
                else if (Input.GetKeyUp(KeyCode.DownArrow)) GridManager.Turn(Side.Down);
                if (Input.GetKeyUp(KeyCode.Backspace)) Undo();
#endif
            }
            if(Input.GetKeyDown(KeyCode.Escape)) Escape();
            Controller.Enabled = GridManager.IsActive;
        }

        public void UpdateScoreText()
        {
            uiManager.SetScoreText(Data.Score.ToString());
            if (Data.Score > CurrentLocalData.BestScores[GridManager.SideLength - 3])
            {
                uiManager.SetBestScoreText(Data.Score.ToString());
            }
        }
        
        public void GameOverMethod()
        {
            uiManager.SetGameOverAnimation("In");
            
            CurrentLocalData.StateIsSaved[GridManager.SideLength - 3] = false;
            
            if (Data.Score > CurrentLocalData.BestScores[GridManager.SideLength - 3])
                CurrentLocalData.BestScores[GridManager.SideLength - 3] = Data.Score;
            CurrentLocalData.LastScores[GridManager.SideLength - 3] = 0;
            
            if (!CurrentLocalData.AdIsShowedInThisGame[GridManager.SideLength - 3])
            {
                CurrentLocalData.AdIsShowedInThisGame[GridManager.SideLength - 3] = true;
                Wait.ForSeconds(GridManager.Drawer.DelayBeforeShowingAd, 
                    adsController.ShowGameOverAd);
            }
            
            GridManager.SaveDataToCloudAndToLocalMemory();
        }

        private void Restart()
        {
            GridManager.SaveDataToCloudAndToLocalMemory();
            GridManager.ClearGrid();
            CurrentLocalData.StateIsSaved[CurrentLocalData.LastGameMode - 3] = false;
            SetSizeAndStartGame(CurrentLocalData.LastGameMode);
        }
        
        private void Escape()
        {
            switch (GridManager.State)
            {
                case GridState.Nothing:
                case GridState.Pause:
                    GridManager.State = GridState.Quitting;
                    uiManager.OpenMessageBox(
                        Application.Quit, 
                        () => GridManager.State = GridState.Pause, 
                        "Are you sure you want to quit the game?");
                    break;
                case GridState.Quitting:
                    Application.Quit();
                    break;
                case GridState.Game:
                case GridState.GameOver:
                    uiManager.PauseButton();
                    uiManager.SetMenuAnimation("Any");
                    //MenuAnimator.SetTrigger(Any);
                    break;
            }
        }

        public void ResumeGame()
        {
            switch (GridManager.State)
            {
                case GridState.Nothing:
                case GridState.GameOver:
                    SetSizeAndStartGame(menuGameMode);
                    GridManager.State = GridState.Game;
                    break;
                case GridState.Game:
                case GridState.Pause:
                    if (menuGameMode == GridManager.SideLength)
                    {
                        GridManager.IsActive = true;
                        GridManager.State = GridState.Game;
                        GridManager.IsActive = true;
                    }
                    else
                    {
                        SetSizeAndStartGame(menuGameMode);
                        GridManager.State = GridState.Game;
                    }

                    break;
            }
        }

        public void Pause()
        {
            if (GridManager.State == GridState.GameOver)
            {
                uiManager.SetGameOverAnimation("Out");
                GridManager.State = GridState.Nothing;
                Wait.ForSeconds(GridManager.Drawer.DelayAfterClearingGridAfterGameOver, GridManager.ClearGrid);
            }
            else
            {
                GridManager.State = GridState.Pause;
                GridManager.IsActive = false;
                GridManager.SaveDataToCloudAndToLocalMemory();
            }
        }

        private void SetSizeAndStartGame(int sideLenght)
        {
            CurrentLocalData.LastGameMode = sideLenght;

            if (FirstGameStarted)
            {
                GridManager.ClearGrid();
            }
            else
            {
                FirstGameStarted = true;
            }
            
            if (CurrentLocalData.StateIsSaved[sideLenght - 3])
            {
                FirstGameStarted = true;
                uiManager.SetBestScoreText(CurrentLocalData.BestScores[sideLenght - 3].ToString());
                GridManager.InitializeGridFromLocalData(sideLenght);
            }
            else
            {
                uiManager.SetBestScoreText(CurrentLocalData.BestScores[sideLenght - 3].ToString());
                CurrentLocalData.LastGameMode = sideLenght;
                CurrentLocalData.AdIsShowedInThisGame[sideLenght - 3] = false;
                GridManager.InitializeGrid(sideLenght);
            }
        }

        public void RestartButton()
        {
            if (GridManager.State == GridState.GameOver)
            {
                uiManager.SetGameOverAnimation("Out");
                GridManager.IsActive = true;
                Restart();
            }
            else
            {
                uiManager.OpenMessageBox(
                    () =>
                    {
                        GridManager.IsActive = true;
                        Restart();
                    },
                    () => { GridManager.IsActive = true; },
                    "Are you sure you want to restart?");
            }
        }

        public void Undo()
        {
            if (!GridManager.CanUndo || !GridManager.MoveIsOver) return;
            GridManager.Undo();
            
            if (GridManager.State == GridState.GameOver)
            {
                uiManager.SetGameOverAnimation("Out");
                GridManager.State = GridState.Game;
                GridManager.IsActive = true;
                CurrentLocalData.StateIsSaved[GridManager.SideLength - 3] = true;
            }
        }
        
        public void ChangeTheme()
        {
            Data.CurrentTheme++;
            if (Data.CurrentTheme > themesAmount)
            {
                Data.CurrentTheme = 0;
            }
            else if (Data.CurrentTheme < 0)
            {
                Data.CurrentTheme = themesAmount;
            }

            GridManager.State = GridState.Nothing;
            PlayerPrefs.SetInt("theme", Data.CurrentTheme);
            GridManager.Drawer.ChangeTheme();
        }

        private void SetTheme(int theme)
        {
            if (themesAmount < theme) theme = 0;
            Data.CurrentTheme = theme;
            GridManager.Drawer.SetTheme();
        }

        private void InitializeSignInOrOutGPGSButtonColor()
        {
            uiManager.SetGPGSButtonSprite(GPGSManager.Authenticated());
        }

        public void SignInOrOutGPGS()
        {
            if (GPGSManager.Authenticated())
            {
                uiManager.OpenMessageBox(
                    () =>
                    {
                        GPGSManager.SignOut();
                        InitializeSignInOrOutGPGSButtonColor();
                        GridManager.State = GridState.Nothing;
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
                CurrentLocalData.BestScores = dataInt;
                LocalDataManager.WriteLocalData(CurrentLocalData);
                GridManager.State = GridState.Nothing;
            }
            else
            {
                bool localDataIsEmpty = CurrentLocalData.BestScores.All(element => element == 0);
                if (!localDataIsEmpty)
                {
                    GPGSManager.WriteDataToCloud(dataName, Converter.ToByteArray(CurrentLocalData.BestScores));
                }
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
    }
}
