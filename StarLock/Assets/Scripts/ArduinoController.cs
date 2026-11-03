using System;
using System.IO.Ports;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    private SerialPort stream = new SerialPort("COM6", 9600);

    private float pitch = 0f;
    private float roll = 0f;

    void Start()
    {
        try
        {
            stream.ReadTimeout = 50;
            stream.DtrEnable = true;
            stream.RtsEnable = true;
            stream.Open();
            Debug.Log("Puerto abierto con éxito.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error al abrir puerto: " + e.Message);
        }
    }

    void Update()
    {
        if (stream != null && stream.IsOpen)
        {
            try
            {
                string value = stream.ReadLine();

                // Limpieza de caracteres invisibles
                value = value.Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");

                if (!string.IsNullOrEmpty(value))
                {
                    string[] values = value.Split(',');

                    // MODIFICADO: Aceptamos que vengan 2 O MÁS valores (como tus 3 valores actuales)
                    if (values.Length >= 2)
                    {
                        // Tomamos el primer valor para Pitch y el segundo para Roll
                        string pitchStr = values[0].Trim();
                        string rollStr = values[1].Trim();

                        // Intentamos convertir usando la configuración del punto decimal (.)
                        bool pitchOk = float.TryParse(pitchStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float nuevoPitch);
                        bool rollOk = float.TryParse(rollStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float nuevoRoll);

                        if (pitchOk && rollOk)
                        {
                            pitch = nuevoPitch;
                            roll = nuevoRoll;

                            // Mensaje de éxito en la consola
                            Debug.Log($"[ÉXITO] Moviendo Cubo -> Pitch: {pitch} | Roll: {roll}");

                            // Aplicamos la rotación al cubo
                            transform.rotation = Quaternion.Euler(pitch, 0f, -roll);
                        }
                        else
                        {
                            Debug.LogWarning($"[ERROR FORMATO] No se pudo convertir a número. Pitch=[{pitchStr}], Roll=[{rollStr}]");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[ERROR DATOS] Faltan datos en la línea. Texto recibido: [{value}]");
                    }
                }
            }
            catch (TimeoutException) { }
            catch (Exception e)
            {
                Debug.LogError("Error inesperado: " + e.Message);
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
