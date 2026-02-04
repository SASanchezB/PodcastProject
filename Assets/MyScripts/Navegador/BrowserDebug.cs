using UnityEngine;

public class BrowserDebug : MonoBehaviour
{
    void Start()
    {
        var components = GetComponents<MonoBehaviour>();

        foreach (var c in components)
        {
            //Usalo para saber los componentes
            //Debug.Log("Componente: " + c.GetType().FullName);
        }
    }
}
