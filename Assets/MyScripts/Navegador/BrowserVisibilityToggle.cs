using UnityEngine;
using UnityEngine.UI;

public class BrowserToggle : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;

    private bool visible;

    void Start()
    {
        // Estado inicial seguro
        visible = false;

        Color c = rawImage.color;
        c.a = 0f;
        rawImage.color = c;

        // Bloquea input desde el inicio
        rawImage.raycastTarget = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Toggle();
        }
    }

    void Toggle()
    {
        visible = !visible;

        // Alpha
        Color c = rawImage.color;
        c.a = visible ? 1f : 0f;
        rawImage.color = c;

        // Input ON / OFF
        rawImage.raycastTarget = visible;
    }
}
