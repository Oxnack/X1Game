using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class input_conf : MonoBehaviour
{

    void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        // disable WebGLInput.captureAllKeyboardInput so elements in web page can handle keyboard inputs
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }

}
