namespace BossMod;

[ConfigDisplay(Name = "配色", Order = -1)]
public sealed class ColorConfig : ConfigNode
{
    [PropertyDisplay("場地：背景")]
    public Color ArenaBackground = new(0xc00f0f0f);

    [PropertyDisplay("場地：邊界")]
    public Color ArenaBorder = new(0xffffffff);

    [PropertyDisplay("場地：一般危險區域（範圍攻擊）")]
    public Color ArenaAOE = new(0x80008080);

    [PropertyDisplay("場地：一般安全區域")]
    public Color ArenaSafeFromAOE = new(0x80008000);

    // TODO: imminent aoes and dangerous players should use separate color
    [PropertyDisplay("場地：一般危險前景元素（連線等）")]
    public Color ArenaDanger = new(0xff00ffff);

    [PropertyDisplay("場地：一般安全前景元素（連線等）")]
    public Color ArenaSafe = new(0xff00ff00);

    [PropertyDisplay("場地：敵人")]
    public Color ArenaEnemy = new(0xff0000ff);

    [PropertyDisplay("場地：重要非敵對物件（無法選取的連線起點、可互動物件等）")]
    public Color ArenaObject = new(0xff0080ff);

    [PropertyDisplay("場地：玩家角色")]
    public Color ArenaPC = new(0xff00ff00);

    [PropertyDisplay("場地：陷阱")]
    public Color ArenaTrap = new(0x80000080);

    [PropertyDisplay("場地：光源")]
    public Color ArenaLight = new(0xffffffff);

    [PropertyDisplay("場地：易受傷害、需要特別注意")]
    public Color ArenaVulnerable = new(0xffff00ff);

    [PropertyDisplay("場地：即將易受傷害")]
    public Color ArenaFutureVulnerable = new(0x80ff00ff);

    [PropertyDisplay("場地：近戰距離指示")]
    public Color ArenaMeleeRangeIndicator = new(0xffff0000);

    [PropertyDisplay("場地：其他")]
    public Color[] ArenaOther = [new(0xffff0080), new(0xff8080ff), new(0xff80ff80), new(0xffff8040), new(0xff40c0c0), new(0x40008080), new(0xffffff00), new(0xffff8000), new(0xffffa080)];

    [PropertyDisplay("場地：機制相關的重要玩家")]
    public Color ArenaPlayerInteresting = new(0xffc0c0c0);

    [PropertyDisplay("場地：一般／非機制相關玩家（依設定可由職責專用顏色取代）")]
    public Color ArenaPlayerGeneric = new(0xff808080);

    [PropertyDisplay("場地：不在目前小隊／團隊中的玩家（通常不會顯示於地圖）")]
    public Color ArenaPlayerReallyGeneric = new(0x80808080);

    [PropertyDisplay("場地：一般／非機制相關防護職業")]
    public Color ArenaPlayerGenericTank = Color.FromComponents(30, 50, 110);

    [PropertyDisplay("場地：一般／非機制相關治療職業")]
    public Color ArenaPlayerGenericHealer = Color.FromComponents(30, 110, 50);

    [PropertyDisplay("場地：一般／非機制相關近戰職業")]
    public Color ArenaPlayerGenericMelee = Color.FromComponents(110, 30, 30);

    [PropertyDisplay("場地：一般／非機制相關魔法遠程職業")]
    public Color ArenaPlayerGenericCaster = Color.FromComponents(70, 30, 110);

    [PropertyDisplay("場地：一般／非機制相關物理遠程職業")]
    public Color ArenaPlayerGenericPhysRanged = Color.FromComponents(110, 90, 30);

    [PropertyDisplay("場地：一般／非機制相關焦點目標")]
    public Color ArenaPlayerGenericFocus = Color.FromComponents(0, 255, 255);

    [PropertyDisplay("輪廓與陰影")]
    public Color Shadows = new(0xFF000000);

    [PropertyDisplay("場地標記：A")]
    public Color WaymarkA = new(0xff964ee5);

