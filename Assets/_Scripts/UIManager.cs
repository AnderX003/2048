using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MessageBoxController messageBoxController;

        [SerializeField] private Text modeText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Image gpgsButtonImage;
        [SerializeField] private Sprite[] gpgsButtonSprites;

        [SerializeField] private Animator gamePanelAnimator;
        [SerializeField] private Animator MenuAnimator;

        #region Buttons

        public void ChangeGameModeButton(bool plus)
        {
            if (plus)
            {
                if (gameManager.menuGameMode == 8) return;
                gameManager.menuGameMode++;
            }
            else
            {
                if (gameManager.menuGameMode == 3) return;
                gameManager.menuGameMode--;
            }

            UpdateModeText();
        }

        public void PauseButton()
        {
            gameManager.Pause();
        }

        public void PlayButton()
        {
            gameManager.ResumeGame();
        }

        public void RestartButton()
        {
            gameManager.RestartButton();
        }

        public void UndoButton()
        {
            gameManager.Undo();
        }

        public void ChangeThemeButton()
        {
            gameManager.ChangeTheme();
        }
    
        public void ShowAchievementsUIButton()
        {
            gameManager.ShowAchievementsUI();
        }
        
        public void ShowLeaderboardUIButton()
        {
            gameManager.ShowLeaderboardUI();
        }

        public void SignInOrOutGPGSButton()
        {
            gameManager.SignInOrOutGPGS();
        }

        #endregion

        #region Animations

        public void SetGameOverAnimation(string id)
        {
            gamePanelAnimator.SetTrigger(id);
        }

        public void SetMenuAnimation(string id)
        {
            MenuAnimator.SetTrigger(id);
        }

        #endregion

        public void UpdateModeText()
        {
            modeText.text = $"{gameManager.menuGameMode}×{gameManager.menuGameMode}";
        }

        public void OpenMessageBox(Action agree, Action disagree, string question)
        {
            messageBoxController.OpenNewMessageBox(agree, disagree, question);
        }

        public void SetGPGSButtonSprite(bool signedIn)
        {
            gpgsButtonImage.sprite = signedIn ? gpgsButtonSprites[0] : gpgsButtonSprites[1];
        }

        public void SetBestScoreText(string text)
        {
            bestScoreText.text = text;
        }

        public void SetScoreText(string text)
        {
            scoreText.text = text;
        }
    }
}
