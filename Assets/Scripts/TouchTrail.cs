using UnityEngine;

public class TouchTrail : MonoBehaviour
{
    private Camera _camera;
    private void Start()
    {
        _camera = Camera.main;
    }
    public void Trail(Vector3 pos)
    {
        pos.z = 1f;
        gameObject.transform.position = _camera.ScreenToWorldPoint(pos);
    }
    public void SetPosition(Vector3 startPos)
    {
        gameObject.SetActive(false);
        startPos.z = 1f;
        gameObject.transform.position = _camera.ScreenToWorldPoint(startPos);
        gameObject.SetActive(true);
    }
}
