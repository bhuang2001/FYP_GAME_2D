using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeManager : MonoBehaviour
{
    // To store game object of the button to change to green and red colour when toggling between modes 
    // Green is for interactive mode (Default) and red is for non-interactive mode 
    public Color interactiveColour, nonInteractiveColour, highlightedColour;
    public GameObject toggleMode;
    public Button button;

    public bool nonInteractive;
    private GameObject modeManager;
    // Start is called before the first frame update
    void Start()
    {
        nonInteractive = false;
        modeManager = GameObject.Find("Mode Manager");
        button = toggleMode.GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    // Function for button - ToggleMode()

    public void ToggleMode()
    {
        // turn on non interactive mode
        if (nonInteractive == false)
        {
            nonInteractive = true;
            AddDemoModeScript(modeManager);
            // initializing colour block to red 
            ColorBlock cb = button.colors;
            cb.normalColor = nonInteractiveColour;
            cb.highlightedColor = highlightedColour;
            cb.pressedColor = nonInteractiveColour;
            cb.selectedColor = nonInteractiveColour;
            button.colors = cb;
        }
        // turn off non interactive mode
        else if (nonInteractive == true)
        {
            nonInteractive = false;
            RemoveDemoModeScript(modeManager);
            // initializing colour block to a shade of green
            ColorBlock cb = button.colors;
            cb.normalColor = interactiveColour;
            cb.highlightedColor = highlightedColour;
            cb.pressedColor = interactiveColour;
            cb.selectedColor = interactiveColour;
            button.colors = cb;
        }
    }

    // Function to add the demo mode script
    private void AddDemoModeScript(GameObject obj)
    {
        obj.AddComponent<DemoMode>();
    }

    // Function to remove the demo mode script
    private void RemoveDemoModeScript(GameObject obj)
    {
        DemoMode script = obj.GetComponent<DemoMode>();
        Destroy(script);
    }


}
