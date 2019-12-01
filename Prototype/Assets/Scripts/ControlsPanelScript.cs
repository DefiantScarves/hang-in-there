using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsPanelScript : MonoBehaviour
{
    public GameObject Panel;
    public GameObject PanelCommands;

    public void OpenPanel()
    {
        if(Panel != null)
        {
            bool isActive = Panel.activeSelf;
            Panel.SetActive(!isActive);
        }

        if (PanelCommands != null)
        {
            bool isActive = PanelCommands.activeSelf;
            PanelCommands.SetActive(!isActive);
        }
    }
}
