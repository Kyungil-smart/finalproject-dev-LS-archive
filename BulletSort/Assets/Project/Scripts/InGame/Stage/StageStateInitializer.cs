using Core;
using InGame.Slot;
using UnityEngine;

class StageStateInitializer : MonoBehaviour
{
    [SerializeField] SlotBoardManager _slotBoardManager;

    private void Awake()
    {
        StageManager.Instance.BindSlotBoardManager(_slotBoardManager);
    }

    private void Start()
    {
        StageManager.Instance.EnterStage();
    }
}
