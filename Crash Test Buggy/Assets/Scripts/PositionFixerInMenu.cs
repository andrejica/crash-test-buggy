using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionFixerInMenu : MonoBehaviour
{
    void LateUpdate()
    {
        transform.position = new Vector3(0.0f, transform.position.y, 0.0f);
    }
}
