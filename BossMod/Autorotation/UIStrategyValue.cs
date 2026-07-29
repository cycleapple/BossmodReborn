using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Autorotation;

public static class UIStrategyValue
{
    private static readonly (string Name, float Value)[] PriorityBaselines =
    [
        ("極低", ActionQueue.Priority.VeryLow),
        ("低", ActionQueue.Priority.Low),
        ("中", ActionQueue.Priority.Medium),
        ("高", ActionQueue.Priority.High),
        ("極高", ActionQueue.Priority.VeryHigh),
    ];

    public static List<string> Preview(StrategyValue value, StrategyConfigTrack cfg, BossModuleRegistry.Info? moduleInfo)
    {
        switch (value)
        {
            case StrategyValueTrack t:
                var opt = cfg.Options[t.Option];
                return [
                    $"選項：{opt.UIName}",
                    $"備註：{value.Comment}",
                    $"優先度：{(float.IsNaN(t.PriorityOverride) ? $"預設（{opt.DefaultPriority:f}）" : t.PriorityOverride.ToString("f"))}",
                    $"目標：{PreviewTarget(t, moduleInfo)}"
                ];
            default:
                return [];
        }
    }

    public static string PreviewTarget(StrategyValueTrack value, BossModuleRegistry.Info? moduleInfo)
    {
        var targetDetails = value.Target switch
        {
            StrategyTarget.PartyByAssignment => ((PartyRolesConfig.Assignment)value.TargetParam).ToString(),
            StrategyTarget.PartyWithLowestHP => PreviewParam((StrategyPartyFiltering)value.TargetParam),
            StrategyTarget.EnemyWithHighestPriority => $"{(StrategyEnemySelection)value.TargetParam}",
            StrategyTarget.EnemyByOID => $"{(moduleInfo?.ObjectIDType != null ? Enum.ToObject(moduleInfo.ObjectIDType, (uint)value.TargetParam).ToString() : "???")} (0x{value.TargetParam:X})",
            StrategyTarget.PointWaymark => $"{(Waymark)value.TargetParam}",
            _ => ""
        };
        var offsetDetails = value.Target == StrategyTarget.PointAbsolute ? $" {value.Offset1}x{value.Offset2}" : value.Offset1 != 0 ? $" + R{value.Offset1}, dir={value.Offset2}" : "";
        return (targetDetails.Length > 0 ? $"{value.Target} ({targetDetails})" : $"{value.Target}") + offsetDetails;
    }

    public static bool DrawEditor(StrategyValue value, StrategyConfigTrack cfg, BossModuleRegistry.Info? moduleInfo, int? level)
    {
        var modified = false;
        if (value is StrategyValueTrack tr)
        {
            modified |= DrawEditorTrackOption(tr, cfg, level);
            modified |= ImGui.InputText("備註", ref value.Comment, 512);
            modified |= DrawEditorPriority(tr);
            modified |= DrawEditorTarget(tr, cfg.Options[tr.Option].SupportedTargets, moduleInfo);
        }
        return modified;
    }

    public static bool DrawEditorTrackOption(StrategyValueTrack value, StrategyConfigTrack cfg, int? level, string label = "選項")
    {
        var modified = false;
        using (var combo = ImRaii.Combo(label, cfg.Options[value.Option].UIName))
        {
            if (combo)
            {
                for (var i = 0; i < cfg.Options.Count; ++i)
                {
                    var opt = cfg.Options[i];
                    if (level < opt.MinLevel || level > opt.MaxLevel)
                        continue; // filter out options outside our level

                    if (ImGui.Selectable(cfg.Options[i].UIName, i == value.Option))
                    {
                        modified = true;
                        value.Option = i;
                    }
                }
            }
        }
        return modified;
    }

