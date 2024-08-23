using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DebugUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text speedText;

    [Header("Reference")]
    public GameObject playerObj;

    private void FixedUpdate()
    {
        speedText.text = playerObj.GetComponent<Rigidbody>().velocity.magnitude.ToString("F2");
    }
}
