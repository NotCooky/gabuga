using UnityEngine;

public class MoveCamera : MonoBehaviour {

    public Transform cameraPos;
    void Update() 
    {
        transform.position = cameraPos.position;
    }
}
