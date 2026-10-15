using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float _smoothTime = 0.15f;

    private Vector3 _velocity;

    public void SetTarget(Transform target) => _target = target;

    private void LateUpdate()
    {
        if (_target == null) return;
        Vector3 goal = _target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, _smoothTime);
    }
}
