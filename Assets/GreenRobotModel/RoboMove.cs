using System.Collections;
using UnityEngine;

public class RoboMove : MonoBehaviour
{
    [SerializeField] private GameObject _robot;
    [SerializeField] private float _speed;
    [SerializeField] private float jumpForce = 5f;
    Rigidbody rb;
    public void Move(float time, int z, int x, int jump)
    {
        Debug.Log("Move in Robo move has started");
        StartCoroutine(MoveHook(time, z, x, jump));
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    //private IEnumerator MoveHook(float time, int z, int x, int jump)
    //{
    //    Rigidbody RoboRb = _robot.GetComponent<Rigidbody>();
    //    //Assets\GreenRobotModel\RoboMove.cs
    //    float moveHorizontal = 0;
    //    float moveVertical = 0;

    //    if (x == -1 || x == 1)
    //    {
    //        moveHorizontal = x;
    //    }

    //    if (z == -1 || z == 1)
    //    {
    //        moveVertical = z;
    //    }

    //    if (jump == 1) {
    //        // Jump();
    //        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    //    }
        

    //    //#####

    //    Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

    //    RoboRb.MovePosition(RoboRb.position + movement * _speed * Time.fixedDeltaTime);

    //    yield return new WaitForSeconds(time);
    //}

    private IEnumerator MoveHook(float time, int z, int x, int jump)
    {
        Rigidbody RoboRb = _robot.GetComponent<Rigidbody>();

        Vector3 movement = Vector3.zero;

        if (x == -1 || x == 1)
        {
            movement += new Vector3(x, 0, 0); 
        }

        if (z == -1 || z == 1)
        {
            movement += new Vector3(0, 0, z); 
        }

        if (movement != Vector3.zero)
        {
            movement.Normalize();
        }

        if (jump == 1)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            RoboRb.MovePosition(RoboRb.position + movement * _speed * Time.fixedDeltaTime);
            elapsedTime += Time.fixedDeltaTime; 
            yield return new WaitForFixedUpdate(); 
        }
    }
}
