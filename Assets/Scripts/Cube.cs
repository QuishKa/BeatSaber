using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] private GameObject RHalfCube;
    [SerializeField] private GameObject LHalfCube;
    private Material CubeMaterial;
    private Material ArrowMaterial;
    public Quaternion DefaultRotation { get; private set; }
    public enum CubeOrientation { up, rightUp, right, rightDown, down, leftDown, left, leftUp }
    public enum CubeColor { blue, red, gray }
    private CubeColor currentColor;
    void Awake()
    {
        ArrowMaterial = gameObject.GetComponentsInChildren<Renderer>()[0].material;
        ArrowMaterial.SetColor("_Color", new Color(0.705882f, 0.741176f, 0.905882f));
        ArrowMaterial.SetColor("_EmissionColor", new Color(0f, 0.070588f, 0.749019f));
        CubeMaterial = gameObject.GetComponentsInChildren<Renderer>()[1].material;
        CubeMaterial.SetColor("_Color", new Color(0.062745f, 0.180392f, 0.435294f));
        DefaultRotation = gameObject.transform.rotation;
        CurrentColor = CubeColor.gray;
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
            if (value == CubeColor.gray)
            {
                CubeMaterial.SetColor("_Color", Color.gray);
                ArrowMaterial.SetColor("_Color", Color.gray);
                ArrowMaterial.SetColor("_EmissionColor", Color.gray);
            }
        }
    }
    public CubeOrientation Orientation { get; set; }
    public void Destroyed()
    {
        Color currentCubeColor = CubeMaterial.GetColor("_Color");
        Color currentArrowColor = ArrowMaterial.GetColor("_Color");
        Color currentArrowEmission = ArrowMaterial.GetColor("_EmissionColor");
        GameObject RHalf = Instantiate(RHalfCube, gameObject.transform.position, gameObject.transform.rotation);
        GameObject LHalf = Instantiate(LHalfCube, gameObject.transform.position, gameObject.transform.rotation);
        Material RArrowMat = RHalf.GetComponentsInChildren<Renderer>()[0].material;
        RArrowMat.SetColor("_Color", currentArrowColor);
        RArrowMat.SetColor("_EmissionColor", currentArrowEmission);
        Material RCubeMat = RHalf.GetComponentsInChildren<Renderer>()[1].material;
        RCubeMat.SetColor("_Color", currentCubeColor);
        Material LArrowMat = LHalf.GetComponentsInChildren<Renderer>()[0].material;
        LArrowMat.SetColor("_Color", currentArrowColor);
        LArrowMat.SetColor("_EmissionColor", currentArrowEmission);
        Material LCubeMat = LHalf.GetComponentsInChildren<Renderer>()[1].material;
        LCubeMat.SetColor("_Color", currentCubeColor);
        Rigidbody rigRHalf = RHalf.GetComponent<Rigidbody>();
        Rigidbody rigLHalf = LHalf.GetComponent<Rigidbody>();
        rigRHalf.AddForce(new Vector3(2f, -5f, -1f), ForceMode.Impulse);
        rigLHalf.AddForce(new Vector3(-2f, -5f, 1f), ForceMode.Impulse);
    }
}
