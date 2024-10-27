using UnityEngine;
using Mirror;

public class PushBall : NetworkBehaviour
{
    public float pushForce = 10f; // —ила, с которой персонаж будет толкать м€ч

    private void OnCollisionEnter(Collision collision)
    {
        if (isServer)
        {
            if (collision.gameObject.tag == "Ball")
            {
                Rigidbody ballRigidbody = collision.gameObject.GetComponent<Rigidbody>();
                if (ballRigidbody != null)
                {
                    Vector3 pushDirection = collision.contacts[0].point - transform.position;
                    pushDirection = -pushDirection.normalized; // ќпредел€ем направление толчка
                    ballRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse); // “олкаем м€ч с определенной силой
                }
            }
        }
    }
}