    [PropertyDisplay("場地標記：B")]
    public Color WaymarkB = new(0xff11a2c6);

    [PropertyDisplay("場地標記：C")]
    public Color WaymarkC = new(0xffe29f30);

    [PropertyDisplay("場地標記：D")]
    public Color WaymarkD = new(0xffbc567a);

    [PropertyDisplay("場地標記：1")]
    public Color Waymark1 = new(0xff964ee5);

    [PropertyDisplay("場地標記：2")]
    public Color Waymark2 = new(0xff11a2c6);

    [PropertyDisplay("場地標記：3")]
    public Color Waymark3 = new(0xffe29f30);

    [PropertyDisplay("場地標記：4")]
    public Color Waymark4 = new(0xffbc567a);

    [PropertyDisplay("方位：北")]
    public Color CardinalN = new(0xff0000ff);

    [PropertyDisplay("方位：東")]
    public Color CardinalE = new(0xffffffff);

    [PropertyDisplay("方位：南")]
    public Color CardinalS = new(0xffffffff);

    [PropertyDisplay("方位：西")]
    public Color CardinalW = new(0xffffffff);

    [PropertyDisplay("身位顏色")]
    public Color[] PositionalColors = [new(0xff00ff00), new(0xff0000ff), new(0xffffffff), new(0xff00ffff)];

    [PropertyDisplay("規劃器：背景")]
    public Color PlannerBackground = new(0x80362b00);

    [PropertyDisplay("規劃器：背景醒目提示")]
    public Color PlannerBackgroundHighlight = new(0x80423607);

    [PropertyDisplay("規劃器：冷卻時間")]
    public Color PlannerCooldown = new(0x80756e58);

    [PropertyDisplay("規劃器：選項預備顏色")]
    public Color PlannerFallback = new(0x80969483);

    [PropertyDisplay("規劃器：效果")]
    public Color PlannerEffect = new(0x8000ff00);

    [PropertyDisplay("規劃器：視窗")]
    public Color[] PlannerWindow = [new(0x800089b5), new(0x80164bcb), new(0x802f32dc), new(0x808236d3), new(0x80c4716c), new(0x80d28b26), new(0x8098a12a), new(0x80009985)];

    [PropertyDisplay("按鈕按下時的顏色")]
    public Color[] ButtonPushColor = [new(0xff000080), new(0xff008080), new(0xff000050), new(0xff000060), new(0xff005050), new(0xff006060)];

    [PropertyDisplay("文字顏色")]
    public Color[] TextColors = [new(0xffffffff), new(0xff00ffff), new(0xff0000ff),
     new(0xff00ff00), new(0xff0080ff), new(0xffff00ff), new(0x80808080), new(0x80800080),
     new(0x80ffffff), new(0x8000ff00), new(0xffffff00), new(0x800000ff), new(0xff404040),
     new(0xffff0000), new(0xff000000), new(0x80008080), new(0x8080ff80), new(0xffc0c0c0)];

    [PropertyDisplay("碰撞偵錯顏色")]
    public Color[] CollisionColors = [new(0xff00ff00), new(0xff00ffff), new(0xff0000ff)];

    [PropertyDisplay("尋路偵錯顏色")]
    public Color[] PathfindingColors = [new(0xff007fff), new(0xff808080), new(0xff0000ff), new(0xffff0080)];

    [PropertyDisplay("尋路偵錯顏色 5")]
    public Color PathfindingColors5 = new(0xffff0000);

    [PropertyDisplay("尋路偵錯顏色 6")]
    public Color PathfindingColors6 = new(0xffffffff);

    [PropertyDisplay("尋路偵錯顏色 7")]
    public Color PathfindingColors7 = new(0x80164bcb);

    [PropertyDisplay("尋路偵錯顏色 8")]
    public Color PathfindingColors8 = new(0x808236d3);

    public static ColorConfig DefaultConfig => new();
}
