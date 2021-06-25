using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        #region Fields

        public GridManager GridManager;

        //public Map MainMap;
        public GameObject ControlButtons, GameOverPanel;
        public SwipeController Controller;
        public Text ScoreText;
        private bool FirstGameStarted;
        public bool GameOver { get; set; }

        #endregion

        private void Awake()
        {
            SetTheme(PlayerPrefs.GetInt("theme"));
            
        }

        #region Game managment

        private void Update()
        {
            if(GameOver) GameOverMethod();
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
        }

        private void GameOverMethod()
        {
            ControlButtons.gameObject.SetActive(false);
            GameOverPanel.gameObject.SetActive(true);
        }

        #endregion

        #region UI

        public void SetMapState(bool state)
        {
            //MainMap.isActive = state;
            GridManager.isActive = state;
        }

        public void SetSizeAndStartGame(int sideLenght)
        {
            if (FirstGameStarted)
            {
                GridManager.ClearGrid();
                //MainMap.ClearMap();
            }
            else
            {
                FirstGameStarted = true;
            }

            //MainMap.InitializeMap(sideLenght);
            GridManager.InitializeGrid(sideLenght);
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
            GridManager.InitializeGrid(4);
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
