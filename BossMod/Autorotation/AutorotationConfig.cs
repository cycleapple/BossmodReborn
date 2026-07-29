namespace BossMod.Autorotation;

[ConfigDisplay(Name = "自動循環", Order = 5)]
public sealed class AutorotationConfig : ConfigNode
{
    [PropertyDisplay("顯示遊戲內介面")]
    public bool ShowUI = false;

    public enum DtrStatus
    {
        [PropertyDisplay("停用")]
        None,
        [PropertyDisplay("僅顯示文字")]
        TextOnly,
        [PropertyDisplay("顯示圖示與文字")]
        Icon
    }

    [PropertyDisplay("在伺服器資訊列顯示自動循環預設")]
    public DtrStatus ShowDTR = DtrStatus.None;

    [PropertyDisplay("隱藏 VBM 預設配置", tooltip: "若你已建立自己的預設且不再需要內建預設，啟用此選項可讓它不再顯示於「自動循環」與「預設編輯器」視窗。")]
    public bool HideDefaultPreset = false;

    public bool SuggestHealerAI = true;

    [PropertyDisplay("在場景中顯示身位提示", tooltip: "顯示身位技能提示，指出應移動至目標的側面或背面。")]
    public bool ShowPositionals = false;

    [PropertyDisplay("角色死亡時自動停用自動循環")]
    public bool ClearPresetOnDeath = true;

    [PropertyDisplay("脫離戰鬥時自動停用自動循環")]
    public bool ClearPresetOnCombatEnd = false;

    [PropertyDisplay("觸發誘餌陷阱時自動停用自動循環", tooltip: "僅適用於深層迷宮。")]
    public bool ClearPresetOnLuring = false;

    [PropertyDisplay("脫離戰鬥時重新啟用遭強制停用的自動循環")]
    public bool ClearForceDisableOnCombatEnd = true;

    [PropertyDisplay("提前開怪判定秒數", tooltip: "若有人在倒數剩餘時間大於此數值時進入首領戰鬥，將視為提前開怪並強制停用自動循環。")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;
}
