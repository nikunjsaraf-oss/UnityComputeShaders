using UnityEngine;

public class AssignTexture : MonoBehaviour
{
    public ComputeShader shader;
    public int textResolution = 256;

    private Renderer _renderer;
    RenderTexture _outputTexture;
    private int _kernelHandle;
    

    private void Start()
    {
        _outputTexture = new RenderTexture(textResolution, textResolution, 0)
        {
            enableRandomWrite = true
        };
        _outputTexture.Create();

        _renderer = GetComponent<Renderer>();
        _renderer.enabled = true;
        
        InitShader();
    }

    private void InitShader()
    {
        _kernelHandle = shader.FindKernel("CSMain");
        shader.SetTexture(_kernelHandle, "Result", _outputTexture);
        _renderer.material.SetTexture("_MainTex", _outputTexture);

        DispatchShader(textResolution / 16, textResolution / 16);
    }

    private void DispatchShader(int x, int y)
    {
        shader.Dispatch(_kernelHandle, x, y, 1);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.U))
        {
            DispatchShader(textResolution / 8, textResolution / 8);
        }
    }
}