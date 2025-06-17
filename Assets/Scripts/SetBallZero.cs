using UnityEngine;
using Mirror;

public class SetBallZero : NetworkBehaviour
{
    [SerializeField] private Vector3 _ballPosition = new Vector3(0,0,0);

    private GameObject _ball;
    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _ball = GameObject.FindGameObjectWithTag("Ball");
                if (_ball != null )
                {
                    _ball.transform.position = _ballPosition;
                    _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                }
            }
        }
    }
}
