using UnityEngine;

namespace InGame.Tower.Data
{
    /// <summary>
    /// 타워 임시 SO
    /// 작성자 : 안정연
    /// </summary>
    
    [CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]

    public class TowerData : ScriptableObject
    {
        public int TowerID;
        
        public int TowerType;

        public int TowerAIType;
        
        public int TowerAtk;

        public float TowerAtkSpeed;

        public int TowerMaxAmmo;

        public int TowerMaxLange;
        
        public int ProjectileCount;
        
    }
}
