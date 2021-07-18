using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    public enum GameMode { main, edit }
    [SerializeField] private int FOVX;
    [Tooltip("Default camera rotation.")]
    [SerializeField] private float FOVXFix;
    [SerializeField] private float FOVY;
    [Tooltip("Smooth the acceleration inputs.")]
    [SerializeField] private float smooth;
    [SerializeField] private float accelerationSens;
    [Tooltip("How far can player slice cubes.")]
    [SerializeField] private float swordLenght;
    [SerializeField] private int maxFingers;
    [Tooltip("The number of raycasts between current and previous finger position. Used to fix input misreading.")]
    [SerializeField] private int fixRayCasts;
    [SerializeField] private GameMode gameMode;
    private Camera _camera;
    private Editor _editor;
    private Quaternion cameraOriginalRotation;
    private Vector3 cameraOriginalPostion;
    private List<Cube> _cubes;
    private Touch[] _touches;
    //private List<TouchTrail> _trails;
    private float angleHorizontal;
    private float angleVertical;
    private float xVelocity = 0f;
    private float yVelocity = 0f;
    private void Start()
    {
        //_trails = FindObjectsOfType<TouchTrail>().ToList();
        _cubes = FindObjectsOfType<Cube>().ToList();
        _camera = Camera.main;
        cameraOriginalRotation = _camera.transform.rotation;
        cameraOriginalPostion = _camera.transform.position;
        if (gameMode == GameMode.edit)
        {
            _editor = FindObjectOfType<Editor>();
            _cubes = _editor.GetCubes();
        }
    }
    private void Update()
    {
        // Math.Round is used to cut off unnesessary and random changes in device position
        angleVertical = Mathf.SmoothDampAngle(_camera.transform.position.x, Mathf.Clamp((float)Math.Round(Input.acceleration.x, 2), -accelerationSens, accelerationSens) * FOVY, ref yVelocity, smooth);
        angleHorizontal = Mathf.SmoothDampAngle(_camera.transform.eulerAngles.x, -(float)Math.Round(Input.acceleration.z, 2) * FOVX - FOVXFix, ref xVelocity, smooth);
        _camera.transform.SetPositionAndRotation(cameraOriginalPostion + new Vector3(angleVertical, 0f, 0f), cameraOriginalRotation * Quaternion.AngleAxis(angleHorizontal, Vector3.right));

        if (Input.touchCount > 0)
        {
            _touches = Input.touches;
            for (int i = 0; i < _touches.Length && i < maxFingers; i++)
            {
                Vector2 pos = _touches[i].position;
                Vector2 deltaPos = _touches[i].deltaPosition;
                //TouchPhase touchPhase = _touches[i].phase;
                //doesn't work as I expected
                //if (touchPhase == TouchPhase.Began)
                //    _trails[_touches[i].fingerId].SetPosition(pos);
                //else
                //    _trails[_touches[i].fingerId].Trail(pos);

                for (int j = 0; j < fixRayCasts; j++)
                {
                    GameObject target = TryRayCast(pos - deltaPos * j / fixRayCasts);
                    if (target != null)
                    {
                        Cube sliced = _cubes.First(cube => cube.gameObject == target);
                        switch (gameMode)
                        {
                            case GameMode.main:
                                // calculating direction
                                if (deltaPos.x > 40 && Math.Abs(deltaPos.x) > Math.Abs(deltaPos.y * 2))
                                    sliced.CurrentColor = Cube.CubeColor.blue;
                                if (deltaPos.x < -40 && Math.Abs(deltaPos.x) > Math.Abs(deltaPos.y * 2))
                                    sliced.CurrentColor = Cube.CubeColor.red;
                                if (deltaPos.y > 40 && Math.Abs(deltaPos.y) > Math.Abs(deltaPos.x * 2))
                                    sliced.CurrentColor = Cube.CubeColor.blue;
                                if (deltaPos.y < -40 && Math.Abs(deltaPos.y) > Math.Abs(deltaPos.x * 2))
                                    sliced.CurrentColor = Cube.CubeColor.red;
                                break;
                            case GameMode.edit:
                                _editor.ClickedCube(sliced);
                                break;
                        } 
                    }
                }
            }
        }
    }

    private GameObject TryRayCast(Vector2 pos)
    {
        Ray ray = _camera.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit target, swordLenght))
        {
            return target.collider.gameObject;
        }
        return null;
    }
}
