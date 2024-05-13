using UnityEngine;
using Mirror;

public class PushBall : NetworkBehaviour
{
    public float pushForce = 10f; // ����, � ������� �������� ����� ������� ���

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;
        if (collision.gameObject.tag == "Ball")
        {
            Rigidbody ballRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                Vector3 pushDirection = collision.contacts[0].point - transform.position;
                pushDirection = -pushDirection.normalized; // ���������� ����������� ������
                ballRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse); // ������� ��� � ������������ �����
            }
        }
    }
}
