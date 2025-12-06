using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupAnimation : MonoBehaviour
{
    public void Popupanimation()
    {
        GetComponent<Animation>().Play("PopUp");
        AudioManager.instance?.PlaySFX("PopUp");
    }
}
