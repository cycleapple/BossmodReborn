using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using System.Diagnostics;
using System.IO;

namespace BossMod;

public sealed class AboutTab(DirectoryInfo? replayDir)
{
    private static readonly Color TitleColor = Color.FromComponents(255u, 165u, default);
    private static readonly Color SectionBgColor = Color.FromComponents(38u, 38u, 38u);
    private static readonly Color BorderColor = Color.FromComponents(178u, 178u, 178u, 204u);
    private static readonly Color DiscordColor = Color.FromComponents(88u, 101u, 242u);

    private string _lastErrorMessage = "";

    public void Draw()
    {
        using var wrap = ImRaii.TextWrapPos(0);

        ImGui.TextUnformatted("BossModReborn（BMR）提供首領戰雷達、自動循環、冷卻規劃與 AI 功能。所有模組都能個別啟用或停用。如需支援，請前往本頁籤底部連結的 Discord 伺服器。");
        ImGui.TextUnformatted("這是原版 BossMod（VBM）的分支版本。請只在 Combat Reborn Discord 尋求支援。");
        ImGui.TextUnformatted("請勿同時載入 VBM 與此分支；同時使用的結果尚未經過測試，也不在支援範圍內。");
        ImGui.Spacing();
        DrawSection("雷達",
        [
            "在畫面上顯示區域小地圖，標示玩家與首領位置、即將出現的範圍攻擊（AOE）及其他機制。",
            "不必記住每個技能名稱也能理解機制。",
            "可精確確認自己是否會被即將到來的 AOE 擊中。",
            "可在「支援的首領」頁籤查看並啟用支援的首領。",
        ]);
        ImGui.Spacing();
        DrawSection("自動循環",
        [
            "盡可能執行最佳化的技能循環。",
            "前往「自動循環預設」頁籤建立預設。",
            "各循環模組的完成度會顯示於工具提示中。",
            "此功能的使用指南可在 Wiki 查看。",
        ]);
        ImGui.Spacing();
        DrawSection("冷卻規劃器",
        [
            "為支援的首領建立冷卻技能計畫。",
            "可在特定戰鬥中取代自動循環。",
            "讓指定技能在預定時間施放。",
            "此功能的使用指南可在 Wiki 查看。",
        ]);
        ImGui.Spacing();
        DrawSection("AI",
        [
            "在首領戰中自動移動。",
            "依照首領模組判定並顯示於雷達的安全區域，自動移動角色。",
            "與不熟悉的玩家組隊時不建議使用。",
            "其他插件可透過介接功能自動執行完整任務。",
        ]);
        ImGui.Spacing();
        DrawSection("重播",
        [
            "可用於建立首領模組、分析模組問題及製作冷卻技能計畫。",
            "尋求協助時請提供重播。請注意，重播會包含你的角色名稱！",
            "請在「設定 > 顯示重播管理介面」啟用（或啟用自動錄製）。",
            $"檔案位於「{replayDir}」。",
        ]);
        ImGui.Spacing();
        ImGui.Spacing();

        using (ImRaii.PushColor(ImGuiCol.Button, DiscordColor.ABGR))
            if (ImGui.Button("Combat Reborn Discord", new(220, 0)))
                _lastErrorMessage = OpenLink("https://discord.gg/p54TZMPnC9");
        ImGui.SameLine();
        if (ImGui.Button("BossModReborn GitHub", new(220, 0)))
            _lastErrorMessage = OpenLink("https://github.com/FFXIV-CombatReborn/BossmodReborn");
        ImGui.SameLine();
        if (ImGui.Button("BossMod Wiki", new(130, 0)))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod/wiki");
        ImGui.SameLine();
        if (ImGui.Button("開啟重播資料夾", new(180, 0)) && replayDir != null)
            _lastErrorMessage = OpenDirectory(replayDir);

        if (_lastErrorMessage.Length > 0)
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor3);
            ImGui.TextUnformatted(_lastErrorMessage);
        }
    }

    private static void DrawSection(string title, string[] bulletPoints)
    {
        using var colorBackground = ImRaii.PushColor(ImGuiCol.ChildBg, SectionBgColor.ABGR);
        using var colorBorder = ImRaii.PushColor(ImGuiCol.Border, BorderColor.ABGR);
        var height = ImGui.GetTextLineHeightWithSpacing() * (bulletPoints.Length + 2);
        using var section = ImRaii.Child(title, new(0, height), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysUseWindowPadding);

        if (!section)
            return;

        using (ImRaii.PushColor(ImGuiCol.Text, TitleColor.ABGR))
            ImGui.TextUnformatted(title);

        ImGui.Separator();
        ImGui.PushTextWrapPos();
        foreach (var point in bulletPoints)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextUnformatted(point);
        }
        ImGui.PopTextWrapPos();
    }

    private static string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return $"無法開啟連結「{link}」，請手動使用瀏覽器開啟。";
        }
    }

    private static string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"找不到資料夾「{dir}」。";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"無法開啟資料夾「{dir}」，請手動開啟。";
        }
    }
}