    public static bool DrawEditorPriority(StrategyValueTrack value)
    {
        var modified = false;
        var overridePriority = !float.IsNaN(value.PriorityOverride);
        if (ImGui.Checkbox("覆寫優先度", ref overridePriority))
        {
            modified = true;
            value.PriorityOverride = overridePriority ? ActionQueue.Priority.Low : float.NaN;
        }
        ImGui.SameLine();
        UIMisc.HelpMarker("""
            設定對應技能的自訂優先度。
            優先度會與其他候選技能比較；建議選用預設基準，再加上少量偏移以區分多個技能。
            各基準優先度如下：
            * 極低（1000）— 只有沒有其他技能可按時才會使用。
            * 低（2000）— 只有不會延後任何輸出技能時才會使用（例如沒有溢出風險時，可能延後消耗第二層充能）。
            * 中（3000）— 會在下一個可用的能力技空檔使用，但不會延後戰技／魔法或極重要的能力技；通常每個 GCD 至少會留一個空檔給中優先度技能。
            * 高（4000）— 會在下一個可用的能力技空檔使用；不會延後 GCD，但若使用不慎，某些情況可能打亂循環。
            * 極高（5000）— 會儘快使用，必要時也會延後 GCD。
            """);

        if (overridePriority)
        {
            var priority = value.PriorityOverride;
            var upperBound = Array.FindIndex(PriorityBaselines, b => b.Value > priority);
            var baselineIndex = upperBound switch
            {
                -1 => PriorityBaselines.Length - 1,
                0 => 0,
                _ => upperBound - 1
            };
            var priorityDelta = value.PriorityOverride - PriorityBaselines[baselineIndex].Value;

            using var indent = ImRaii.PushIndent();
            ImGui.SetNextItemWidth(100);
            using (var combo = ImRaii.Combo("###baseline", PriorityBaselines[baselineIndex].Name))
            {
                if (combo)
                {
                    for (var i = 0; i < PriorityBaselines.Length; ++i)
                    {
                        if (ImGui.Selectable(PriorityBaselines[i].Name, i == baselineIndex))
                        {
                            modified = true;
                            value.PriorityOverride = PriorityBaselines[i].Value + priorityDelta;
                        }
                    }
                }
            }
            ImGui.SameLine();
            ImGui.TextUnformatted("+");
            ImGui.SameLine();
            if (ImGui.InputFloat("###delta", ref priorityDelta))
            {
                modified = true;
                value.PriorityOverride = PriorityBaselines[baselineIndex].Value + priorityDelta;
            }
        }

        return modified;
    }

    public static bool DrawEditorTarget(StrategyValueTrack value, ActionTargets supportedTargets, BossModuleRegistry.Info? moduleInfo)
    {
        var modified = false;
        using (var combo = ImRaii.Combo("目標", value.Target.ToString()))
        {
            if (combo)
            {
                for (var i = StrategyTarget.Automatic; i < StrategyTarget.Count; ++i)
                {
                    if (AllowTarget(i, supportedTargets, moduleInfo) && ImGui.Selectable(i.ToString(), i == value.Target))
                    {
                        value.Target = i;
                        value.TargetParam = 0;
                        modified = true;
                    }
                }
            }
        }

        using var indent = ImRaii.PushIndent();
        switch (value.Target)
        {
            case StrategyTarget.PartyByAssignment:
                modified |= DrawEditorTargetParamCombo<PartyRolesConfig.Assignment>(ref value.TargetParam, "職責分配");
                break;
            case StrategyTarget.PartyWithLowestHP:
                if (supportedTargets.HasFlag(ActionTargets.Self))
                    modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.IncludeSelf, "允許自己", false);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeTanks, "允許防護職業", true);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeHealers, "允許治療職業", true);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeMelee, "允許近戰職業", true);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeRanged, "允許遠程職業", true);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeNoPredictedDamage, "僅限預期會受到更多傷害時", false);
                break;
            case StrategyTarget.EnemyWithHighestPriority:
                modified |= DrawEditorTargetParamCombo<StrategyEnemySelection>(ref value.TargetParam, "選擇條件");
                break;
            case StrategyTarget.EnemyByOID:
                if (moduleInfo?.ObjectIDType != null)
                {
                    var v = (Enum)Enum.ToObject(moduleInfo.ObjectIDType, (uint)value.TargetParam);
                    if (UICombo.Enum("OID", ref v))
                    {
                        value.TargetParam = (int)(uint)(object)v;
                        modified = true;
                    }
                }
                break;
            case StrategyTarget.PointWaymark:
                var wm = (Waymark)value.TargetParam;
                if (UICombo.Enum("場地標記", ref wm))
                {
                    value.TargetParam = (int)wm;
                    modified = true;
                }
                break;
        }

        if (supportedTargets.HasFlag(ActionTargets.Area))
        {
            if (value.Target == StrategyTarget.PointAbsolute)
            {
                modified |= ImGui.InputFloat("X", ref value.Offset1);
                modified |= ImGui.InputFloat("Z", ref value.Offset2);
            }
            else
            {
                modified |= ImGui.DragFloat("偏移", ref value.Offset1, 0.1f, 0, 30);
                modified |= ImGui.DragFloat("方向", ref value.Offset2, 1, -180, 180);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("以度為單位；0 為南，逆時針增加（90 為東、180 為北、-90 為西）");
            }
        }

        return modified;
    }

    public static bool AllowTarget(StrategyTarget t, ActionTargets supported, BossModuleRegistry.Info? moduleInfo) => supported.HasFlag(ActionTargets.Area) || t switch
    {
        StrategyTarget.Self => supported.HasFlag(ActionTargets.Self),
        StrategyTarget.PartyByAssignment => supported.HasFlag(ActionTargets.Party),
        StrategyTarget.PartyWithLowestHP => supported.HasFlag(ActionTargets.Party),
        StrategyTarget.EnemyWithHighestPriority => supported.HasFlag(ActionTargets.Hostile),
        StrategyTarget.EnemyByOID => supported.HasFlag(ActionTargets.Hostile) && moduleInfo != null,
        StrategyTarget.PointAbsolute or StrategyTarget.PointCenter or StrategyTarget.PointWaymark => false,
        _ => true
    };

    private static string PreviewParam(StrategyPartyFiltering pf)
    {
        string excludeIfSet(StrategyPartyFiltering flag, string value) => pf.HasFlag(flag) ? $"、排除{value}" : "";
        return $"{(pf.HasFlag(StrategyPartyFiltering.IncludeSelf) ? "包含" : "排除")}自己"
            + excludeIfSet(StrategyPartyFiltering.ExcludeTanks, "防護職業")
            + excludeIfSet(StrategyPartyFiltering.ExcludeHealers, "治療職業")
            + excludeIfSet(StrategyPartyFiltering.ExcludeMelee, "近戰職業")
            + excludeIfSet(StrategyPartyFiltering.ExcludeRanged, "遠程職業")
            + excludeIfSet(StrategyPartyFiltering.ExcludeNoPredictedDamage, "預期不會受到傷害的玩家");
    }

    private static bool DrawEditorTargetParamCombo<E>(ref int current, string text) where E : Enum
    {
        var value = (E)(object)current;
        if (!UICombo.Enum(text, ref value))
            return false;
        current = (int)(object)value;
        return true;
    }

    private static bool DrawEditorTargetParamFlags(ref int current, StrategyPartyFiltering flag, string text, bool inverted)
    {
        var isChecked = ((StrategyPartyFiltering)current).HasFlag(flag) != inverted;
        if (!ImGui.Checkbox(text, ref isChecked))
            return false;
        current ^= (int)flag;
        return true;
    }
}

