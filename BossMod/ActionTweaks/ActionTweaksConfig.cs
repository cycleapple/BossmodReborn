namespace BossMod;

[ConfigDisplay(Name = "技能調整", Order = 4)]
public sealed class ActionTweaksConfig : ConfigNode
{
    // TODO: consider exposing max-delay to config; 0 would mean 'remove all delay', max-value would mean 'disable'
    [PropertyDisplay("移除網路延遲造成的額外能力技僵直（請閱讀提示）", tooltip: "請勿與 XivAlexander 或 NoClippy 同時使用。偵測到它們時本功能應會自動停用，但仍請再次確認。")]
    public bool RemoveAnimationLockDelay = false;

    [PropertyDisplay("能力技僵直的最大模擬延遲（請閱讀提示）", tooltip: "設定移除能力技僵直時的最大模擬延遲（毫秒）。此值不可設為零。設為 20 毫秒時，自動循環可能在一個公共冷卻內插入三個能力技；至少設為 26 毫秒可避免此情況。FFLogs 已接受最低 20 毫秒的設定，理論上不會影響戰鬥紀錄。")]
    [PropertySlider(20, 50, Speed = 0.1f)]
    public int AnimationLockDelayMax = 20;

    [PropertyDisplay("移除幀率造成的額外冷卻延遲", tooltip: "動態調整冷卻與技能僵直，使佇列中的技能不受幀率限制並立即執行。")]
    public bool RemoveCooldownDelay = false;

    [PropertyDisplay("詠唱期間阻止移動", tags: ["slidecast"])]
    public bool PreventMovingWhileCasting = false;

    public enum ModifierKey
    {
        [PropertyDisplay("無")]
        None,
        [PropertyDisplay("Ctrl")]
        Ctrl,
        [PropertyDisplay("Alt")]
        Alt,
        [PropertyDisplay("Shift")]
        Shift,
        [PropertyDisplay("滑鼠左鍵＋右鍵")]
        M12
    }

    [PropertyDisplay("詠唱期間允許移動的按住鍵", tooltip: "必須同時啟用上方設定。", tags: ["slidecast"])]
    public ModifierKey MoveEscapeHatch = ModifierKey.None;

    [PropertyDisplay("目標死亡時自動取消詠唱")]
    public bool CancelCastOnDeadTarget = false;

    [PropertyDisplay("即將觸發類似「動則火起」的機制時，阻止移動與執行技能（設為 0 可停用；其他情況請依延遲提高門檻）。")]
    [PropertySlider(0, 10, Speed = 0.01f)]
    public float PyreticThreshold = 1.0f;

    [PropertyDisplay("自動處理方向錯亂：正常移動與錯亂後方向的夾角大於此門檻時阻止移動（設為 180 可停用）。")]
    [PropertySlider(0, 180)]
    public float MisdirectionThreshold = 180f;

    [PropertyDisplay("使用技能後恢復角色面向")]
    public bool RestoreRotation = false;

    [PropertyDisplay("對滑鼠指向的目標使用技能")]
    public bool PreferMouseover = false;

    [PropertyDisplay("智慧技能選擇目標", tooltip: "若一般目標（滑鼠指向／目前目標）不適用於該技能，則自動選擇次佳目標（例如退避選擇另一名防護職業）。")]
    public bool SmartTargets = true;

    [PropertyDisplay("手動按下的技能使用自訂佇列", tooltip: "使手動操作與自動循環更妥善整合，避免在自動循環運作時按下治療能力技而發生三插或公共冷卻偏移。")]
    public bool UseManualQueue = false;

    [PropertyDisplay("嘗試避免突進至範圍攻擊內", tooltip: "若指定目標的突進技能（例如戰士的猛攻）會令你進入危險區域，便阻止自動使用。未提供模組的副本可能無法正常判斷。\n\n若啟用「手動按下的技能使用自訂佇列」，此選項也會套用至手動使用的突進技能。")]
    public bool DashSafety = true;

    [PropertyDisplay("將上一個選項套用至所有位移技能，而不僅是接近技", tooltip: "包含後退位移（例如武士的夜天）、傳送（例如忍者的縮地），以及固定距離位移（例如龍騎士的後躍）。")]
    public bool DashSafetyExtra = true;

    [PropertyDisplay("自動管理自動攻擊", tooltip: "防止倒數期間提早開始自動攻擊，並在開怪、切換目標或使用不會明確取消自動攻擊的技能時自動開始攻擊。")]
    public bool AutoAutos = false;

    [PropertyDisplay("執行技能時自動解除坐騎")]
    public bool AutoDismount = true;

    public enum GroundTargetingMode
    {
        [PropertyDisplay("額外點擊以手動選擇位置（遊戲預設行為）")]
        Manual,

        [PropertyDisplay("在目前滑鼠位置施放")]
        AtCursor,

        [PropertyDisplay("在所選目標的位置施放")]
        AtTarget
    }
    [PropertyDisplay("地面指定技能的自動目標選擇")]
    public GroundTargetingMode GTMode = GroundTargetingMode.Manual;

    public bool ActivateAnticheat = true;
}
