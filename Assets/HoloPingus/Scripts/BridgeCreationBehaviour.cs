using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeCreationBehaviour
{

    public enum BuildingState
    {
        Building,
        Done
    }

    private const string _name = "Bridge";
    private static float _bridgeMaxLength = 0.8f;
    private static float _buildingSpeed = 0.2f;

    public GameObject Bridge;
    public float CreationTime;

    private Vector3 _direction;
    private Vector3 _position;

    private BuildingState _state = BuildingState.Building;

    public static bool IsBridge(GameObject gameObject)
    {
        return !string.IsNullOrEmpty(gameObject.name) && gameObject.name.StartsWith(_name);
    }

    public BuildingState Build(float atTime)
    {
        if (State == BuildingState.Building)
        {
            float timeBuilding = atTime - CreationTime;
            float bridgeLength = timeBuilding * _buildingSpeed;

            //if (WillBumpIntoSomething(bridgeLength))
            //{
            //    State = BuildingState.Done;
            //}
            //else
            {

                if (bridgeLength >= _bridgeMaxLength)
                {
                    bridgeLength = _bridgeMaxLength;
                    State = BuildingState.Done;
                }

                Bridge.transform.position = _position + (_direction * bridgeLength / 2f);
                Bridge.transform.localScale = new Vector3(Bridge.transform.localScale.x, Bridge.transform.localScale.y, bridgeLength);

            }
        }

        return State;
    }

    //private bool WillBumpIntoSomething(float newLength)
    //{
    //    float currentBridgeLength = Bridge.transform.localScale.z;
    //    float growth = newLength - currentBridgeLength;
    //    Vector3 forwardBumpPollingPosition = _position + (_direction * currentBridgeLength);
    //    RaycastHit forwardBumpHitInfo;
    //    if (Physics.Raycast(
    //        forwardBumpPollingPosition,
    //        _direction,
    //        out forwardBumpHitInfo,
    //        growth
    //        ))
    //    {

    //        Debug.Log("The bridge will bump into something !");
    //        return true;
    //    }

    //    return false;
    //}

    public Vector3 Position {
        get { return _position; }
        set {
            _position = value;
            Bridge.transform.position = _position;
        }
    }

    public Vector3 Direction {
        get { return _direction; }
        set {
            _direction = value;
            Bridge.transform.rotation = Quaternion.LookRotation(_direction);
        }
    }

    public BuildingState State {
        get { return _state; }
        set {
            _state = value;
        }
    }

}
