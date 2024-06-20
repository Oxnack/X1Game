using UnityEngine;
using Mirror;

public class CommandColor : NetworkBehaviour   // Вешается на обьект визуального касания (работает только на сервере)
{
    [SerializeField] private Color _color; // цвет на который будет изменяться перс

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.gameObject.GetComponent<ColorChanger>().SetColor(_color);
            }
        }
    }
}

