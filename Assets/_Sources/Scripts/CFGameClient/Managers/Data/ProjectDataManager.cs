using System;
using System.Collections.Generic;
using System.Threading;
using CerberusFramework.Managers.Data;
using CerberusFramework.Managers.Data.Syncers;
using CerberusFramework.Managers.Pool;
using CFGameClient.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace CFGameClient.Managers.Data
{
    public class ProjectDataManager : DataManager
    {
        public GameSettings GameSettings
        {
            get
            {
                return _gameSettings;
            }
        }

        public ProjectSaveStorage ProjectSaveStorage
        {
            get
            {
                return _saveStorage;
            }
        }

        private ILocalSyncer<ProjectSaveStorage> _localSyncer;
        private ProjectSaveStorage _saveStorage;
        private GameSettings _gameSettings;

        private AssetManager _assetManager;

        [Inject]
        public void Inject(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        protected override async UniTask Initialize(CancellationToken disposeToken)
        {
            SaveKey = "ProjectSave";

            _localSyncer = new LocalStorageSyncer<ProjectSaveStorage>(SaveKey, PlayerPrefs.GetInt("PlayerID").ToString());

            var tasks = new List<UniTask>();
            _gameSettings = _assetManager.GetScriptableAsset<GameSettings>(CFPoolKeys.GameSettings);

            if (_gameSettings == null)
            {
                tasks.Add(_assetManager.GetScriptableAsset<GameSettings>(CFPoolKeys.GameSettings, disposeToken)
                 .ContinueWith((gameSettings) => _gameSettings = gameSettings));
            }

            tasks.Add(_localSyncer.Load(disposeToken).ContinueWith((storage) => _saveStorage = storage));

            await UniTask.WhenAll(tasks);

            StartAutoSavingJob(disposeToken).Forget();
            SetReady();
        }

        protected override void SaveLocal()
        {
            _localSyncer.Save(_saveStorage);
        }

        public override T Load<T>()
        {
            return !IsReady() ? throw new InvalidOperationException("Trying to load data before ProjectDataManager is ready") : _saveStorage.Get<T>();
        }

        public override void Save<T>(T data)
        {
            if (!IsReady())
            {
                throw new InvalidOperationException("Trying to save data before ProjectDataManager is ready");
            }

            _saveStorage.Set(data);
            IsSaveDirty = true;
        }
    }
}
