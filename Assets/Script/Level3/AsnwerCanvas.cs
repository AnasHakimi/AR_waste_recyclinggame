using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerCanvas : MonoBehaviour
{
    public Level3Manager level3Manager;
    public int answerIndex;

    public void OnUserTap()
    {

        level3Manager.HandleAnswer(answerIndex);
    }
}
