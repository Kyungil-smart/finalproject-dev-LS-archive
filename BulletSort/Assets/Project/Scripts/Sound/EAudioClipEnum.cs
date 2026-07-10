
namespace Audio
{
    public enum EAudioClipEnum
    {
        // BGM 
        Lobby, Stage1, Stage2, Stage3, Stage4, Stage5,

        // SFX

        // Boss
        BossSpawnWarning = 10, BossDeath,

        //Slot
        // ReapairSlot => finishRepair
        // FixSlot => notyet Repair
        BrokenSlot = 20, RepairSlot, FixSlot,

        // Wave
        WaveEncounter = 30,

        // Stage
        StageVictory = 40, StageDefeat,

        // Projectile   
        NormalProjectile = 50, ShotgunProjectile, SniperProjectile, 
        Grenade_Projectile, AntiTankProjectile, HandgunProjectile,

        // Deck
        BulletSelected = 60, BulletLocked,

        // Piece
        // PieceSort => BulletSort 
        // PieceSelect => BulletSelect
        PieceSort = 70, PieceSelect,

        // UpGrade(Ganghwa)
        GanghwaSuccess = 80,

        // Monster
        MonsterDeath = 90, Monster_Hit,

        // Perks
        PerksReroll = 100, PerksSelect, PerksUi,

        // Logo
        logo_sound = 110,
        
        // Button
        PositiveButton = 120, NegativeButton,
    }
}
