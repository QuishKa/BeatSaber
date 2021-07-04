using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    private Camera _camera;
    private IEnumerable<Cube> _cubes;
    private RaycastHit _target;
    private Touch _touch;
    private Cube _sliced = null;
    private Ray _ray;

    private List<TouchTrail> _trails;
    void Start()
    {
        _trails = FindObjectsOfType<TouchTrail>().ToList();
        _cubes = FindObjectsOfType<Cube>().ToList();
        _camera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                _touch = Input.GetTouch(i);
                if (i < 5) // fix later
                {
                    if (_touch.phase == TouchPhase.Began)
                        _trails[i].Move(_touch.position);
                    _trails[i].Trail(_touch.position);
                }
                for (int j = 0; j < 7; j++)
                {
                    _ray = _camera.ScreenPointToRay(_touch.position - _touch.deltaPosition * j / 7);
                    if (Physics.Raycast(_ray, out _target))
                    {
                        if (_target.distance < 8)
                        {
                            Debug.Log("hit");
                            _sliced = _cubes.First(cube => cube.gameObject == _target.collider.gameObject);
                            // direction
                            if (_touch.deltaPosition.x > 20 && Math.Abs(_touch.deltaPosition.x) > Math.Abs(_touch.deltaPosition.y * 2))
                                _sliced.CubeColor = Color.gray;
                            if (_touch.deltaPosition.x < -20 && Math.Abs(_touch.deltaPosition.x) > Math.Abs(_touch.deltaPosition.y * 2))
                                _sliced.CubeColor = Color.red;
                            if (_touch.deltaPosition.y > 20 && Math.Abs(_touch.deltaPosition.y) > Math.Abs(_touch.deltaPosition.x * 2))
                                _sliced.CubeColor = Color.blue;
                            if (_touch.deltaPosition.y < -20 && Math.Abs(_touch.deltaPosition.y) > Math.Abs(_touch.deltaPosition.x * 2))
                                _sliced.CubeColor = Color.white;
                        }
                    }
                }
            }
        }
    }
}
