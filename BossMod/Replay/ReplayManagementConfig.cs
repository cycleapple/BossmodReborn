namespace BossMod;

public record struct ReplayMemory(string Path, bool IsOpen, DateTime PlaybackPosition);

[ConfigDisplay(Name = "戰鬥重播", Order = 0)]
public sealed class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("顯示重播管理介面")]
    public bool ShowUI = false;

    [PropertyDisplay("進入或錄製未提供模組的任務時，在聊天視窗顯示提醒")]
    public bool ImportantDutyAlert = true;

    [PropertyDisplay("任務或野外模組開始／結束時自動錄製重播")]
    public bool AutoRecord = false;

    [PropertyDisplay("任務記錄器重播時自動錄製", tooltip: "必須先啟用自動錄製。")]
    public bool AutoARR = false;

    [PropertyDisplay("自動移除前最多保留的重播數量")]
    [PropertySlider(0, 1000)]
    public int MaxReplays = 0;

    [PropertyDisplay("在重播中記錄並儲存伺服器封包")]
    public bool RecordServerPackets = false;

    [PropertyDisplay("將伺服器封包輸出至 dalamud.log")]
    public bool DumpServerPackets = false;

    [PropertyDisplay("輸出至 dalamud.log 時忽略其他玩家的封包")]
    public bool DumpServerPacketsPlayerOnly = false;

    [PropertyDisplay("將客戶端封包輸出至 dalamud.log")]
    public bool DumpClientPackets = false;

    [PropertyDisplay("錄製紀錄的格式")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;

    [PropertyDisplay("插件重新載入時開啟先前未關閉的重播")]
    public bool RememberReplays;

    [PropertyDisplay("記住先前開啟重播的播放位置")]
    public bool RememberReplayTimes;

    // TODO: this should not be part of the actual config! figure out where to store transient user preferences...
    public List<ReplayMemory> ReplayHistory = [];

    public string ReplayFolder = "";
}
