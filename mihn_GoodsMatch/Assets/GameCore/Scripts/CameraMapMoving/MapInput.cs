//using MewtonGames.Common;
//using MewtonGames.Nonogram.GameData;
//using MewtonGames.Nonogram.PlayerData;
using UnityEngine;

namespace MewtonGames.Nonogram
{
    public class MapInput : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] LayerMask _stageViewLayer;

        bool _isHolding = false;
        float _lastTouchScreenPosY = 0f;
        float _movingWorldOffsetY = 0f;
        float _unitPerPixel = 0f;
        bool _isFlick;
        int _selectedStage = 0;
        bool _enabled;
        bool _pointedOverUIObject;

        public float pMovingWorldOffsetY => _movingWorldOffsetY;
        public bool pIsHolding => _isHolding;
        public bool pIsFlick => _isFlick;

        //private IPlayerDataProvider _playerDataProvider;
        //private GameController _gameController;

        public void Init()
        {
           // _playerDataProvider = GameContext.Get<IPlayerDataProvider>();
 //           _gameController = GameContext.Get<GameController>();

            _isFlick = false;
            _isHolding = false;

            _pointedOverUIObject = false;

            var p1 = _camera.ScreenToWorldPoint(Vector3.zero);
            var p2 = _camera.ScreenToWorldPoint(Vector3.right);
            _unitPerPixel = Vector3.Distance(p1, p2);
        }

        public void Enable(bool enable)
        {
            _enabled = enable;
        }

        public void Tick()
        {
            if (!_enabled)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverUIGameObject())
                    _pointedOverUIObject = true;

                _lastTouchScreenPosY = Input.mousePosition.y;
                _isFlick = false;
                _isHolding = true;

                RaycastHit2D hit = Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 5f, _stageViewLayer);
                if (hit.collider != null)
                {
                    //StageNode view = hit.transform.GetComponent<StageNode>();
                    //if (view != null)
                    //    _selectedStage = view.pStageIndex;
                }
            }

            if (Input.GetMouseButton(0))
            {
                if (IsPointerOverUIGameObject())
                    _pointedOverUIObject = true;

                _isHolding = true;
                float currentScreenTouchPosY = Input.mousePosition.y;
                _movingWorldOffsetY = (currentScreenTouchPosY - _lastTouchScreenPosY) * _unitPerPixel * -1f;
                _lastTouchScreenPosY = currentScreenTouchPosY;
            }

            if (Input.GetMouseButtonUp(0))
            {
                float currentScreenTouchPosY = Input.mousePosition.y;
                _movingWorldOffsetY = (currentScreenTouchPosY - _lastTouchScreenPosY) * _unitPerPixel * -1f;
                _lastTouchScreenPosY = currentScreenTouchPosY;

                _isFlick = true;

                if (Mathf.Abs(_movingWorldOffsetY) < 0.04f)
                {
                    if (_pointedOverUIObject)
                        return;

                    RaycastHit2D hit = Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 5f, _stageViewLayer);
                    if (hit.collider != null)
                    {
                        //StageNode view = hit.transform.GetComponent<StageNode>();
                        //if (view != null && view.pStageIndex == _selectedStage)
                        //    SelectStage(_selectedStage);
                    }
                }
            }
        }

        public void Reset()
        {
            _isHolding = false;
            _isFlick = false;
            _movingWorldOffsetY = 0f;
            _pointedOverUIObject = false;
        }

        private void SelectStage(int index)
        {
           // _gameController.LoadLevel(LevelType.Default, index.ToString());
        }

        public static bool IsPointerOverUIGameObject()
        {
#if UNITY_EDITOR
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return true;

#else
            if (Input.touchCount > 0)
            {
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return true;
            }
#endif

            return false;
        }
    }
}
