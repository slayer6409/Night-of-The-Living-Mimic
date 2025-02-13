using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SlayerDeadBodiesBecomeZombiesRandomly.Patches
{
    public class ManeaterPatchThing : MonoBehaviour
    {
        CaveDwellerAI _instance;
        public void Update() 
        {
            if (_instance == null)
            {
                gameObject.TryGetComponent<CaveDwellerAI>(out _instance);
            }
            else
            {
                _instance.growthMeter = 0f;
            }
        }
    }
}
