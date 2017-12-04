using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{

    public enum CreatureState
    {
        Normal,
        Selected
    }

    public enum CreatureFunction
    {
        Normal,
        TransitionGoLeft,
        GoLeft,
        TransitionGoRight,
        GoRight,
        BuildingBridge,
        BuildBridge,
        BuildingStairUp,
        BuildStairUp,
        BuildingStairDown,
        BuildStairDown,
        Dead
    }

    private CreatureState _state;
    private CreatureFunction _function;

    private const string _name = "Pingus";
    private const float _movingSpeed = 0.1f;
    private const float _rotationSpeed = 5; // degrees
    private const float _maxFallHeight = 0.2f;

    private float _bodyHeight = 0.18f;
    private float _faceDistance = 0.04f;
    private float _space = 0.1f;

    private const float _buildingDuration = 2;
    private float _buildingStartTime;

    private const float _rotate180Duration = 2;
    private float _rotateStartTime;

    private Vector3 _direction;

    public Material MainMaterial;
    public Material SelectedMaterial;

    public static bool IsCreature(GameObject gameObject)
    {
        return !string.IsNullOrEmpty(gameObject.name) && gameObject.name.StartsWith(_name);
    }

    public CreatureState State {
        get { return _state; }
        set {
            _state = value;
        }
    }

    public CreatureFunction Function {
        get { return _function; }
        set {
            _function = value;
            if (_function == CreatureFunction.TransitionGoLeft
                || _function == CreatureFunction.TransitionGoRight)
                _rotateStartTime = Time.time;
            if (_function == CreatureFunction.BuildingBridge
                || _function == CreatureFunction.BuildingStairUp
                || _function == CreatureFunction.BuildingStairDown)
                _buildingStartTime = Time.time;
        }
    }

    public Vector3 Position {
        get { return this.transform.position; }
        set { this.transform.position = value; }
    }

    public Vector3 Direction {
        get { return _direction; }
        set {
            _direction = value;
            this.transform.rotation = Quaternion.LookRotation(_direction);
        }
    }

    private void UpdatePosture(float timePassed)
    {

        if (Function == CreatureFunction.Dead)
            return;

        if (Function == CreatureFunction.GoLeft
            || Function == CreatureFunction.GoRight)
            return;

        if (Function == CreatureFunction.BuildingBridge
                || Function == CreatureFunction.BuildingStairUp
                || Function == CreatureFunction.BuildingStairDown)
        {
            float buildingTimeDelta = Time.time - _buildingStartTime;
            if (buildingTimeDelta > _buildingDuration)
            {
                switch (Function)
                {
                    case CreatureFunction.BuildingBridge:
                        Function = CreatureFunction.BuildBridge;
                        break;
                    case CreatureFunction.BuildingStairUp:
                        Function = CreatureFunction.BuildStairUp;
                        break;
                    case CreatureFunction.BuildingStairDown:
                        Function = CreatureFunction.BuildStairDown;
                        break;
                }
            }

            return;
        }

        if (Function == CreatureFunction.TransitionGoLeft)
        {

            Vector3 eulerDirection = Quaternion.LookRotation(_direction).eulerAngles;
            float eulerDirectionYRotation = eulerDirection.y;

            float rotationTimeDelta = Time.time - _rotateStartTime;
            float rotationDelta = rotationTimeDelta * 180 / _rotate180Duration;
            if (rotationTimeDelta > _rotate180Duration)
            {
                rotationDelta = 180;
                Function = CreatureFunction.GoLeft;
            }

            this.transform.rotation = Quaternion.Euler(0, eulerDirectionYRotation - rotationDelta, 0);

            return;
        }

        if (Function == CreatureFunction.TransitionGoRight)
        {

            Vector3 eulerDirection = Quaternion.LookRotation(_direction).eulerAngles;
            float eulerDirectionYRotation = eulerDirection.y;

            float rotationTimeDelta = Time.time - _rotateStartTime;
            float rotationDelta = rotationTimeDelta * 180 / _rotate180Duration;
            if (rotationTimeDelta > _rotate180Duration)
            {
                rotationDelta = 180;
                Function = CreatureFunction.GoRight;
            }

            this.transform.rotation = Quaternion.Euler(0, eulerDirectionYRotation + rotationDelta, 0);

            return;
        }

        Vector3 forwardBumpHeadPollingPosition = this.transform.position + (Vector3.up * _bodyHeight);
        RaycastHit forwardBumpHeadHitInfo;
        if (Physics.Raycast(
            forwardBumpHeadPollingPosition,
            Direction,
            out forwardBumpHeadHitInfo,
            _faceDistance
            ))
        {

            //Debug.Log("It is about to bump its head into something !!!");
            return;

        }

        Vector3 forwardBumpBodyPollingPosition = this.transform.position + (Vector3.up * (_bodyHeight / 2));
        RaycastHit forwardBumpBodyHitInfo;
        if (Physics.Raycast(
            forwardBumpBodyPollingPosition,
            Direction,
            out forwardBumpBodyHitInfo,
            _space
            ))
        {

            GameObject hitObject = forwardBumpBodyHitInfo.collider.gameObject;
            //Debug.Log("It is about to walk into something !!!");
            if (Creature.IsCreature(hitObject))
            {
                //Debug.Log("It is about to walk into another creature !!!");
                Creature creature = hitObject.GetComponent<Creature>();
                if (creature.Function == CreatureFunction.GoLeft)
                {
                    Direction = Quaternion.AngleAxis(-90, Vector3.up) * Direction;
                }
                else if (creature.Function == CreatureFunction.GoRight)
                {
                    Direction = Quaternion.AngleAxis(90, Vector3.up) * Direction;
                }
                return;
            }
        }

        float distanceTravelled = _movingSpeed * timePassed;

        Vector3 nextPosition = this.transform.position + Direction * distanceTravelled;
        Vector3 groundPollingPosition = nextPosition + (Vector3.up * _bodyHeight);

        int checkLayerMask = CreatureManager.FloorMask | CreatureManager.PathModifierMask;

        RaycastHit groundHitInfo;
        if (Physics.Raycast(
            groundPollingPosition,
            Vector3.down,
            out groundHitInfo,
            20.0f,
            checkLayerMask))
        {

            GameObject hitObject = groundHitInfo.collider.gameObject;

            //Debug.Log("Pingus hit gameobject: " + hitObject.name);

            //if (BridgeCreationBehaviour.IsBridge(hitObject)) {
            //    //Debug.Log("It stepped on the bridge !!!");
            //}

            if (CreatureManager.IsEndPosition(hitObject))
            {
                float distanceToEnd = Vector3.Distance(hitObject.transform.position, groundHitInfo.point);
                if (distanceToEnd < CreatureManager._endDistance)
                {
                    Function = CreatureFunction.Dead;
                    GameObject gameObject = this.gameObject;
                    gameObject.SetActive(false);
                }
            }

            if ((this.transform.position.y - groundHitInfo.point.y) > _maxFallHeight)
            {
                Function = CreatureFunction.Dead;
                //GameObject gameObject = this.gameObject;
                //gameObject.SetActive(false);
            }

            nextPosition = groundHitInfo.point;
        }

        this.transform.position = nextPosition;
    }

    private void DrawCreature()
    {
        switch (_function)
        {
            case CreatureFunction.Dead:
                foreach (Transform child in this.transform)
                {
                    string childName = child.gameObject.name;
                    if (childName.StartsWith("Blood"))
                        child.gameObject.SetActive(true);
                    else
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                break;
            default:
                switch (_state)
                {
                    case CreatureState.Normal:
                        foreach (Transform child in this.transform)
                        {
                            string childName = child.gameObject.name;
                            if (childName.StartsWith("Blood"))
                                child.gameObject.SetActive(false);
                            else if (childName.StartsWith("L_Arm") &&
                                (Function == CreatureFunction.GoLeft || Function == CreatureFunction.BuildBridge))
                                child.gameObject.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
                            else if (childName.StartsWith("R_Arm") &&
                                (Function == CreatureFunction.GoRight || Function == CreatureFunction.BuildBridge))
                                child.gameObject.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
                            else
                            {
                                Renderer renderer = child.gameObject.GetComponent<Renderer>();
                                if (renderer != null)
                                    renderer.material = MainMaterial;

                                foreach (Transform nestedChild in child.gameObject.transform)
                                {
                                    Renderer nestedRenderer = nestedChild.gameObject.GetComponent<Renderer>();
                                    if (nestedRenderer != null)
                                        nestedRenderer.material = MainMaterial;
                                }

                            }
                        }
                        break;
                    case CreatureState.Selected:
                        foreach (Transform child in this.transform)
                        {
                            string childName = child.gameObject.name;
                            if (childName.StartsWith("Blood"))
                                child.gameObject.SetActive(false);
                            else
                            {
                                Renderer renderer = child.gameObject.GetComponent<Renderer>();
                                if (renderer != null)
                                    renderer.material = SelectedMaterial;

                                foreach (Transform nestedChild in child.gameObject.transform)
                                {
                                    Renderer nestedRenderer = nestedChild.gameObject.GetComponent<Renderer>();
                                    if (nestedRenderer != null)
                                        nestedRenderer.material = SelectedMaterial;
                                }
                            }

                        }
                        break;
                }

                break;
        }
    }

    void Start()
    {
        foreach (Transform child in this.transform)
        {
            string childName = child.gameObject.name;
            if (childName.StartsWith("Blood"))
                child.gameObject.SetActive(false);
            else
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    void Update()
    {
        UpdatePosture(Time.deltaTime);
        DrawCreature();
    }
}
