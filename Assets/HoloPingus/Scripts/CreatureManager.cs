using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class CreatureManager : MonoBehaviour
{

    private string VOICECOMMAND_LEFT = "left";
    private string VOICECOMMAND_RICHT = "right";
    private string VOICECOMMAND_BRIDGE = "bridge";
    private string VOICECOMMAND_DOWN = "down";
    private string VOICECOMMAND_UP = "up";

    public const int LAYER_PINGUS = 8;
    public const int LAYER_PATHMODIFIER = 9;
    public const int LAYER_FLOOR = 10;

    public const string STARTPOSITION_OBJECTNAME = "StartPosition";
    public const string ENDPOSITION_OBJECTNAME = "EndPosition";

    public const float _endDistance = 0.05f;

    public static int FloorMask;
    public static int PathModifierMask;

    public Camera TheMainCamera;

    KeywordRecognizer _keywordRecognizer;
    Dictionary<string, System.Action> _keywords = new Dictionary<string, System.Action>();

    public GameObject PingusPrototype;
    public GameObject BridgePrototype;
    public GameObject StairUpPrototype;
    public GameObject StairDownPrototype;
    public GameObject HitCursor;

    float _lastPingusCreationTime = 0.0f;
    float _pingusCreationInerval = 4.0f;

    Vector3 _startPosition;
    Vector3 _endPosition;

    Vector3 _movingDirection;

    const int _maxNumberOfCreatures = 100;
    int _currentNumberOfCreatures;

    Dictionary<GameObject, Creature> _walkingPinguses = new Dictionary<GameObject, Creature>();
    //Dictionary<GameObject, CreatureBehaviour> _steeringPinguses = new Dictionary<GameObject, CreatureBehaviour>();
    List<BridgeCreationBehaviour> _bridgesCreating = new List<BridgeCreationBehaviour>();
    List<StairCreationBehaviour> _stairsCreating = new List<StairCreationBehaviour>();
    Creature _selectedPingus;

    bool _startCreation = false;
    //bool _justStarted = true;

    public static bool IsStartPosition(GameObject gameObject)
    {
        return !string.IsNullOrEmpty(gameObject.name) && gameObject.name.StartsWith(STARTPOSITION_OBJECTNAME);
    }

    public static bool IsEndPosition(GameObject gameObject)
    {
        return !string.IsNullOrEmpty(gameObject.name) && gameObject.name.StartsWith(ENDPOSITION_OBJECTNAME);
    }

    // Use this for initialization
    void Start()
    {
        PingusPrototype.SetActive(false);
        BridgePrototype.SetActive(false);
        StairUpPrototype.SetActive(false);
        StairDownPrototype.SetActive(false);
        HitCursor.SetActive(false);

        FloorMask = (1 << LAYER_FLOOR); // SpatialMappingManager.Instance.LayerMask;
        PathModifierMask = (1 << LAYER_PATHMODIFIER);

        //_pathFactory.Clear();
        _walkingPinguses.Clear();
        _currentNumberOfCreatures = 0;

        //_keywords.Add(VOICECOMMAND_LEFT, SetSelectedToLeft);
        //_keywords.Add(VOICECOMMAND_RICHT, SetSelectedToRight);
        //_keywords.Add(VOICECOMMAND_BRIDGE, SetSelectedBuildBridge);
        //_keywords.Add(VOICECOMMAND_UP, SetSelectedBuildStairsUp);
        //_keywords.Add(VOICECOMMAND_DOWN, SetSelectedBuildStairsDown);

        //_keywordRecognizer = new KeywordRecognizer(_keywords.Keys.ToArray());
        //_keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        //_keywordRecognizer.Start();
    }

    //private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    //{
    //    System.Action keywordAction;
    //    if (_keywords.TryGetValue(args.text, out keywordAction))
    //    {
    //        keywordAction.Invoke();
    //    }
    //}

    private void SetSelectedToLeft()
    {
        //Debug.Log("You said LEFT !!!");
        if (_selectedPingus != null)
        {
            //Debug.Log("You said LEFT to a pingus !!!");

            _selectedPingus.Function = Creature.CreatureFunction.TransitionGoLeft;
            //_pathFactory.GoLeft(_selectedPingus.Position);
        }
    }

    private void SetSelectedToRight()
    {
        //Debug.Log("You said RIGHT !!!");
        if (_selectedPingus != null)
        {
            //Debug.Log("You said RIGHT to a pingus !!!");

            _selectedPingus.Function = Creature.CreatureFunction.TransitionGoRight;
            //_pathFactory.GoRight(_selectedPingus.Position);
        }
    }

    private void SetSelectedBuildBridge()
    {
        //Debug.Log("You said BRIDGE !!!");
        if (_selectedPingus != null)
        {
            //Debug.Log("You said BRIDGE to a pingus !!!");

            _selectedPingus.Function = Creature.CreatureFunction.BuildingBridge;

            BridgeCreationBehaviour bridge = CreateBridgeAtPosition(_selectedPingus.Position, _selectedPingus.Direction, Time.time);
            _bridgesCreating.Add(bridge);
        }
    }

    private void SetSelectedBuildStairsUp()
    {
        //Debug.Log("You said UP !!!");
        if (_selectedPingus != null)
        {
            //Debug.Log("You said UP to a pingus !!!");

            _selectedPingus.Function = Creature.CreatureFunction.BuildingStairUp;

            StairCreationBehaviour stair = CreateStairUpAtPosition(_selectedPingus.Position, _selectedPingus.Direction, Time.time);
            _stairsCreating.Add(stair);
        }

    }

    private void SetSelectedBuildStairsDown()
    {
        //Debug.Log("You said DOWN !!!");
        if (_selectedPingus != null)
        {
            //Debug.Log("You said DOWN to a pingus !!!");

            _selectedPingus.Function = Creature.CreatureFunction.BuildingStairDown;

            StairCreationBehaviour stair = CreateStairDownAtPosition(_selectedPingus.Position, _selectedPingus.Direction, Time.time);
            _stairsCreating.Add(stair);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_startCreation)
        {

            HitCursor.SetActive(true);

            //if (_justStarted) {
            //    _justStarted = !_justStarted;
            //    _pathFactory.SetStart(_startPosition, _movingDirection);
            //}

            if (Input.GetKeyUp(KeyCode.L))
            {
                SetSelectedToLeft();
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                SetSelectedToRight();
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                SetSelectedBuildBridge();
            }
            if (Input.GetKeyUp(KeyCode.U))
            {
                SetSelectedBuildStairsUp();
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                SetSelectedBuildStairsDown();
            }

            List<BridgeCreationBehaviour> bridgesBuild = new List<BridgeCreationBehaviour>();
            foreach (BridgeCreationBehaviour creatingBridge in _bridgesCreating)
            {
                BridgeCreationBehaviour.BuildingState state = creatingBridge.Build(Time.time);
                if (state == BridgeCreationBehaviour.BuildingState.Done)
                    bridgesBuild.Add(creatingBridge);
            }

            foreach (BridgeCreationBehaviour buildBridge in bridgesBuild)
            {
                _bridgesCreating.Remove(buildBridge);
            }


            //Debug.Log("Handling stairs !!!");

            List<StairCreationBehaviour> stairsBuild = new List<StairCreationBehaviour>();
            foreach (StairCreationBehaviour creatingStair in _stairsCreating)
            {

                //Debug.Log("Handling building stair" + creatingStair.Name + " !!!");

                StairCreationBehaviour.BuildingState state = creatingStair.Build(Time.time);
                if (state == StairCreationBehaviour.BuildingState.Done)
                    stairsBuild.Add(creatingStair);
            }
            //Debug.Log("Removing stair !!!");

            foreach (StairCreationBehaviour buildStair in stairsBuild)
            {
                //Debug.Log("Handling removal of a stair !!!");
                _stairsCreating.Remove(buildStair);
            }
            //Debug.Log("Handed building stairs !!!");


            if ((_currentNumberOfCreatures < _maxNumberOfCreatures) && (Time.time - _lastPingusCreationTime > _pingusCreationInerval))
            {

                GameObject pingus = CreatePingusAtPosition(_startPosition, _movingDirection, Time.time);
                _walkingPinguses.Add(pingus, pingus.GetComponent<Creature>());
                _currentNumberOfCreatures++;

                _lastPingusCreationTime = Time.time;
            }

            List<GameObject> pingusesArrived = new List<GameObject>();
            //bool processingFirst = true;
            foreach (KeyValuePair<GameObject, Creature> pingus in _walkingPinguses)
            {
                pingus.Value.State = Creature.CreatureState.Normal;

            }

            Ray ray = new Ray(TheMainCamera.transform.position, TheMainCamera.transform.forward);
            if (GamePlay.Testing)
                ray = TheMainCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit pingusHitInfo;
            if (Physics.Raycast(
                    ray
                    , out pingusHitInfo
                    , 20.0f
                    ))
            {

                HitCursor.transform.position = pingusHitInfo.point;

                GameObject hitObject = pingusHitInfo.collider.gameObject;
                //Debug.Log(hitObject.name);

                Creature pingus;
                if (Creature.IsCreature(hitObject) && _walkingPinguses.TryGetValue(hitObject, out pingus))
                {
                    pingus.State = Creature.CreatureState.Selected;
                    _selectedPingus = pingus;
                    //Debug.Log("You hit a pingus !!!");
                }

            }
        }
    }

    public void StartCreation(Vector3 startPosition, Vector3 endPosition)
    {
        _startPosition = startPosition;
        _endPosition = endPosition;

        _movingDirection = GetInitialMovingDirection();

        _startCreation = true;
    }

    Vector3 GetInitialMovingDirection()
    {
        var angle = Random.Range(0f, 360.0f);

        Debug.Log(string.Format("Initial angle {0}", angle));

        Vector3 movingDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        return movingDirection;

        //Vector3 targetPosition = new Vector3(
        //    _endPosition.x,
        //    _startPosition.y,
        //    _endPosition.z);

        //Vector3 movingDirection = targetPosition - _startPosition;
        //return movingDirection.normalized;
    }

    GameObject CreatePingusAtPosition(Vector3 position, Vector3 direction, float creationTime)
    {
        GameObject pingusObject = Instantiate(PingusPrototype);

        Creature pingus = pingusObject.GetComponent<Creature>();
        pingus.State = Creature.CreatureState.Normal;
        pingus.Function = Creature.CreatureFunction.Normal;
        pingus.Position = position;
        pingus.Direction = direction;

        pingusObject.SetActive(true);

        return pingusObject;
    }

    BridgeCreationBehaviour CreateBridgeAtPosition(Vector3 position, Vector3 direction, float creationTime)
    {
        GameObject bridgeObject = Instantiate(BridgePrototype);

        BridgeCreationBehaviour bridge = new BridgeCreationBehaviour();
        bridge.Bridge = bridgeObject;
        bridge.CreationTime = creationTime;
        bridge.Position = position;
        bridge.Direction = direction;

        bridgeObject.SetActive(true);

        return bridge;
    }

    StairCreationBehaviour CreateStairUpAtPosition(Vector3 position, Vector3 direction, float creationTime)
    {
        GameObject stairUpObject = Instantiate(StairUpPrototype);

        StairCreationBehaviour stairUp = new StairCreationBehaviour();
        stairUp.Name = "UP";
        stairUp.Stair = stairUpObject;
        stairUp.CreationTime = creationTime;
        stairUp.Position = position;
        Debug.Log("setting inclination of newly created Stair Up");
        stairUp.Inclination = StairCreationBehaviour.InclinationDirection.Up;
        stairUp.Direction = direction;

        stairUpObject.SetActive(true);

        return stairUp;
    }

    StairCreationBehaviour CreateStairDownAtPosition(Vector3 position, Vector3 direction, float creationTime)
    {
        GameObject stairDownObject = Instantiate(StairDownPrototype);

        StairCreationBehaviour stairDown = new StairCreationBehaviour();
        stairDown.Name = "DOWN";
        stairDown.Stair = stairDownObject;
        stairDown.CreationTime = creationTime;
        stairDown.Position = position;
        Debug.Log("setting inclination of newly created Stair Down");
        stairDown.Inclination = StairCreationBehaviour.InclinationDirection.Down;
        stairDown.Direction = direction;

        stairDownObject.SetActive(true);

        return stairDown;
    }
}
