using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class V3PIDController
{
    [NonSerialized]
    private Vector3 lastError = new Vector3();
    [NonSerialized]
    private Vector3 integral = new Vector3();
    //PID parameters
    public float P = 0f;
    public float I = 0f;
    public float D = 0f;
    public bool clampIntegral;
    public float maxIntegralMagnitude;
    public float averageAmount = 20.0f;
    public Vector3 GetOutput(Vector3 error)
    {
        Vector3 derivative = (error - lastError) / Time.deltaTime;
        integral += error * Time.deltaTime;
        if (clampIntegral)
        {
            integral = Vector3.ClampMagnitude(integral, maxIntegralMagnitude);
        }
        integral += (error - integral) / averageAmount;
        lastError = error;
        return P * error + I * integral + D * derivative;
    }

    public void Reset()
    {
        integral = Vector3.zero;
        lastError = Vector3.zero;
    }
}