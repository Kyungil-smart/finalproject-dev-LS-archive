
using Ingame.ExpSystem;
using UnityEngine;

class IngameUI : MonoBehaviour
{
    private void Start()
    {

    }

    private void OnEnable()
    {
        ExpManager.OnExpChanged += ControllExpGauge;
    }

    private void OnDestroy()
    {
        ExpManager.OnExpChanged -= ControllExpGauge;
    }

    private void ControllExpGauge(int amount)
    {

    }
}
