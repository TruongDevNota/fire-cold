using UnityEngine;

namespace MewtonGames.Nonogram
{
    public class CameraMapMoving : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        
        [Header("Moving Settings")]
        [SerializeField] float _cameraSpeedFactor;
        [SerializeField] float _cameraFriction;

        public Camera pCamera => _camera;

        private Transform _cameraTrans;
        private float _currentSpeed;
        private bool _usingFriction;
        private float _movingDistance;

        private float _screenBoundMinY;
        private float _screenBoundMaxY;
        
        public void Init()
        {
            _cameraTrans = _camera.transform;
            _currentSpeed = 0f;
            _movingDistance = 0;
            _usingFriction = false;
        }

        public void SetupBound(float boundMinY, float boundMaxY)
        {
            _screenBoundMaxY = boundMaxY;
            _screenBoundMinY = boundMinY;
        }

        public void Show(float currentCameraPosY)
        {
            _cameraTrans.position = new Vector3(0f, Mathf.Clamp(currentCameraPosY, _screenBoundMinY, _screenBoundMaxY), -10f);
        }

        public void Tick()
        {
            if (_movingDistance == 0f)
                return;

            if (!_usingFriction)
            {
                Vector3 currentPos = _cameraTrans.position;
                currentPos.y += _movingDistance;
                currentPos.y = Mathf.Clamp(currentPos.y, _screenBoundMinY, _screenBoundMaxY);
                _cameraTrans.position = currentPos;
            }
            else 
            {
                _currentSpeed -= _currentSpeed * _cameraFriction;
                if (_currentSpeed <= 0.001f && _currentSpeed >= -0.001f)
                {
                    _currentSpeed = 0f;
                    _movingDistance = 0f;
                    _usingFriction = false;
                }

                Vector3 currentPos = _cameraTrans.position;
                currentPos.y += _currentSpeed * Time.deltaTime;
                currentPos.y = Mathf.Clamp(currentPos.y, _screenBoundMinY, _screenBoundMaxY);
                _cameraTrans.position = currentPos;
            }
        }

        public void MoveDistance(float distance, bool withFriction)
        {
            _movingDistance = distance;
            _currentSpeed = _movingDistance * _cameraSpeedFactor;
            _usingFriction = withFriction;
        }
    }
}
