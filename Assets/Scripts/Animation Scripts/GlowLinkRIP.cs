using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowLinkRIP : MonoBehaviour
{
    public float timePassed = 0f;
    public float onDuration = 0.5f;
    public int level;
    private bool finished = false;
    private Color blue = Color.blue;
    private Color white = Color.white;
    // Start is called before the first frame update
    void Start()
    {
        float waitTime = level * 0.5f;
        float finishTime = waitTime + 0.5f;
        Invoke("GlowLinks",waitTime);
        Invoke("DisableGlow", finishTime);

    }

    // Update is called once per frame
    void Update()
    {
        if(finished == true)
        {
            Destroy(GetComponent<GlowLinkRIP>());
        }
    }


    private void GlowLinks()
    {
        LineRenderer lr;
        lr = GameObject.Find(this.gameObject.name).GetComponent<LineRenderer>();
        SetColourGradient(blue,lr);
        
    }

    // Returns the links back to white colour
    private void DisableGlow()
    {    
    LineRenderer lr;
    lr = GameObject.Find(this.gameObject.name).GetComponent<LineRenderer>();
    SetColourGradient(white,lr);
    finished = true;
        
    }

    // Sets colour of line
    private void SetColourGradient(Color colour, LineRenderer lr) 
    {
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(colour, 0.0f), new GradientColorKey(colour, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lr.colorGradient = gradient;
    }
}
