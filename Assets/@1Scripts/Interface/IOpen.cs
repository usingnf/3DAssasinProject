using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOpen
{
    void Open(float intensity);
    void Close(float intensity);

    void OpenPerfect(float intensity);
    void ClosePerfect(float intensity);

    float GetAngle();
}
