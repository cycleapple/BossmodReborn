using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public sealed class ConfigUI : IDisposable
{
    private class UINode(ConfigNode node)
    {
        public ConfigNode Node = node;
        public string Name = "";
        public int Order;
        public UINode? Parent;
        public List<UINode> Children = [];
        public string[] Tags = [];

        public List<string> Path = [];
    }

    private readonly List<UINode> _roots = [];
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();
    private readonly AboutTab _about;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;

    private readonly List<List<string>> _filterNodes = [];

    public ConfigUI(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _about = new(replayDir);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB) : null;

        _tabs.Add("設定", DrawSettings);
        _tabs.Add("支援的首領", () => _mv.Draw(_tree, _ws));
        _tabs.Add("自動循環預設", () => _presets?.Draw());
        _tabs.Add("斜線指令", DrawAvailableCommands);
        _tabs.Add("關於", _about.Draw);

        Dictionary<Type, UINode> nodes = [];
        var nodes2 = _root.Nodes;
        for (var i = 0; i < nodes2.Count; ++i)
        {
            var n = nodes2[i];
            nodes[n.GetType()] = new(n);
        }

        foreach (var (t, n) in nodes)
        {
            var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;
            n.Tags = props?.Tags ?? [];

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
        }

        SortByOrder(_roots);
        ResolvePaths(_roots, []);
    }

    private void ResolvePaths(List<UINode> nodes, IEnumerable<string> parent)
    {
        foreach (var n in nodes)
        {
            n.Path = [.. parent, n.Name];
            ResolvePaths(n.Children, n.Path);
        }
    }

    public void Dispose()
    {
        _mv.Dispose();
    }

    public void ShowTab(string name) => _tabs.Select(name);

    public void Draw()
    {
        _tabs.Draw();
    }

    private string _searchText = "";

    private void DrawSettings()
    {
        ImGui.SetNextItemWidth(300);
        if (ImGui.InputTextEx("", "搜尋設定……", ref _searchText))
            FilterNodes();

        ImGui.SameLine();
        using (ImRaii.Disabled(_searchText.Length == 0))
            if (ImGui.Button("清除"))
            {
                _searchText = "";
                FilterNodes();
            }

        DrawNodes(_roots);
    }

    private static readonly (string, string)[] _availableAICommands =
    [
        ( "on", "啟用 AI。" ),
        ( "off", "停用 AI。" ),
        ( "toggle", "切換 AI 啟用／停用。" ),
        ( "targetmaster", "切換是否以目標隊長為焦點。" ),
        ( "follow slotX", "跟隨指定隊伍欄位，例如 Slot1。" ),
        ( "follow name", "依名稱跟隨指定小隊成員。" ),
        ( "ui", "開啟或關閉 AI 選單。" ),
        ( "forbidactions", "切換禁止動作。（僅限自動循環）" ),
        ( "forbidactions on/off", "設定是否禁止動作。（僅限自動循環）" ),
        ( "forbidmovement", "切換禁止移動。" ),
        ( "forbidmovement on/off", "設定是否禁止移動。" ),
        ( "idlewhilemounted", "切換騎乘時是否閒置。" ),
        ( "idlewhilemounted on/off", "設定騎乘時是否閒置。" ),
        ( "followcombat", "切換戰鬥中是否跟隨。" ),
        ( "followcombat on/off", "設定戰鬥中是否跟隨。" ),
        ( "followmodule", "切換首領模組作用期間是否跟隨。" ),
        ( "followmodule on/off", "設定首領模組作用期間是否跟隨。" ),
        ( "followoutofcombat", "切換非戰鬥中是否跟隨。" ),
        ( "followoutofcombat on/off", "設定非戰鬥中是否跟隨目標。" ),
        ( "followtarget", "切換戰鬥中是否跟隨目標。" ),
        ( "followtarget on/off", "設定戰鬥中是否跟隨目標。" ),
        ( "positional X", "跟隨目標時切換身位。（any、rear、flank、front）" ),
        ( "maxdistancetarget X", "設定與目標的最大距離。（預設 = 2.6）" ),
        ( "maxdistanceslot X", "設定與隊伍欄位的最大距離。（預設 = 1）" ),
        ( "mindistance X", "設定與碰撞箱的最小距離。（預設 = 0）" ),
        ( "prefdistance X", "設定與禁止區域的偏好距離。（預設 = 0）" ),
        ( "movedelay X", "設定 AI 移動決策延遲。（預設 = 0）" ),
        ( "obstaclemaps", "切換是否載入障礙物地圖。" ),
        ( "obstaclemaps on/off", "設定是否載入障礙物地圖。" ),
        ( "setpresetname X", "為 AI 設定自動循環預設，例如 setpresetname vbm default。" )
    ];

    private static readonly (string, string)[] _autorotationCommands =
    [
        ( "ar clear", "清除目前預設；除非計畫正在作用，否則自動循環不會執行任何動作。" ),
        ( "ar disable", "強制停用自動循環；即使計畫正在作用，也不會自動執行動作。" ),
        ( "ar set Preset", "開始執行指定預設。" ),
        ( "ar toggle", "若尚未強制停用便停用自動循環，否則清除覆寫。" ),
        ( "ar toggle Preset", "指定預設未啟用時開始執行，已啟用時則清除。" ),
        ( "ar ui", "開啟或關閉自動循環介面。" ),
    ];

    private static readonly (string, string)[] _availableOtherCommands =
    [
        ( "restorerotation", "切換使用動作後恢復角色面向的設定。" ),
        ( "resetcolors", "將所有色彩重設為預設值。" ),
        ( "d", "開啟偵錯選單。" ),
        ( "r", "開啟重播選單。" ),
        ( "r on/off", "開始／停止錄製重播。" ),
        ( "gc", "觸發記憶體回收。" ),
        ( "cfg", "列出所有設定。" )
    ];

    private static void DrawAvailableCommands()
    {
        ImGui.Text("可用指令：");
        ImGui.Separator();
        ImGui.Text("AI:");
        ImGui.Separator();
        for (var i = 0; i < 28; ++i)
        {
            ref readonly var text = ref _availableAICommands[i];
            ImGui.Text($"/bmrai {text.Item1}: {text.Item2}");
        }
        ImGui.Separator();
        ImGui.Text("自動循環指令：");
        ImGui.Separator();
        for (var i = 0; i < 6; ++i)
        {
            ref readonly var text = ref _autorotationCommands[i];
            ImGui.Text($"/bmr {text.Item1}: {text.Item2}");
        }
        ImGui.Separator();
        ImGui.Text("其他指令：");
        ImGui.Separator();
        for (var i = 0; i < 7; ++i)
        {
            ref readonly var text = ref _availableOtherCommands[i];
            ImGui.Text($"/bmr {text.Item1}: {text.Item2}");
        }
    }

    private void FilterNodes()
    {
        _filterNodes.Clear();

        if (_searchText.Length == 0)
            return;

        foreach (var r in _roots)
            foreach (var path in WalkNodes(r))
                _filterNodes.Add(path);
    }

    private static readonly Dictionary<Type, List<(FieldInfo Field, PropertyDisplayAttribute Attr)>> _fieldCache = [];

    private List<List<string>> WalkNodes(UINode node)
    {
        var results = new List<List<string>>();
        WalkNodesInternal(node, [], results);
        return results;
    }

    private void WalkNodesInternal(UINode node, List<string> path, List<List<string>> results)
    {
        if (Utils.TextMatch(node.Name, _searchText) || TagsMatch(node.Tags))
        {
            var matchPath = new List<string>(path) { node.Name, "*" };
            results.Add(matchPath);
            return;
        }

        foreach (var (_, props) in GetFieldAttributes(node.Node.GetType()))
        {
            if (Utils.TextMatch(props.Label, _searchText) || TagsMatch(props.Tags))
            {
                var matchPath = new List<string>(path) { node.Name, props.Label };
                results.Add(matchPath);
            }
        }

        path.Add(node.Name);
        foreach (var child in node.Children)
        {
            WalkNodesInternal(child, path, results);
        }
        path.RemoveAt(path.Count - 1);
    }

    private static List<(FieldInfo, PropertyDisplayAttribute)> GetFieldAttributes(Type type)
    {
        if (_fieldCache.TryGetValue(type, out var cached))
            return cached;

        var list = new List<(FieldInfo, PropertyDisplayAttribute)>();
        foreach (var field in type.GetFields())
        {
            var attr = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (attr != null)
                list.Add((field, attr));
        }

        _fieldCache[type] = list;
        return list;
    }

    private bool TagsMatch(string[] tags)
    {
        foreach (var tag in tags)
        {
            if (Utils.TextMatch(tag, _searchText))
                return true;
        }
        return false;
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, Func<PropertyDisplayAttribute, bool>? filter = null)
    {
        // draw standard properties
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            if (filter?.Invoke(props) == false)
                continue;

            var value = field.GetValue(node);
            if (DrawProperty(props.Label, props.Tooltip, node, field, value, root, tree, ws))
            {
                node.Modified.Fire();
            }

            if (props.Separator)
            {
                ImGui.Separator();
            }
        }

        // draw custom stuff
        node.DrawCustom(tree, ws);
    }

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name[..^"Config".Length] : t.Name;

    private static void SortByOrder(List<UINode> nodes)
    {
        nodes.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        foreach (var n in nodes)
            SortByOrder(n.Children);
    }

    private void DrawNodes(List<UINode> nodes)
    {
        foreach (var n in _tree.Nodes(nodes.Where(n => MatchesFilter(n.Path)), n => new(n.Name)))
        {
            DrawNode(n.Node, _root, _tree, _ws, props => MatchesFilter([.. n.Path, props.Label]));
            DrawNodes(n.Children);
        }
    }

    private bool MatchesFilter(List<string> path)
    {
        if (_filterNodes.Count == 0)
            return true;

        bool matchesOneFilter(List<string> filter)
        {
            var i = 0;
            foreach (var f in filter)
            {
                if (f == "*" || i >= path.Count)
                    return true;

                if (f != path[i])
                    return false;

                i++;
            }

            return true;
        }

        return _filterNodes.Any(matchesOneFilter);
    }

    private static void DrawHelp(string tooltip)
    {
        // draw tooltip marker with proper alignment
        ImGui.AlignTextToFramePadding();
        if (tooltip.Length > 0)
        {
            UIMisc.HelpMarker(tooltip);
        }
        else
        {
            using var invisible = ImRaii.PushColor(ImGuiCol.Text, 0x00000000);
            UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle, "(?)");
        }
        ImGui.SameLine();
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, object? value, ConfigRoot root, UITree tree, WorldState ws) => value switch
    {
        bool v => DrawProperty(label, tooltip, node, member, v),
        Enum v => DrawProperty(label, tooltip, node, member, v),
        float v => DrawProperty(label, tooltip, node, member, v),
        int v => DrawProperty(label, tooltip, node, member, v),
        string v => DrawProperty(label, tooltip, node, member, v),
        Color v => DrawProperty(label, tooltip, node, member, v),
        Color[] v => DrawProperty(label, tooltip, node, member, v),
        GroupAssignment v => DrawProperty(label, tooltip, node, member, v, root, tree, ws),
        _ => false
    };

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, bool v)
    {
        DrawHelp(tooltip);
        var combo = member.GetCustomAttribute<PropertyComboAttribute>();
        if (combo != null)
        {
            if (UICombo.Bool(label, combo.Values, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.Checkbox(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Enum v)
    {
        DrawHelp(tooltip);
        if (UICombo.Enum(label, ref v))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, float v)
    {
        DrawHelp(tooltip);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            ImGui.SetNextItemWidth(Math.Min(ImGui.GetWindowWidth() * 0.30f, 175));
            if (ImGui.DragFloat(label, ref v, slider.Speed, slider.Min, slider.Max, "%.3f", flags))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputFloat(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, int v)
    {
        DrawHelp(tooltip);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            ImGui.SetNextItemWidth(Math.Min(ImGui.GetWindowWidth() * 0.30f, 175));
            if (ImGui.DragInt(label, ref v, slider.Speed, (int)slider.Min, (int)slider.Max, "%d", flags))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputInt(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, string v)
    {
        DrawHelp(tooltip);
        if (ImGui.InputText(label, ref v, 256))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Color v)
    {
        DrawHelp(tooltip);
        var col = v.ToFloat4();
        if (ImGui.ColorEdit4(label, ref col, ImGuiColorEditFlags.PickerHueWheel))
        {
            member.SetValue(node, Color.FromFloat4(col));
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Color[] v)
    {
        var modified = false;
        for (var i = 0; i < v.Length; ++i)
        {
            DrawHelp(tooltip);
            var col = v[i].ToFloat4();
            if (ImGui.ColorEdit4($"{label} {i}", ref col, ImGuiColorEditFlags.PickerHueWheel))
            {
                v[i] = Color.FromFloat4(col);
                member.SetValue(node, v);
                modified = true;
            }
        }
        return modified;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, GroupAssignment v, ConfigRoot root, UITree tree, WorldState ws)
    {
        var group = member.GetCustomAttribute<GroupDetailsAttribute>();
        if (group == null)
            return false;

        DrawHelp(tooltip);
        var modified = false;
        foreach (var tn in tree.Node(label, false, v.Validate() ? Colors.TextColor1 : Colors.TextColor2, () => DrawPropertyContextMenu(node, member, v)))
        {
            using var indent = ImRaii.PushIndent();
            using var table = ImRaii.Table("table", group.Names.Length + 2, ImGuiTableFlags.SizingFixedFit);
            if (!table)
                continue;

            foreach (var n in group.Names)
                ImGui.TableSetupColumn(n);
            ImGui.TableSetupColumn("----");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
            for (var i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; ++i)
            {
                var r = (PartyRolesConfig.Assignment)i;
                ImGui.TableNextRow();
                for (var c = 0; c < group.Names.Length; ++c)
                {
                    ImGui.TableNextColumn();
                    if (ImGui.RadioButton($"###{r}:{c}", v[r] == c))
                    {
                        v[r] = c;
                        modified = true;
                    }
                }
                ImGui.TableNextColumn();
                if (ImGui.RadioButton($"###{r}:---", v[r] < 0 || v[r] >= group.Names.Length))
                {
                    v[r] = -1;
                    modified = true;
                }

                var name = r.ToString();
                if (assignments.Length > 0)
                    name += $" ({ws.Party[assignments[i]]?.Name})";
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(name);
            }
        }
        return modified;
    }

    private static void DrawPropertyContextMenu(ConfigNode node, FieldInfo member, GroupAssignment v)
    {
        foreach (var preset in member.GetCustomAttributes<GroupPresetAttribute>())
        {
            if (ImGui.MenuItem(preset.Name))
            {
                for (var i = 0; i < preset.Preset.Length; ++i)
                    v.Assignments[i] = preset.Preset[i];
                node.Modified.Fire();
            }
        }
    }
}
