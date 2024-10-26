using System.Threading;
using CerberusFramework.UI.Popups;
using Cysharp.Threading.Tasks;

namespace CFGameClient.UI.Popups.WinPopupVariant
{
    public class WinPopupVariant : Popup<WinPopupVariantView, WinPopupVariantData>
    {
        public override void Initialize(CancellationToken cancellationToken)
        {
            base.Initialize(cancellationToken);

            View.BackToMainMenuButtonClicked += OnMainMenuButtonClicked;
            View.SetHighScoreText(Data.GameSessionSaveStorage.HighScore.ToString(), Data.GameSessionSaveStorage.CurrentScore.ToString());
        }

        public override bool Dispose()
        {
            if (base.Dispose())
            {
                return true;
            }

            View.BackToMainMenuButtonClicked -= OnMainMenuButtonClicked;

            return false;
        }

        private void OnMainMenuButtonClicked()
        {
            ClosePopup();
            Data.LevelSessionScene.ReturnToMainScene();
        }

        private void ClosePopup()
        {
            PopupManager.Close(this).Forget();
        }
    }
}