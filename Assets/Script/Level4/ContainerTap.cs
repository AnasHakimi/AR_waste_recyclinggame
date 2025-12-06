using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerTap : MonoBehaviour
{
    void OnMouseDown()
    {
#if UNITY_EDITOR || UNITY_ANDROID
        Level4Manager.Instance?.OnContainerTapped();
#endif
    }
}


