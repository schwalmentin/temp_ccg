using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestPlayer : NetworkBehaviour
{
    [SerializeField] private float _acceleration = 80;
    [SerializeField] private float _deceleration = 80;

    [SerializeField] private float _maxVelocity = 10;
    private Vector3 _input;
    private Rigidbody _rb;

    private void Awake()
    {
        //if (!IsOwner)
        //{
        //    Destroy(this);
        //    return;
        //}

        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        _rb.velocity += _input.normalized * (_acceleration * Time.deltaTime);
        if (_input.magnitude < 0.1f)
        {
            _rb.velocity -= _rb.velocity * (_deceleration * Time.deltaTime);
        }
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, _maxVelocity);

    }
}
