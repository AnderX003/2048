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
        
        [SerializeField] private GridManager GridManager;
        [SerializeField] private SwipeController Controller;
        [SerializeField] private AdsController adsController;
        [SerializeField] private Text ScoreText, BestScoreText;
        [SerializeField] private MessageBoxController messageBoxController;
        [SerializeField] private Animator MenuAnimator;

        private bool FirstGameStarted;

        #endregion

        private void Awake()
        {
            Instance = this;

            LocalDataManager.ReadLocalData(data =>
            {
                CurrentLocalData = data ?? new LocalData();
                menuGameMode = CurrentLocalData.LastGameMode;
                UpdateModeText();
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

        #region Game managment

        private void Update()
        {
            if (GridManager.IsActive && GridManager.MoveIsOver)
            {
#if UNITY_ANDROID
                if (Controller.SwipeRight) GridManager.Turn(0);
                else if (Controller.SwipeUp) GridManager.Turn(1);
                else if (Controller.SwipeLeft) GridManager.Turn(2);
                else if (Controller.SwipeDown) GridManager.Turn(3);
                #endif
#if UNITY_EDITOR
                if (Input.GetKeyUp(KeyCode.RightArrow)) GridManager.Turn(0);
                else if (Input.GetKeyUp(KeyCode.UpArrow)) GridManager.Turn(1);
                else if (Input.GetKeyUp(KeyCode.LeftArrow)) GridManager.Turn(2);
                else if (Input.GetKeyUp(KeyCode.DownArrow)) GridManager.Turn(3);
#endif
            }
            if(Input.GetKeyDown(KeyCode.Escape)) Escape();
            Controller.Enabled = GridManager.IsActive;
        }

        public void UpdateScoreText()
        {
            ScoreText.text = Data.Score.ToString();
            if (Data.Score > CurrentLocalData.BestScores[GridManager.SideLength - 3])
                BestScoreText.text = Data.Score.ToString();
        }

        [SerializeField] private Animator gamePanelAnimator;
        private static readonly int In = Animator.StringToHash("In");
        private static readonly int Out = Animator.StringToHash("Out");
        public void GameOverMethod()
        {
            gamePanelAnimator.SetTrigger(In);
            
            CurrentLocalData.StateIsSaved[GridManager.SideLength - 3] = false;
            
            if (Data.Score > CurrentLocalData.BestScores[GridManager.SideLength - 3])
                CurrentLocalData.BestScores[GridManager.SideLength - 3] = Data.Score;
            CurrentLocalData.LastScores[GridManager.SideLength - 3] = 0;
            
            if (!CurrentLocalData.AdIsShowedInThisGame[GridManager.SideLength - 3])
            {
                CurrentLocalData.AdIsShowedInThisGame[GridManager.SideLength - 3] = true;
                StartCoroutine(
                    GridManager.InvokeWithDelay(
                        0.4f, adsController.ShowGameOverAd));
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

        private GridState previousState;
        private static readonly int Any = Animator.StringToHash("Any");
        private void Escape()
        {
            switch (GridManager.State)
            {
                case GridState.Nothing:
                case GridState.Pause:
                    previousState = GridManager.State;
                    GridManager.State = GridState.Quitting;
                    messageBoxController.OpenNewMessageBox(
                        Application.Quit,
                        () => GridManager.State = previousState, 
                        "Are you sure you want to quit the game?");
                    break;
                case GridState.Quitting:
                    Application.Quit();
                    break;
                case GridState.Game:
                case GridState.GameOver:
                    Pause();
                    MenuAnimator.SetTrigger(Any);
                    break;
            }
        }

        #endregion

        #region UI

        public void ResumeGame()
        {
            switch (GridManager.State)
            {
                case GridState.Nothing:
                case GridState.GameOver:
                    SetSizeAndStartGame(menuGameMode);//CurrentLocalData.LastGameMode);
                    GridManager.State = GridState.Game;
                    //GridManager.IsActive = true;
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
                gamePanelAnimator.SetTrigger(Out);
                GridManager.State = GridState.Nothing;
                StartCoroutine(GridManager.InvokeWithDelay(1 / 3f, ClearGrid));
            }
            else
            {
                GridManager.State = GridState.Pause;
                GridManager.IsActive = false;
                GridManager.SaveDataToCloudAndToLocalMemory();
            }
        }

        private void ClearGrid()
        {
            GridManager.ClearGrid();
        }

        public void SetSizeAndResumeGame(int sideLenght)
        {
            //GridManager.IsActive = true;
            switch (GridManager.State)
            {
                case GridState.GameOver:
                case GridState.Nothing:
                    SetSizeAndStartGame(sideLenght);
                    break;
                case GridState.Game:
                case GridState.Pause:
                    if (GridManager.SideLength == sideLenght)
                    {
                        GridManager.State = GridState.Game;
                        GridManager.IsActive = true;
                    }
                    else SetSizeAndStartGame(sideLenght);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private int menuGameMode = 4;
        [SerializeField] private Text modeText;
        public void ChangeGameMode(bool plus)
        {
            if (plus)
            {
                if(menuGameMode == 8) return;
                menuGameMode++;
            }
            else
            {
                if (menuGameMode == 3) return;
                menuGameMode--;
            }
            UpdateModeText();
        }

        private void UpdateModeText()
        {
            modeText.text = $"{menuGameMode}×{menuGameMode}";
        }

        private void SetSizeAndStartGame(int sideLenght)
        {
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
            }
            else
            {
                BestScoreText.text = CurrentLocalData.BestScores[sideLenght - 3].ToString();
                CurrentLocalData.LastGameMode = sideLenght;
                CurrentLocalData.AdIsShowedInThisGame[sideLenght - 3] = false;
                GridManager.InitializeGrid(sideLenght);
            }
        }

        public void RestartButton()
        {
            if (GridManager.State == GridState.GameOver)
            {
                gamePanelAnimator.SetTrigger(Out);
                GridManager.IsActive = true;
                Restart();
            }
            else
                messageBoxController.OpenNewMessageBox(
                    () =>
                    {
                        GridManager.IsActive = true;
                        Restart();
                    },
                    () => { GridManager.IsActive = true; },
                    "Are you sure you want to restart?");
        }

        public void Undo()
        {
            if (GridManager.State == GridState.GameOver)
            {
                gamePanelAnimator.SetTrigger(Out);
                GridManager.State = GridState.Game;
                GridManager.IsActive = true;
                CurrentLocalData.StateIsSaved[GridManager.SideLength - 3] = true;
            }
            
            GridManager.Undo();
        }

        private const int themes = 4 - 1;
        public void ChangeTheme(int theme)
        {
            Data.CurrentTheme += theme;
            if (Data.CurrentTheme > themes)
            {
                Data.CurrentTheme = 0;
            }
            else if (Data.CurrentTheme < 0)
            {
                Data.CurrentTheme = themes;
            }

            GridManager.State = GridState.Nothing;
            PlayerPrefs.SetInt("theme", Data.CurrentTheme);
            GridManager.Drawer.ChangeTheme();
        }

        private void SetTheme(int theme)
        {
            if (themes < theme) theme = 0;
            Data.CurrentTheme = theme;
            GridManager.Drawer.SetTheme();
        }

        //[SerializeField] private Material uiMaterial, signedInMaterial;
        [SerializeField] private Image gpgsButtonImage;
        [SerializeField] private Sprite[] gpgsButtonSprites;
        public static string dataName => "bests";

        private void InitializeSignInOrOutGPGSButtonColor()
        {
            //gpgsButtonImage.material = GPGSManager.Authenticated() ? signedInMaterial : uiMaterial;
            gpgsButtonImage.sprite = GPGSManager.Authenticated() ? gpgsButtonSprites[0] : gpgsButtonSprites[1];
        }

        public void SignInOrOutGPGS()
        {
            if (GPGSManager.Authenticated())
            {
                messageBoxController.OpenNewMessageBox(
                    () =>
                    {
                        GPGSManager.SignOut();
                        InitializeSignInOrOutGPGSButtonColor();
                        GridManager.State = GridState.Nothing;
                        //gpgsButtonImage.material = uiMaterial;
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
