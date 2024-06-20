using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;

public class UI_GetName : MonoBehaviour     //для получения имени с канваса вешается на инпут
{
#if UNITY_WEBGL

    [DllImport("__Internal")]
    private static extern void Hello();

    [DllImport("__Internal")]
    private static extern string getUserNickname();

#else

#endif

    [SerializeField] private TMP_InputField _inputField;

    private string _name;
    private void Start()
    {
        if (PlayerPrefs.GetString("name") != null)
        {
            _inputField.text = PlayerPrefs.GetString("name");  
            _name = PlayerPrefs.GetString("name");
        }

#if UNITY_WEBGL 
        Debug.Log("This is a WebJL build!");
        Hello();
        Debug.Log("Unity Get Nickname: " + getUserNickname());
        if (getUserNickname() != null && getUserNickname() != "")
        {
            _inputField.text = getUserNickname();
            _name = getUserNickname();
        }
        Debug.Log(_name);
#else
            Debug.Log("This is not a WebJL build.");
#endif
        SaveName();
    }
    public void SaveName()
    {
        _name = _inputField.text;
        PlayerPrefs.SetString("name", _name);
    }
}
