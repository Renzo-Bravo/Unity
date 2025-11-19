using UnityEngine;
using System.IO.Ports;

public class LeverToServoSerial : MonoBehaviour
{
    [Header("References")]
    public LeverController lever;   // arrastrá tu script aquí

    [Header("Serial Settings")]
    public string portName = "COM3"; 
    public int baudRate = 9600;

    private SerialPort serial;

    void Start()
    {
        serial = new SerialPort(portName, baudRate);
        serial.ReadTimeout = 50;
        serial.Open();
    }

    void Update()
    {
        if (serial != null && serial.IsOpen)
        {
            float leverAngle = lever.GetCurrentRotation();

            // Convertimos -45 / +45 a 0 / 180
            float normalized = Mathf.InverseLerp(lever.MinRotationAngle, lever.MaxRotationAngle, leverAngle);
            int servoAngle = Mathf.RoundToInt(normalized * 180f);

            serial.WriteLine(servoAngle.ToString());
        }
    }

    private void OnApplicationQuit()
    {
        if (serial != null && serial.IsOpen)
        {
            serial.Close();
        }
    }
}