[AttributeUsage(AttributeTargets.Enum)]
public sealed class RendererAttribute(Type type) : Attribute
{
    public Type Type => type;
}

public class RendererFactory
{
    private static RendererFactory? _instance;
    private readonly Dictionary<Type, IStrategyRenderer> _dict = [];

    public static bool Draw(StrategyConfig config, ref StrategyValue value)
    {
        var inst = (_instance ??= new()).Get(config.Renderer);

        ImGui.TableNextRow();
        using var _ = ImRaii.PushId(config.InternalName);
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        inst.DrawLabel(config);
        ImGui.TableNextColumn();
        return inst.DrawValue(config, ref value);
    }

    private IStrategyRenderer Get(Type t) => _dict.TryGetValue(t, out var r) ? r : (_dict[t] = (IStrategyRenderer)Activator.CreateInstance(t)!);
}

public interface IStrategyRenderer
{
    public void DrawLabel(StrategyConfig config);
    public bool DrawValue(StrategyConfig config, ref StrategyValue value);
}

public class TrackRenderer : IStrategyRenderer
{
    public virtual void DrawLabel(StrategyConfig config) => ImGui.TextWrapped(config.UIName);
    public bool DrawValue(StrategyConfig config, ref StrategyValue value)
    {
        var v = (StrategyValueTrack)value;
        if (DrawValue((StrategyConfigTrack)config, ref v))
        {
            value = v;
            return true;
        }
        return false;
    }

    public virtual bool DrawValue(StrategyConfigTrack config, ref StrategyValueTrack value) => UICombo.EnumIndex("", config.OptionEnum, ref value.Option, ix => config.Options[ix].DisplayName.Length > 0 ? config.Options[ix].DisplayName : UICombo.EnumString((Enum)config.OptionEnum.GetEnumValues().GetValue(ix)!));
}

public class FloatRenderer : IStrategyRenderer
{
    public void DrawLabel(StrategyConfig config) => ImGui.TextWrapped(config.UIName);
    public bool DrawValue(StrategyConfig config, ref StrategyValue value)
    {
        var cfg = (StrategyConfigFloat)config;
        var f = ((StrategyValueFloat)value).Value;
        ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
        if (cfg.Drag)
        {
            if (ImGui.DragFloat("", ref f, cfg.Speed, cfg.MinValue, cfg.MaxValue))
            {
                value = new StrategyValueFloat() { Value = f };
                return true;
            }
        }
        else
        {
            if (ImGui.InputFloat("", ref f, cfg.Speed))
            {
                value = new StrategyValueFloat() { Value = f };
                return true;
            }
        }

        return false;
    }
}

public class IntRenderer : IStrategyRenderer
{
    public void DrawLabel(StrategyConfig config) => ImGui.TextWrapped(config.UIName);
    public bool DrawValue(StrategyConfig config, ref StrategyValue value)
    {
        var cfg = (StrategyConfigInt)config;
        var f = ((StrategyValueInt)value).Value;
        ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
        if (cfg.Drag)
        {
            if (ImGui.DragLong("", ref f, cfg.Speed, cfg.MinValue, cfg.MaxValue))
            {
                value = new StrategyValueInt() { Value = f };
                return true;
            }
        }
        else
        {
            if (ImGui.InputLong("", ref f, (long)cfg.Speed))
            {
                value = new StrategyValueInt() { Value = f };
                return true;
            }
        }

        return false;
    }
}
