using UnityEngine;

namespace MewtonGames.Nonogram
{
    public class MapController : MonoBehaviour
    {
        [Header("Components")]
        //[SerializeField] StageGenerator _stageGenerator;
        [SerializeField] MapInput _mapInput;
        [SerializeField] CameraMapMoving _cameraMoving;
        [SerializeField] private Vector2 boundY;
        [SerializeField] private float currentY;
        public void Start()
        {
            _mapInput.Init();
            _cameraMoving.Init();

            _mapInput.Enable(true);
            
            _cameraMoving.SetupBound(boundY.x, boundY.y);
            _cameraMoving.Show(currentY);
        }

        public void OnDestroy()
        {
            _mapInput.Enable(false);
        }

        private void Update()
        {
            _mapInput.Tick();

            if (_mapInput.pIsHolding)
                _cameraMoving.MoveDistance(_mapInput.pMovingWorldOffsetY, _mapInput.pIsFlick);

            if (_mapInput.pIsFlick)
                _mapInput.Reset();

            _cameraMoving.Tick();
            //_stageGenerator.Tick();
        }

        #region Events

        #endregion
    }
}
