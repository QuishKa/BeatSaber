using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    [SerializeField] private int FOVX;
    [SerializeField] private float FOVY;
    public float sens;
    [SerializeField] private float smooth;
    private Camera _camera;
    private Quaternion cameraOriginalRotation;
    private Vector3 cameraOriginalPostion;
    private IEnumerable<Cube> _cubes;
    private Touch[] _touches;
    private List<TouchTrail> _trails;
    private float angleHorizontal;
    private float angleVertical;
    private float xVelocity = 0f;
    private float yVelocity = 0f;
    void Start()
    {
        _trails = FindObjectsOfType<TouchTrail>().ToList();
        _cubes = FindObjectsOfType<Cube>().ToList();
        _camera = Camera.main;
        cameraOriginalRotation = _camera.transform.rotation;
        cameraOriginalPostion = _camera.transform.position;
    }

    void Update()
    {
        angleVertical = Mathf.SmoothDampAngle(_camera.transform.position.x, Mathf.Clamp((float)Math.Round(Input.acceleration.x, 2), -sens, sens) * FOVY, ref yVelocity, smooth);
        angleHorizontal = Mathf.SmoothDampAngle(_camera.transform.eulerAngles.x, -(float)Math.Round(Input.acceleration.z, 2) * FOVX - 25, ref xVelocity, smooth);
        _camera.transform.SetPositionAndRotation(cameraOriginalPostion + new Vector3(angleVertical, 0f, 0f), cameraOriginalRotation * Quaternion.AngleAxis(angleHorizontal, Vector3.right));

        if (Input.touchCount > 0)
        {
            _touches = Input.touches;
            for (int i = 0; i < _touches.Length && i < 5; i++)
            {
                Vector2 pos = _touches[i].position;
                Vector2 deltaPos = _touches[i].deltaPosition;
                TouchPhase touchPhase = _touches[i].phase;
                if (touchPhase == TouchPhase.Began)
                    _trails[_touches[i].fingerId].SetPosition(pos);
                else
                    _trails[_touches[i].fingerId].Trail(pos);

                for (int j = 0; j < 7; j++)
                {
                    GameObject target = TryRayCast(pos - deltaPos * j / 7);
                    if (target != null)
                    {
                        if (target.layer == 7) // Cube layer
                        {
                            Cube sliced = _cubes.First(cube => cube.gameObject == target);
                            if (sliced != null)
                            {
                                if (deltaPos.x > 40 && Math.Abs(deltaPos.x) > Math.Abs(deltaPos.y * 2))
                                    sliced.color = Cube.CubeColor.blue;
                                if (deltaPos.x < -40 && Math.Abs(deltaPos.x) > Math.Abs(deltaPos.y * 2))
                                    sliced.color = Cube.CubeColor.red;
                                if (deltaPos.y > 40 && Math.Abs(deltaPos.y) > Math.Abs(deltaPos.x * 2))
                                    sliced.color = Cube.CubeColor.blue;
                                if (deltaPos.y < -40 && Math.Abs(deltaPos.y) > Math.Abs(deltaPos.x * 2))
                                    sliced.color = Cube.CubeColor.red;
                            }
                        }
                        if (target.layer == 5 && touchPhase == TouchPhase.Began) // UI layer
                        {

                        }
                    }
                }
            }
        }
    }

    private GameObject TryRayCast(Vector2 pos)
    {
        Ray ray = _camera.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit target))
        {
            if (target.distance < 8)
            {
                return target.collider.gameObject;
            }
        }
        return null;
    }
}
