using System;
using CerberusFramework.UI.Components;
using CerberusFramework.UI.Popups;
using UnityEngine;

namespace CFGameClient.UI.Popups.WinPopupVariant
{
    public class WinPopupVariantView : PopupView
    {
        [SerializeField] protected CFText HighScoreText;
        [SerializeField] protected CFText CurrentScoreText;
        [SerializeField] protected CFButton BackToMainMenuButton;

        public event Action BackToMainMenuButtonClicked;

        public override void Initialize()
        {
            base.Initialize();

            BackToMainMenuButton.onClick.AddListener(() => BackToMainMenuButtonClicked?.Invoke());
        }

        public override void Dispose()
        {
            BackToMainMenuButton.onClick.RemoveListener(() => BackToMainMenuButtonClicked?.Invoke());
        }

        public void SetHighScoreText(string highScore, string currentScoreText)
        {
            HighScoreText.Text = $"High Score: {highScore}";
            CurrentScoreText.Text = $"Current Score: {currentScoreText}";
        }
    }
}
