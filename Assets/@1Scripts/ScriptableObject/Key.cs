using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Key.asset", menuName = "Data/KeyData")]
public class Key : ScriptableObject
{
    public KeyCode W = KeyCode.W;
    public KeyCode A = KeyCode.A;
    public KeyCode S = KeyCode.S;
    public KeyCode D = KeyCode.D;
    public KeyCode Q = KeyCode.Q; //detect
    public KeyCode E = KeyCode.E; //invisible
    public KeyCode C = KeyCode.C; //crouch
    public KeyCode Shift = KeyCode.LeftShift; //run
    public KeyCode Space = KeyCode.Space; // jump
    public KeyCode T = KeyCode.T; // TestMode
}
