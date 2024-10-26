using System;
using CerberusFramework.UI.Components;
using CerberusFramework.Utilities.MonoBehaviourUtilities;
using GameClient.Runtime;
using GameClient.Runtime.Events;
using MessagePipe;
using UnityEngine;
using UnityEngine.UI;

namespace GameClient.UI
{
    public class PlayerUnitSeller : MonoBehaviour
    {
        [SerializeField] private EasyInputManager _easyInputManager;
        [SerializeField] private GameObject _movedItem;

        [SerializeField] private Image _costImage;
        [SerializeField] private CFText _costText;

        private IPublisher<SellerPlacedEvent> _sellerPlacedEventPublisher;

        private Vector2 _movedItemStartPosition;
        private int _poolKey;
        private int _cost;

        private IDisposable _messageSubscription;

        private Color _onColor;
        private Color _offColor;

        private bool _isActive;

        private Camera _camera;

        private int _layerMask;

        public void Initialize(Camera camera, int cost, int poolKey)
        {
            _camera = camera;
            _cost = cost;
            _poolKey = poolKey;

            var bagBuilder = DisposableBag.CreateBuilder();
            GlobalMessagePipe.GetSubscriber<GoldChangedEvent>().Subscribe(OnGoldChangedEvent).AddTo(bagBuilder);
            GlobalMessagePipe.GetSubscriber<PlayerUnitCostChangedEvent>().Subscribe(OnPlayerUnitCostChangedEvent).AddTo(bagBuilder);
            _messageSubscription = bagBuilder.Build();

            _sellerPlacedEventPublisher = GlobalMessagePipe.GetPublisher<SellerPlacedEvent>();

            _onColor = _costImage.color;
            _offColor = Color.gray;
            _costText.Text = _cost.ToString();

            _movedItemStartPosition = _movedItem.transform.position;
            _movedItem.SetActive(false);

            _isActive = true;

            _layerMask = LayerMask.GetMask("PassiveTile");

            _easyInputManager.Selected += OnSelected;
            _easyInputManager.Dragged += OnDragged;
            _easyInputManager.Released += OnReleased;
        }

        public void Dispose()
        {
            _messageSubscription?.Dispose();

            _easyInputManager.Selected -= OnSelected;
            _easyInputManager.Dragged -= OnDragged;
            _easyInputManager.Released -= OnReleased;
        }

        private void OnSelected(UnityEngine.EventSystems.PointerEventData obj)
        {
            if (!_isActive)
            {
                return;
            }

            _movedItem.transform.position = obj.position;
            _movedItem.SetActive(true);
        }

        private void OnDragged(UnityEngine.EventSystems.PointerEventData obj)
        {
            if (!_isActive)
            {
                return;
            }

            _movedItem.transform.position = obj.position;
        }

        private void OnReleased(UnityEngine.EventSystems.PointerEventData obj)
        {
            if (!_isActive)
            {
                return;
            }

            _movedItem.transform.position = _movedItemStartPosition;
            _movedItem.SetActive(false);

            Ray ray = _camera.ScreenPointToRay(obj.position);
            if (Physics.Raycast(ray, out var hit, 100f, _layerMask) && hit.transform.gameObject.TryGetComponent<PassiveTile>(out var tile))
            {
                _sellerPlacedEventPublisher.Publish(new SellerPlacedEvent(_poolKey, tile));
            }
        }

        private void OnGoldChangedEvent(GoldChangedEvent evt)
        {
            _isActive = evt.Gold >= _cost;
            SetCostImageColor(_isActive);
        }

        private void OnPlayerUnitCostChangedEvent(PlayerUnitCostChangedEvent evt)
        {
            if (_poolKey != evt.PoolKey)
            {
                return;
            }

            _cost = evt.Cost;
            _costText.Text = _cost.ToString();
        }

        private void SetCostImageColor(bool isActive)
        {
            _costImage.color = isActive ? _onColor : _offColor;
        }
    }
}
