using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
public class PlayerSetup : MonoBehaviour
{
    public PlayerStateMachine playerMovementScript;
    public GameObject Camera;
    public CinemachineVirtualCamera CinemachineVirtualCamera;
    public string nickname;
    public TextMeshPro nicknameText;
    public void ToLocalPlayer()
    {
        playerMovementScript.enabled = true;
        CinemachineVirtualCamera.Priority += 1;
        Camera.SetActive(true);
    }
    public void SetNickname(string _name)
    {
        nickname = _name;

        nicknameText.text = nickname;
    }


}
