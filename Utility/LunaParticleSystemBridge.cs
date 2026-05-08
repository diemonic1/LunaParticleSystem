using System;
using System.Globalization;
using PlayablesPlugins;
using UnityEngine;
using Object = System.Object;

public class LunaParticleSystemBridge : MonoBehaviour
{
    private const string Version = "1.0.2";
    
    // DO NOT MAKE IT A CONSTANT!!!!
    private string code_SubscribeOnUIUpdate = "window.addEventListener('message', (e) => " +
                                              "{ if(e.data.type != 'LPS_DATA_PS_SETTINGS') return;  window.windowLPSData = JSON.stringify(e.data.payload); });";
    private string code_SubscribeOnTimeScaleUpdate = "window.addEventListener('message', (e) => " +
                                                     "{ if(e.data.type != 'LPS_DATA_TimeScale') return;  window.windowLPSDataTimeScale = JSON.stringify(e.data.payload); });";
    private string code_SubscribeOnEnableLogsUpdate = "window.addEventListener('message', (e) => " +
                                                      "{ if(e.data.type != 'LPS_DATA_EnableLogs') return;  window.windowLPSDataEnableLogs = JSON.stringify(e.data.payload); });";
    private string code_RegistrateFunction = "window.windowLPSsendToPage = function(data) {" +
                                             "window.parent.postMessage({type:'LPS_DATA_StartSetup',payload:data}, '*');};";

    public bool EnableLos { get; private set; }
    
    public Action<string, bool> OnDataUpdated;

    private int _currentId = 0;

#if UNITY_LUNA
    public int RegisterParticleSystem(LunaParticleSystem lunaParticleSystem)
    {
        _currentId++;
        LPSJSWindow.windowLPSsendToPage($"{{\"name\": \"{lunaParticleSystem.gameObject.name}\", \"id\": {_currentId}, \"settings\": {lunaParticleSystem.GetData()}}}");
        return _currentId;
    }
    
    // necessarily Awake, because Bridge must have time to register the transmitter function in js
    private void Awake()
    {
        Debug.Log($"[LunaParticleSystemBridge] Start Working \n Version {Version}");

        Bridge.Script.Write( "let fn = Function(this.code_SubscribeOnUIUpdate);" );
        Bridge.Script.Write( "fn();" );
        Bridge.Script.Write( "let fn2 = Function(this.code_SubscribeOnTimeScaleUpdate);" );
        Bridge.Script.Write( "fn2();" );
        Bridge.Script.Write( "let fn3 = Function(this.code_SubscribeOnEnableLogsUpdate);" );
        Bridge.Script.Write( "fn3();" );
        Bridge.Script.Write( "let fn4 = Function(this.code_RegistrateFunction);" );
        Bridge.Script.Write( "fn4();" );
    }

    private void Update()
    {
        if (LPSJSWindow.windowLPSData != null)
        {
            OnDataUpdated?.Invoke(LPSJSWindow.windowLPSData, EnableLos);
            LPSJSWindow.windowLPSData = null;
        }

        if (LPSJSWindow.windowLPSDataTimeScale != null)
        {
            var str = LPSJSWindow.windowLPSDataTimeScale.Trim('"');
            Time.timeScale = float.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            LPSJSWindow.windowLPSDataTimeScale = null;
        }

        if (LPSJSWindow.windowLPSDataEnableLogs != null)
        {
            var str = LPSJSWindow.windowLPSDataEnableLogs.Trim('"');

            if (str == "true")
                EnableLos = true;
            else 
                EnableLos = false;

            LPSJSWindow.windowLPSDataEnableLogs = null;
        }
    }
#endif
}