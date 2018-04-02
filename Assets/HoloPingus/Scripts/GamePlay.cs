using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GamePlay : MonoBehaviour
{

    enum GameState
    {
        Scanning,
        PlacingStart,
        PlacingEnd,
        StartPlaying,
        Playing
    }

    public Camera TheMainCamera;

    public GameObject StartPositionMarker;
    public GameObject EndPositionMarker;

    CreatureManager _behaviour;

    GameObject _currentPositionMarker;

    GameState _gameState;

    UnityEngine.XR.WSA.Input.GestureRecognizer _gestureRecognizer;

    float _scanTime = 10.0f;

    public static bool Testing = false;

    // Use this for initialization
    void Start()
    {

        _behaviour = this.GetComponent<CreatureManager>();

        _gameState = GameState.Scanning;

        StartPositionMarker.SetActive(false);
        EndPositionMarker.SetActive(false);

        _gestureRecognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        _gestureRecognizer.SetRecognizableGestures(UnityEngine.XR.WSA.Input.GestureSettings.Tap);
        _gestureRecognizer.TappedEvent += _gestureRecognizer_TappedEvent;

        SpatialMappingManager.Instance.StartObserver();

        if (Testing)
        {
            StartPositionMarker.SetActive(true);
            EndPositionMarker.SetActive(true);

            StartPositionMarker.transform.position = new Vector3(0, 0, 1);
            EndPositionMarker.transform.position = new Vector3(0, 0, 0);

            _scanTime = 0.0f;
            _gameState = GameState.StartPlaying;
        }

    }

    private void _gestureRecognizer_TappedEvent(UnityEngine.XR.WSA.Input.InteractionSourceKind source, int tapCount, Ray headRay)
    {

        if (_gameState == GameState.PlacingStart)
        {
            EndPositionMarker.SetActive(true);
            _currentPositionMarker = EndPositionMarker;
            _gameState = GameState.PlacingEnd;

            return;
        }
        if (_gameState == GameState.PlacingEnd)
        {

            _gameState = GameState.StartPlaying;

            return;
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (Time.time - SpatialMappingManager.Instance.StartTime < _scanTime)
        {
            Debug.Log("Keep looking around !!!");

            return;
        }

        SpatialMappingManager.Instance.StopObserver();

        if (_gameState == GameState.Scanning)
        {
            StartPositionMarker.SetActive(true);
            _currentPositionMarker = StartPositionMarker;
            _gestureRecognizer.StartCapturingGestures();
            _gameState = GameState.PlacingStart;
        }

        if (_gameState == GameState.PlacingStart
            || _gameState == GameState.PlacingEnd)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(
                    TheMainCamera.transform.position,
                    TheMainCamera.transform.forward,
                    out hitInfo,
                    20.0f,
                    SpatialMappingManager.Instance.LayerMask))
            {
                Debug.Log("Something was hit");

                _currentPositionMarker.transform.position = hitInfo.point;
                _currentPositionMarker.transform.up = hitInfo.normal;

            }
        }

        if (_gameState == GameState.StartPlaying)
        {
            _behaviour.StartCreation(StartPositionMarker.transform.position, EndPositionMarker.transform.position);
            _gameState = GameState.Playing;
        }

    }
}
