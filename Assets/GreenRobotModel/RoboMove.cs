using System.Collections;
using UnityEngine;

public class RoboMove : MonoBehaviour
{
    [SerializeField] private GameObject _robot;
    [SerializeField] private float _speed;

    public void Move(float time, int z, int x)
    {
        StartCoroutine(MoveHook(time, z, x));
    }

    private IEnumerator MoveHook(float time, int z, int x)
    {
        Rigidbody RoboRb = _robot.GetComponent<Rigidbody>();
        //Assets\GreenRobotModel\RoboMove.cs
        float moveHorizontal = 0;
        float moveVertical = 0;

        if (x == -1 || x == 1)
        {
            moveHorizontal = x;
        }

        if (z == -1 || z == 1)
        {
            moveVertical = z;
        }

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        RoboRb.MovePosition(RoboRb.position + movement * _speed * Time.fixedDeltaTime);

        yield return new WaitForSeconds(time);
    }
}
