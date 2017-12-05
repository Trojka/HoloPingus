using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour {

    public enum BuildingState
    {
        Building,
        Done
    }

    private BuildingState _state = BuildingState.Building;

    private const string _name = "Bridge";
    private static float _bridgeMaxLength = 0.8f;
    private static float _buildingSpeed = 0.2f;

    private float _creationTime;
    private Vector3 _direction;

    public static bool IsBridge(GameObject gameObject)
    {
        return !string.IsNullOrEmpty(gameObject.name) && gameObject.name.StartsWith(_name);
    }

    //public BuildingState State
    //{
    //    get { return _state; }
    //    set
    //    {
    //        _state = value;
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
        set
        {
            _direction = value;
            this.transform.rotation = Quaternion.LookRotation(_direction);
        }
    }

    private void Build(float timePassed)
    {
        if (_state == BuildingState.Building)
        {
            //float timeBuilding = atTime - _creationTime;
            float bridgeGrowth = timePassed * _buildingSpeed;
            float oldBridgeLength = this.transform.localScale.z;
            float newBridgeLength = this.transform.localScale.z + bridgeGrowth;
            //if (WillBumpIntoSomething(bridgeLength))
            //{
            //    State = BuildingState.Done;
            //}
            //else
            {

                if (newBridgeLength >= _bridgeMaxLength)
                {
                    //bridgeLength = _bridgeMaxLength;
                    newBridgeLength = _bridgeMaxLength;
                    _state = BuildingState.Done;
                }

                float finalGrowth = newBridgeLength - oldBridgeLength;
                this.transform.position = Position + (_direction * finalGrowth / 2f);
                this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, newBridgeLength);

            }
        }

        //return State;
    }

    // Use this for initialization
    void Start ()
    {
        Debug.Log("Bridge: Start method");

        _state = BuildingState.Building;
        _creationTime = Time.time;
        this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, 0);
    }

    // Update is called once per frame
    void Update ()
    {
        Build(Time.deltaTime);
    }
}
