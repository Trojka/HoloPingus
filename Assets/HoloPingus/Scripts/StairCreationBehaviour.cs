using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairCreationBehaviour
{

    public enum BuildingState
    {
        Building,
        Done
    }

    public enum InclinationDirection
    {
        Up,
        Down
    }

    private const string _name = "Stair";
    private const float _stairMaxLength = 0.8f;
    private const float _buildingSpeed = 0.2f;

    private float _inclinationAngle = Mathf.PI / 4f;

    public string Name;

    public GameObject Stair;
    public float CreationTime;

    private Vector3 _direction;
    private Vector3 _position;

    private BuildingState _state = BuildingState.Building;
    private InclinationDirection _inclination = InclinationDirection.Up;

    public static bool IsStair(GameObject gameObject)
    {
        return !string.IsNullOrEmpty(gameObject.name) && gameObject.name.StartsWith(_name);
    }

    public BuildingState Build(float atTime)
    {
        if (State == BuildingState.Building)
        {
            //Debug.Log("Stair: " + Name + ", updating - CreationTime: " + CreationTime);

            float timeBuilding = atTime - CreationTime;
            float stairLength = timeBuilding * _buildingSpeed;


            if (WillBumpIntoSomething(stairLength))
            {
                State = BuildingState.Done;
            }
            else
            {

                if (stairLength >= _stairMaxLength)
                {
                    stairLength = _stairMaxLength;
                    State = BuildingState.Done;
                }

                //Debug.Log("Stair: " + Name + ", updating - position: " + _position.ToString());
                //Debug.Log("Stair: " + Name + ", updating - inclinationAngle: " + _inclinationAngle);
                //Debug.Log("Stair: " + Name + ", updating - stairLength: " + stairLength);

                Stair.transform.position = _position
                    + (Mathf.Cos(_inclinationAngle) * _direction * stairLength / 2f)
                    + (Mathf.Sin(_inclinationAngle) * Vector3.up * stairLength / 2f);
                //Debug.Log("Stair: " + Name + ", updating position to: " + Stair.transform.position.ToString());
                Stair.transform.localScale = new Vector3(Stair.transform.localScale.x, Stair.transform.localScale.y, stairLength);

            }
        }

        return State;
    }

    private bool WillBumpIntoSomething(float newLength)
    {
        float currentStairLength = Stair.transform.localScale.z;
        float growth = newLength - currentStairLength;
        Vector3 forwardBumpPollingPosition = _position
            + (Mathf.Cos(_inclinationAngle) * _direction * currentStairLength)
            + (Mathf.Sin(_inclinationAngle) * Vector3.up * currentStairLength);
        Vector3 inclinationDirection = new Vector3(_direction.x, _direction.y, Mathf.Sin(_inclinationAngle));
        RaycastHit forwardBumpHitInfo;
        if (Physics.Raycast(
            forwardBumpPollingPosition,
            inclinationDirection,
            out forwardBumpHitInfo,
            growth
            ))
        {
            return true;
        }

        return false;
    }

    public Vector3 Position {
        get { return _position; }
        set {
            _position = value;
            Stair.transform.position = _position;

            //Debug.Log("Stair: " + Name + ", setting position: " + Stair.transform.position.ToString());

        }
    }

    public Vector3 Direction {
        get { return _direction; }
        set {
            _direction = value;
            Quaternion rotation = Quaternion.LookRotation(_direction);
            rotation = rotation * Quaternion.LookRotation(new Vector3(0, Mathf.Sin(_inclinationAngle), Mathf.Cos(_inclinationAngle)));
            Stair.transform.rotation = rotation;
        }
    }

    public BuildingState State {
        get { return _state; }
        set {
            _state = value;
        }
    }

    public InclinationDirection Inclination {
        get { return _inclination; }
        set {
            _inclination = value;
            //Debug.Log("Stair: " + Name + ", set - _inclination: " + _inclination);
            switch (_inclination)
            {
                case InclinationDirection.Up:
                    _inclinationAngle = Mathf.PI / 4f;
                    //Debug.Log("Stair: " + Name + ", set - inclinationAngle: " + _inclinationAngle);
                    break;
                case InclinationDirection.Down:
                    _inclinationAngle = -1 * Mathf.PI / 4f;
                    //Debug.Log("Stair: " + Name + ", set - inclinationAngle: " + _inclinationAngle);
                    break;
            }
        }
    }

}
