using CerberusFramework.Managers.Pool;
using CerberusFramework.UI.Popups;
using CFGameClient.Core.Scenes;
using CFGameClient.Managers.Data;

namespace CFGameClient.UI.Popups.WinPopupVariant
{
    public class WinPopupVariantData : PopupData
    {
        public LevelSceneController LevelSessionScene;
        public GameSessionSaveStorage GameSessionSaveStorage;

        public WinPopupVariantData(LevelSceneController levelSceneController, GameSessionSaveStorage gameSessionSaveStorage)
            : base(CFPoolKeys.FromId(GamePoolKeys.WinPopupVariant.Id))
        {
            LevelSessionScene = levelSceneController;
            GameSessionSaveStorage = gameSessionSaveStorage;
        }
    }
}
