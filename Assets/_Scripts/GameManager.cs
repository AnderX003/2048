using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        #region Fields

        public GridManager GridManager;

        //public Map MainMap;
        public GameObject ControlButtons, GameOverPanel, MenuPanel;
        public SwipeController Controller;
        public Text ScoreText, BestScoreText, DebugText;
        private bool FirstGameStarted;
        public bool GameOver { get; set; }

        public LocalData CurrentLocalData { get; private set; }

        #endregion

        private void Awake()
        {
            SetTheme(PlayerPrefs.GetInt("theme"));
            CurrentLocalData = LocalDataManager.ReadLocalData() ?? new LocalData();
            /*if (CurrentLocalData.LastScores[CurrentLocalData.LastGameMode - 3] != 0)
            {
                FirstGameStarted = true;


                BestScoreText.text = CurrentLocalData.BestScores[CurrentLocalData.LastGameMode - 3].ToString();
                GridManager.InitializeGridFromLocalData(CurrentLocalData.LastGameMode);
                MenuPanel.SetActive(false);
            }*/
            /*DebugText.text += "\n "+ CurrentLocalData.LastGameMode;
            DebugText.text += "\n "+ CurrentLocalData.BestScores[0];
            DebugText.text += "\n "+ CurrentLocalData.LastScores[0];
            DebugText.text += "\n "+ CurrentLocalData.GridState[1][0][0];*/
        }

        #region Game managment

        private void Update()
        {
            //Debug.Log(GridManager.GridState);
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

        public void GameOverMethod()
        {
            ControlButtons.gameObject.SetActive(false);
            GameOverPanel.gameObject.SetActive(true);
            
            CurrentLocalData.StateIsSaved[GridManager.SideLength - 3] = false;
            if (Data.Score > CurrentLocalData.BestScores[GridManager.SideLength - 3])
                CurrentLocalData.BestScores[GridManager.SideLength - 3] = Data.Score;
            CurrentLocalData.LastScores[GridManager.SideLength - 3] = 0;
            LocalDataManager.WriteLocalData(CurrentLocalData);
        }

        #endregion

        #region UI

        public void RemuseGame()
        {
            switch (GridManager.GridState)
            {
                case 0:
                case 3:
                    MenuPanel.SetActive(false);
                    SetSizeAndStartGame(CurrentLocalData.LastGameMode);
                    GridManager.GridState = 1;
                    GridManager.isActive = true;
                    break;
                case 1:
                case 2:
                    MenuPanel.SetActive(false);
                    SetMapState(true);
                    GridManager.GridState = 1;
                    GridManager.isActive = true;
                    break;
            }
        }

        public void Pause()
        {
            GridManager.GridState = 2;
            MenuPanel.SetActive(true);
            GridManager.isActive = false;
        }

        public void SetMapState(bool state)
        {
            //MainMap.isActive = state;
            GridManager.isActive = state;
        }

        public void SetSizeAndStartGame(int sideLenght)
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
                MenuPanel.SetActive(false);
                LocalDataManager.WriteLocalData(CurrentLocalData);
            }
            else
            {
                BestScoreText.text = CurrentLocalData.BestScores[sideLenght - 3].ToString();
                CurrentLocalData.LastGameMode = sideLenght;
                GridManager.InitializeGrid(sideLenght);
            }
        }

        public void CloseSizesPanel(GameObject self)
        {
            if (!FirstGameStarted) return;
            self.SetActive(false);
            SetMapState(true);
        }

        public void ReloadGrid()
        {
            ControlButtons.gameObject.SetActive(true);
            GameOverPanel.gameObject.SetActive(false);
            GridManager.ClearGrid();
            SetSizeAndStartGame(CurrentLocalData.LastGameMode);
        }

        public void EraseLocalData()
        {
            CurrentLocalData = new LocalData();
            LocalDataManager.WriteLocalData(CurrentLocalData);
        }

        public void Undo()
        {
            GridManager.Undo();
        }

        public void TurnOnOffSound()
        {

        }

        public void SignInOutGPGS()
        {

        }
//
        public Material MaterialBackground, MaterialForeground, MaterialText, MaterialUI, MaterialCellText;

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

        #endregion
    }
}
