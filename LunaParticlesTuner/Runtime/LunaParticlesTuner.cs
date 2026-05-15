using System;
using UnityEngine;

namespace PlayablesPlugins
{
    [RequireComponent(typeof(ParticleSystem))]
    public class LunaParticlesTuner : MonoBehaviour
    {
        [Space(20)]
        [Header("Link on ParticleSystem")]
        [SerializeField] private ParticleSystem _particleSystem;
        
#if UNITY_EDITOR
        [Space(20)]
        [Header("Paste settings in json format here")]
        [TextArea(10, 20)]
        public string Json;
#endif

        private LunaParticlesTunerBridge _lunaParticlesTunerBridge;

        private LPSData _lpsData;

        private int _id;
        
        // Be sure to use Start, because Bridge must have time to register the transmitter function in js
        private void Start()
        {
            CheckParticleSystem();
            _lpsData = new LPSData(_particleSystem);

            _lunaParticlesTunerBridge = GameObject.FindObjectOfType<LunaParticlesTunerBridge>();
            
            if (_lunaParticlesTunerBridge == null)
                _lunaParticlesTunerBridge = new GameObject("LunaParticlesTunerBridge").AddComponent<LunaParticlesTunerBridge>();
            
#if UNITY_LUNA
            _id = _lunaParticlesTunerBridge.RegisterParticleSystem(this);
#endif
            _lunaParticlesTunerBridge.OnDataUpdated += SetData;
        }

        public string GetData()
        {
            return _lpsData.GetData();
        }
        
        private void SetData(string data, bool logsEnabled)
        {
            _lpsData.SetData(data, _id, logsEnabled);
        }

#if UNITY_EDITOR
        public void SetDataInEditor(string data)
        {
            // If you don't stop it, an error will appear
            _particleSystem.Stop();
            _particleSystem.Clear(true);
            _lpsData = new LPSData(_particleSystem);
            _lpsData.SetData(data, _id, false, true);
            _particleSystem.Play(true);
        }
#endif
        
        private void CheckParticleSystem()
        {
            if (_particleSystem == null)
                throw new Exception("[LunaParticlesTuner] ParticleSystem reference is missing on " + gameObject.name);
        }
        
#if UNITY_EDITOR
        private void Reset()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }
#endif
    }
}