﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BasePP : MonoBehaviour
{
    public ComputeShader shader = null;

    protected string kernelName = "CSMain";

    protected Vector2Int texSize = new Vector2Int(0,0);
    protected Vector2Int groupSize = new Vector2Int();
    protected Camera thisCamera;

    protected RenderTexture output = null;
    protected RenderTexture renderedSource = null;

    protected int kernelHandle = -1;
    protected bool init = false;

    protected virtual void Init()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("It seems your target Hardware does not support Compute Shaders.");
            return;
        }

        if (!shader)
        {
            Debug.LogError("No shader");
            return;
        }

        kernelHandle = shader.FindKernel(kernelName);

        thisCamera = GetComponent<Camera>();

        if (!thisCamera)
        {
            Debug.LogError("Object has no Camera");
            return;
        }

        CreateTextures();

        init = true;
    }

    void ClearTexture(ref RenderTexture textureToClear)
    {
        if (null != textureToClear)
        {
            textureToClear.Release();
            textureToClear = null;
        }
    }

    protected virtual void ClearTextures()
    {
        ClearTexture(ref output);
        ClearTexture(ref renderedSource);
    }

    protected void CreateTexture(ref RenderTexture textureToMake)
    {
        textureToMake = new RenderTexture(texSize.x, texSize.y, 0);
        textureToMake.enableRandomWrite = true;
        textureToMake.Create();
    }


    protected virtual void CreateTextures()
    {
        texSize.x = Mathf.RoundToInt(thisCamera.pixelWidth);
        texSize.y = Mathf.RoundToInt(thisCamera.pixelHeight);

        if (shader)
        {
            uint x, y;
            shader.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out _);
            groupSize.x = Mathf.CeilToInt((float)texSize.x / (float)x);
            groupSize.y = Mathf.CeilToInt((float)texSize.y / (float)y);
        }

        CreateTexture(ref output);
        CreateTexture(ref renderedSource);

        shader.SetTexture(kernelHandle, "source", renderedSource);
        shader.SetTexture(kernelHandle, "output", output);
    }

    void OnEnable()
    {
        Init();
        CreateTextures();
    }

    void OnDisable()
    {
        ClearTextures();
        init = false;
    }

    void OnDestroy()
    {
        ClearTextures();
        init = false;
    }

    protected virtual void DispatchWithSource(ref RenderTexture source, ref RenderTexture destination)
    {
        if (!init) return;

        Graphics.Blit(source, renderedSource);

        shader.Dispatch(kernelHandle, groupSize.x, groupSize.y, 1);
        
        Graphics.Blit(output, destination);
    }

    protected void CheckKernelAndResolution(out bool resChange )
    {
        resChange = false;

        if (!init)
        {
            Init();
        }

        if (!shader)
        {
            init = false;
            ClearTextures();
            return;
        }

        Vector2Int resolution = new Vector2Int(Mathf.RoundToInt(thisCamera.pixelWidth), Mathf.RoundToInt(thisCamera.pixelWidth));

        if (kernelHandle<0) kernelHandle = shader.FindKernel("CSMain");

        if (texSize.x != resolution.x || texSize.y != resolution.y)
        {
            resChange = true;

            CreateTextures();

            uint x, y;
            shader.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out _);

        }
    }

}