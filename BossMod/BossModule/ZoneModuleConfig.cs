namespace BossMod;

[ConfigDisplay(Name = "完整任務自動化", Order = 6)]
public sealed class ZoneModuleConfig : ConfigNode
{
    [PropertyDisplay("載入區域模組所需的最低成熟度")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("啟用任務戰鬥／單人任務自動執行")]
    public bool EnableQuestBattles = false;

    [PropertyDisplay("在遊戲場景中繪製路徑點")]
    public bool ShowWaypoints = false;

    [PropertyDisplay("尋路時使用位移技能（疾塗、後躍等）")]
    public bool UseDash = true;

    [PropertyDisplay("鎖定區域模組視窗的位置與滑鼠互動")]
    public bool Lock = false;

    [PropertyDisplay("區域模組視窗使用透明背景", tooltip: "移除區域模組視窗周圍的黑色背景；若將雷達移至其他螢幕，此功能可能無法運作。")]
    public bool TransparentMode = false;
}
