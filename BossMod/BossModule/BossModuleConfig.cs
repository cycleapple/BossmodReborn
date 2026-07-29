namespace BossMod;

[ConfigDisplay(Name = "首領模組與雷達", Order = 1)]
public sealed class BossModuleConfig : ConfigNode
{
    // boss module settings
    [PropertyDisplay("載入模組所需的最低成熟度", tooltip: "部分模組標記為「開發中」，除非調整此設定，否則不會自動載入。")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("允許模組自動使用技能", tooltip: "例如：模組可在擊退發生前自動使用防擊退技能。")]
    public bool AllowAutomaticActions = true;

    [PropertyDisplay("顯示測試用雷達與提示視窗", tooltip: "不需進入首領戰，也能用來調整雷達與提示視窗。", separator: true)]
    public bool ShowDemo = false;

    // radar window settings
    [PropertyDisplay("啟用雷達")]
    public bool Enable = true;

    [PropertyDisplay("鎖定雷達與提示視窗的位置及滑鼠互動")]
    public bool Lock = false;

    [PropertyDisplay("雷達視窗使用透明背景", tooltip: "移除雷達周圍的黑色視窗背景；若將雷達移至其他螢幕，此功能可能無法運作。")]
    public bool TrishaMode = true;

    [PropertyDisplay("為雷達中的場地加入不透明背景")]
    public bool OpaqueArenaBackground = true;

    [PropertyDisplay("顯示雷達標記的輪廓與陰影")]
    public bool ShowOutlinesAndShadows = true;

    [PropertyDisplay("雷達場地縮放比例", tooltip: "雷達視窗內場地的縮放比例。")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ArenaScale = 1;

    [PropertyDisplay("雷達元素粗細比例", tooltip: "統一調整雷達元素輪廓的粗細。")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ThicknessScale = 1;

    [PropertyDisplay("旋轉雷達以配合鏡頭方向")]
    public bool RotateArena = true;

    [PropertyDisplay("關閉地圖旋轉時將地圖旋轉 180°")]
    public bool FlipArena = false;

    [PropertyDisplay("為雷達旋轉保留額外空間", tooltip: "使用上方設定時，可在雷達邊緣裁切前預留額外空間，以容納戰鬥中鏡頭旋轉或方位文字。")]
    [PropertySlider(1, 2, Speed = 0.1f, Logarithmic = true)]
    public float SlackForRotations = 1.5f;

    [PropertyDisplay("在雷達中顯示場地邊界")]
    public bool ShowBorder = true;

    [PropertyDisplay("玩家有危險時改變場地邊界顏色", tooltip: "當站位可能遭機制命中時，將白色邊界改為紅色。")]
    public bool ShowBorderRisk = true;

    [PropertyDisplay("在雷達上顯示方位名稱")]
    public bool ShowCardinals = false;

    [PropertyDisplay("方位文字大小")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float CardinalsFontSize = 17f;

    [PropertyDisplay("場地標記文字大小")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float WaymarkFontSize = 22f;

    [PropertyDisplay("角色三角形縮放比例")]
    [PropertySlider(0.1f, 10, Speed = 0.1f)]
    public float ActorScale = 1f;

    [PropertyDisplay("在雷達上顯示場地標記")]
    public bool ShowWaymarks = false;

    [PropertyDisplay("永遠顯示所有存活的小隊成員")]
    public bool ShowIrrelevantPlayers = false;

    [PropertyDisplay("雷達中未指定顏色的玩家依職責著色")]
    public bool ColorPlayersBasedOnRole = false;

    [PropertyDisplay("永遠顯示設為焦點目標的小隊成員", separator: true)]
    public bool ShowFocusTargetPlayer = false;

    // hint window settings
    [PropertyDisplay("在獨立視窗顯示文字提示", tooltip: "將提示視窗與雷達視窗分開，方便單獨調整提示視窗位置。")]
    public bool HintsInSeparateWindow = false;

    [PropertyDisplay("獨立提示視窗使用透明背景")]
    public bool HintsInSeparateWindowTransparent = false;

    [PropertyDisplay("顯示機制順序與計時提示")]
    public bool ShowMechanicTimers = true;

    [PropertyDisplay("顯示全隊機制提示")]
    public bool ShowGlobalHints = true;

    [PropertyDisplay("顯示玩家提示與警告", separator: true)]
    public bool ShowPlayerHints = true;

    // misc. settings
    [PropertyDisplay("在場景中顯示移動提示", tooltip: "此功能較少使用，但可在遊戲場景中顯示箭頭，指出部分機制的移動方向。")]
    public bool ShowWorldArrows = false;

    [PropertyDisplay("顯示近戰距離指示")]
    public bool ShowMeleeRangeIndicator = false;

    [PropertyDisplay("最大載入距離", tooltip: "最大載入距離，單位為亞姆。")]
    [PropertySlider(0.1f, 500f, Speed = 0.1f, Logarithmic = true)]
    public float MaxLoadDistance = 500f;
}
