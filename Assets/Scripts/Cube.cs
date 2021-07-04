using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    private Material CubeMaterial;
    private Material ArrowMaterial;
    private enum Orientation { left, right, up, down }
    void Start()
    {
        CubeMaterial = gameObject.GetComponentsInChildren<Renderer>()[1].material;
        CubeMaterial.SetColor("_Color", new Color(0.062745f, 0.180392f, 0.435294f));
        ArrowMaterial = gameObject.GetComponentsInChildren<Renderer>()[0].material;
        ArrowMaterial.SetColor("_Color", new Color(0.705882f, 0.741176f, 0.905882f));
        ArrowMaterial.SetColor("_EmissionColor", new Color(0f, 0.070588f, 0.749019f));
    }

    void Update()
    {
        
    }

    public Color CubeColor
    {
        get => CubeMaterial.color;
        set => CubeMaterial.color = value;
    }
}
