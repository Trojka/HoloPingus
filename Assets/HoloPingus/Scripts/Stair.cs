using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : MonoBehaviour {

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

    private BuildingState _state = BuildingState.Building;
    public InclinationDirection _inclination = InclinationDirection.Up;

    private const string _name = "Stair";
    private const float _stairMaxLength = 0.8f;
    private const float _buildingSpeed = 0.2f;

    private float _inclinationAngle = 0.0f;

    private float _creationTime;
    private Vector3 _direction;

    public static bool IsStair(GameObject gameObject)
    {
        return !string.IsNullOrEmpty(gameObject.name) && gameObject.name.StartsWith(_name);
    }

    public string Name
    {
        get;
        set;
    }

    //public BuildingState State
    //{
    //    get { return _state; }
    //    set
    //    {
    //        _state = value;
    //    }
    //}

    //public InclinationDirection Inclination
    //{
    //    get { return _inclination; }
    //    set
    //    {
    //        _inclination = value;
    //        //Debug.Log("Stair: " + Name + ", set - _inclination: " + _inclination);
    //        switch (_inclination)
    //        {
    //            case InclinationDirection.Up:
    //                _inclinationAngle = Mathf.PI / 4f;
    //                //Debug.Log("Stair: " + Name + ", set - inclinationAngle: " + _inclinationAngle);
    //                break;
    //            case InclinationDirection.Down:
    //                _inclinationAngle = -1 * Mathf.PI / 4f;
    //                //Debug.Log("Stair: " + Name + ", set - inclinationAngle: " + _inclinationAngle);
    //                break;
    //        }
    //    }
    //}

    public Vector3 Position
    {
        get { return this.transform.position; }
        set { this.transform.position = value; }
    }

    public Vector3 Direction
    {
        get { return _direction; }
        set { _direction = value; }
    }

    public void Build(float timePassed)
    {
        if (_state == BuildingState.Building)
        {
            //Debug.Log("Stair: " + Name + ", updating - CreationTime: " + CreationTime);

            //float timeBuilding = atTime - CreationTime;
            float stairGrowth = timePassed * _buildingSpeed;
            float oldStairLength = this.transform.localScale.z;
            float newStairLength = this.transform.localScale.z + stairGrowth;

            //if (WillBumpIntoSomething(stairLength))
            //{
            //    State = BuildingState.Done;
            //}
            //else
            {

                if (newStairLength >= _stairMaxLength)
                {
                    newStairLength = _stairMaxLength;
                    _state = BuildingState.Done;
                }

                //Debug.Log("Stair: " + Name + ", updating - position: " + _position.ToString());
                //Debug.Log("Stair: " + Name + ", updating - inclinationAngle: " + _inclinationAngle);
                //Debug.Log("Stair: " + Name + ", updating - stairLength: " + stairLength);

                float finalGrowth = newStairLength - oldStairLength;
                this.transform.position = Position
                    + (Mathf.Cos(_inclinationAngle) * _direction * finalGrowth / 2f)
                    + (Mathf.Sin(_inclinationAngle) * Vector3.up * finalGrowth / 2f);
                //Debug.Log("Stair: " + Name + ", updating position to: " + Stair.transform.position.ToString());
                this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, newStairLength);

            }
        }

        //return State;
    }

    void Start ()
    {
        Debug.Log("Stair: Start method");

        _state = BuildingState.Building;
        _creationTime = Time.time;

        switch (_inclination)
        {
            case InclinationDirection.Up:
                _inclinationAngle = Mathf.PI / 4f;
                Debug.Log("Stair: " + Name + ", set - inclinationAngle: " + _inclinationAngle);
                break;
            case InclinationDirection.Down:
                _inclinationAngle = -1 * Mathf.PI / 4f;
                Debug.Log("Stair: " + Name + ", set - inclinationAngle: " + _inclinationAngle);
                break;
        }

        Quaternion rotation = Quaternion.LookRotation(_direction);
        rotation = rotation * Quaternion.LookRotation(new Vector3(0, Mathf.Sin(_inclinationAngle), Mathf.Cos(_inclinationAngle)));
        this.transform.rotation = rotation;
        this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, 0);
    }

    void Update ()
    {
        Build(Time.deltaTime);
    }
}
