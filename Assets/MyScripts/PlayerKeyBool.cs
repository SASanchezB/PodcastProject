using UnityEngine;

public class PlayerKeyBool : MonoBehaviour
{

    public bool triggeredButtonKey = false;

    private void Awake()
    {
        triggeredButtonKey = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            triggeredButtonKey = true;
        }
    }
}
