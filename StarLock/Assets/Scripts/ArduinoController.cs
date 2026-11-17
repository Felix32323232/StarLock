using System;
using System.IO.Ports;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    [Header("Configuración Puerto Serie")]
    public string portName = "COM6";
    public int baudRate = 9600;
    public float smoothSpeed = 10f;

    private SerialPort stream;
    private float pitch = 0f;
    private float roll = 0f;

    void Start()
    {
        stream = new SerialPort(portName, baudRate);
        try
        {
            stream.ReadTimeout = 50;
            stream.DtrEnable = true;
            stream.RtsEnable = true;
            stream.Open();
            Debug.Log("Puerto serie abierto correctamente.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error al abrir puerto serie: " + e.Message);
        }
    }

    void Update()
    {
        if (stream != null && stream.IsOpen)
        {
            try
            {
                // Leer y vaciar el buffer serie acumulado para tomar solo la última línea recibida
                string latestLine = null;
                while (stream.BytesToRead > 0)
                {
                    latestLine = stream.ReadLine();
                }

                if (!string.IsNullOrEmpty(latestLine))
                {
                    ProcesarLectura(latestLine);
                }
            }
            catch (TimeoutException) { }
            catch (Exception e)
            {
                Debug.LogError("Error en la lectura serie: " + e.Message);
            }

            // Aplicar la rotación de forma suave
            Quaternion targetRotation = Quaternion.Euler(pitch, 0f, -roll);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }

    void ProcesarLectura(string rawData)
    {
        string cleanData = rawData.Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");

        // Detección del comando DISPARO definido en la bitácora
        if (cleanData.Equals("DISPARO", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[EVENTO] Disparo detectado desde el pulsador físico.");
            return;
        }

        // Eliminar prefijo "MOT:" si viene en la trama según el protocolo
        if (cleanData.StartsWith("MOT:", StringComparison.OrdinalIgnoreCase))
        {
            cleanData = cleanData.Substring(4);
        }

        string[] values = cleanData.Split(',');

        if (values.Length >= 2)
        {
            bool pitchOk = float.TryParse(values[0].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float nuevoPitch);
            bool rollOk = float.TryParse(values[1].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float nuevoRoll);

            if (pitchOk && rollOk)
            {
                pitch = nuevoPitch;
                roll = nuevoRoll;
            }
        }
    }

    void OnDisable()
    {
        if (stream != null && stream.IsOpen)
        {
            stream.Close();
        }
    }
}