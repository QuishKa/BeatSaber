using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    private Material CubeMaterial;
    private Material ArrowMaterial;
    public enum CubeOrientation { left, right, up, down }
    public enum CubeColor { blue, red }
    private CubeColor currentColor;
    void Start()
    {
        CubeMaterial = gameObject.GetComponentsInChildren<Renderer>()[1].material;
        CubeMaterial.SetColor("_Color", new Color(0.062745f, 0.180392f, 0.435294f));
        ArrowMaterial = gameObject.GetComponentsInChildren<Renderer>()[0].material;
        ArrowMaterial.SetColor("_Color", new Color(0.705882f, 0.741176f, 0.905882f));
        ArrowMaterial.SetColor("_EmissionColor", new Color(0f, 0.070588f, 0.749019f));
        CurrentColor = CubeColor.blue;
    }

    void Update()
    {
        
    }

    public CubeColor CurrentColor
    {
        get => currentColor;
        set
        {
            currentColor = value;
            if (value == CubeColor.blue)
            {
                CubeMaterial.SetColor("_Color", new Color(0.062745f, 0.180392f, 0.435294f));
                ArrowMaterial.SetColor("_Color", new Color(0.705882f, 0.741176f, 0.905882f));
                ArrowMaterial.SetColor("_EmissionColor", new Color(0f, 0.070588f, 0.749019f));
            }
            if (value == CubeColor.red)
            {
                CubeMaterial.SetColor("_Color", new Color(0.470588f, 0.152941f, 0.113725f));
                ArrowMaterial.SetColor("_Color", new Color(0.925490f, 0.623529f, 0.596078f));
                ArrowMaterial.SetColor("_EmissionColor", new Color(0.749019f, 0f, 0f));
            }
        }
    }

    public CubeOrientation Orientation { get; set; }
}
