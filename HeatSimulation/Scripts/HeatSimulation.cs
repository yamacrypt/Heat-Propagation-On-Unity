using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class HeatSimulation:MonoBehaviour{
    private struct KernelDef
    {
        public int UpdateTemperatureID;
        public int UpdateTextureID;
        public int TestID;
    }

    private struct PropertyDef
    {
        public int DeltaTimeID;
        public int ScaleID;
        public int SourceTextureID;
        public int ResultTextureID;
        public int UpdateTemperatureID;
        public int ResultTemperatureID;
        public int WidthID;
        public int HeightID;
        public int StartColorID;
        public int EndColorID;
    }
    [SerializeField] private ComputeShader _shader = null;
    [SerializeField] private Texture2D _texture = null;
    [SerializeField] private RawImage _preview = null;
     private float _scale = 1.0f;
    [SerializeField] private float _numCalcTemperature = 20;

    private KernelDef _kernelDef = default;
    private PropertyDef _propertyDef = default;

    private SwapBuffer _previewBuffer = null;
    private SwapBuffer _tempBuffer = null;

    [SerializeField]Color startColor=Color.white;
    [SerializeField]Color endColor=Color.red;

    void Start()
    {
        Initialize();
        GetAllPropertyID();
        _shader.SetTexture(_kernelDef.TestID, _propertyDef.ResultTextureID, _previewBuffer.Current);
    }

    void Update()
    {
        BasicSetup();
        UpdateTemperature();
        UpdateTexture();
        UpdatePreview();
    }
     private void OnDestroy()
    {
        _previewBuffer.Release();
    }

    private void Initialize()
    {
        _scale = _texture.width / _preview.rectTransform.rect.width;

        CreateBuffers();

        InitializeKernel();
    }


    private void UpdatePreview()
    {
        _preview.texture = _previewBuffer.Current;
    }

    private void CreateBuffers()
    {
        _previewBuffer = new SwapBuffer(_texture.width, _texture.height);
        _tempBuffer = new SwapBuffer(_texture.width, _texture.height);

        //Graphics.Blit(_texture, _previewBuffer.Current);

    }

    private void BasicSetup()
    {
        _shader.SetFloat(_propertyDef.WidthID, _texture.width);
        _shader.SetFloat(_propertyDef.HeightID, _texture.height);
        _shader.SetFloat(_propertyDef.DeltaTimeID, Time.deltaTime);
        _shader.SetFloat(_propertyDef.ScaleID, _scale);
        _shader.SetFloats(_propertyDef.StartColorID, ColorToFloats(startColor));
        _shader.SetFloats(_propertyDef.EndColorID, ColorToFloats(endColor));
    }

    float[] ColorToFloats(Color from){
        return new float[4]{from.r,from.g,from.b,from.a};
    }

    private void InitializeKernel()
    {
        _kernelDef.UpdateTemperatureID = _shader.FindKernel("UpdateTemperature");
        _kernelDef.UpdateTextureID = _shader.FindKernel("UpdateTexture");
    }

    private void GetAllPropertyID()
    {
        _propertyDef.DeltaTimeID = Shader.PropertyToID("_DeltaTime");
        _propertyDef.ScaleID = Shader.PropertyToID("_Scale");
        _propertyDef.WidthID = Shader.PropertyToID("_Width");
        _propertyDef.HeightID = Shader.PropertyToID("_Height");
        _propertyDef.SourceTextureID = Shader.PropertyToID("_SourceTexture");
        _propertyDef.ResultTextureID = Shader.PropertyToID("_ResultTexture");
        _propertyDef.UpdateTemperatureID = Shader.PropertyToID("_UpdateTemperature");
        _propertyDef.ResultTemperatureID = Shader.PropertyToID("_ResultTemperature");
        _propertyDef.StartColorID = Shader.PropertyToID("_StartColor");
        _propertyDef.EndColorID = Shader.PropertyToID("_EndColor");
    }
    private void UpdateTemperature()
    {
        for (int i = 0; i < _numCalcTemperature; i++)
        {
            _shader.SetTexture(_kernelDef.UpdateTemperatureID, _propertyDef.UpdateTemperatureID, _tempBuffer.Current);
            _shader.SetTexture(_kernelDef.UpdateTemperatureID, _propertyDef.ResultTemperatureID, _tempBuffer.Other);

            _shader.Dispatch(_kernelDef.UpdateTemperatureID, _tempBuffer.Width / 8, _tempBuffer.Height / 8, 1);

            _tempBuffer.Swap();
        }
    }
    private void UpdateTexture()
    {
        _shader.SetTexture(_kernelDef.UpdateTextureID, _propertyDef.UpdateTemperatureID, _tempBuffer.Current);
        _shader.SetTexture(_kernelDef.UpdateTextureID, _propertyDef.SourceTextureID, _previewBuffer.Current);
        _shader.SetTexture(_kernelDef.UpdateTextureID, _propertyDef.ResultTextureID, _previewBuffer.Other);

        _shader.Dispatch(_kernelDef.UpdateTextureID, _previewBuffer.Width / 8, _previewBuffer.Height / 8, 1);

        _previewBuffer.Swap();
    }
}