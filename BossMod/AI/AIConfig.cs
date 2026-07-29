namespace BossMod.AI;

[ConfigDisplay(Name = "AI 設定（此功能仍處於高度實驗階段，請自行承擔風險！）", Order = 7)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("在伺服器資訊列顯示狀態")]
    public bool ShowDTR = false;

    [PropertyDisplay("顯示 AI 介面")]
    public bool DrawUI = false;

    [PropertyDisplay("將焦點目標設為主控角色")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("將按鍵輸入廣播至其他視窗", tooltip: "可能導致部分電腦卡頓，請只在確實需要時啟用。此功能僅供多開玩家使用。")]
    public bool BroadcastToSlaves = false;

    [PropertyDisplay("跟隨小隊欄位")]
    public int FollowSlot = 0;

    [PropertyDisplay("禁止執行技能")]
    public bool ForbidActions = false;

    [PropertyDisplay("手動選擇目標")]
    public bool ManualTarget = false;

    [PropertyDisplay("禁止移動")]
    public bool ForbidMovement = false;

    [PropertyDisplay("戰鬥中跟隨")]
    public bool FollowDuringCombat = true;

    [PropertyDisplay("首領模組啟用時跟隨")]
    public bool FollowDuringActiveBossModule = true;

    [PropertyDisplay("非戰鬥中跟隨")]
    public bool FollowOutOfCombat = false;

    [PropertyDisplay("跟隨目標")]
    public bool FollowTarget = true;

    [PropertyDisplay("跟隨目標時的偏好身位")]
    [PropertyCombo(["任意", "側面", "背面", "正面"])]
    public Positional DesiredPositional = Positional.Any;

    [PropertyDisplay("與跟隨欄位的最大距離")]
    public float MaxDistanceToSlot = 1f;

    [PropertyDisplay("與目標的最大距離")]
    public float MaxDistanceToTarget = 2.6f;

    [PropertyDisplay("與目標判定圈的最小距離")]
    public float MinDistance = default;

    [PropertyDisplay("與禁入區域的偏好距離")]
    public float PreferredDistance = default;

    [PropertyDisplay("啟用自動暫離", tooltip: "非戰鬥狀態經過指定時間後自動進入暫離。暫離期間 AI 不會使用自動循環或選擇任何目標。")]
    public bool AutoAFK = false;

    [PropertyDisplay("自動暫離計時", tooltip: "脫離戰鬥後經過多少秒進入暫離模式。任何移動都會重設計時；若已進入暫離則會解除。")]
    public float AFKModeTimer = 10f;

    [PropertyDisplay("停用障礙物地圖載入", tooltip: "部分內容（例如深層迷宮）可能需要啟用此選項。")]
    public bool DisableObstacleMaps = false;

    [PropertyDisplay("移動決策延遲", tooltip: "請自行承擔修改風險並維持較低數值。數值過高可能無法及時迴避機制；不同內容可能需要重新調整。")]
    public double MoveDelay = default;

    [PropertyDisplay("騎乘坐騎時保持待機")]
    public bool ForbidAIMovementMounted = false;

    [PropertyDisplay("將斜線指令回顯至聊天視窗")]
    public bool EchoToChat = true;

    public string? AIAutorotPresetName;
}
