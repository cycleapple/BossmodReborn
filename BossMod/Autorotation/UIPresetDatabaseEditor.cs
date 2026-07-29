using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BossMod.Autorotation;

// note: the editor assumes it's the only thing that modifies the database instance; having multiple editors or editing database externally will break things
public sealed class UIPresetDatabaseEditor(RotationDatabase rotationDB)
{
    private readonly PresetDatabase PresetDB = rotationDB.Presets;

    private int _selectedPresetIndex = -1;
    private bool _selectedPresetDefault;
    private int _pendingSelectPresetIndex = -1; // if >= 0, we want to select different preset, but current one has modifications
    private bool _pendingSelectPresetDefault;
    private Type? _selectedModuleType; // we want module selection to be persistent when changing presets
    private UIPresetEditor? _selectedPreset;

    private readonly AutorotationConfig _cfg = Service.Config.Get<AutorotationConfig>();

    private bool HaveUnsavedModifications => _selectedPreset?.Modified ?? false;

    public void Draw()
    {
        if (_pendingSelectPresetIndex >= 0)
            DrawPendingSwitch();
        DrawPresetSelector();
        if (_selectedPreset != null)
        {
            _selectedPreset.Draw();
            _selectedModuleType = _selectedPreset.SelectedModuleType ?? _selectedModuleType;
        }
        else
        {
            ImGui.TextUnformatted("請選擇要編輯的預設，或建立新預設。");
        }
    }

    private void DrawPendingSwitch()
    {
        if (_pendingSelectPresetIndex < 0)
            return;
        if (!HaveUnsavedModifications)
        {
            CompleteChangeCurrentPreset();
            return;
        }

        ImGui.OpenPopup("未儲存的變更"); // TODO: why do i have to do it every frame???
        var modalOpen = true;
        using var modal = ImRaii.PopupModal("未儲存的變更", ref modalOpen, ImGuiWindowFlags.AlwaysAutoResize);
        if (!modal)
            return;
        ImGui.TextUnformatted($"目前開啟的預設「{_selectedPreset?.Preset.Name}」有未儲存的變更。");
        ImGui.TextUnformatted("選擇其他預設前，必須先儲存或捨棄這些變更。");
        ImGui.TextUnformatted("要如何繼續？");
        if (DrawSaveCurrentPresetButton())
        {
            SaveCurrentPreset();
            CompleteChangeCurrentPreset();
        }
        ImGui.SameLine();
        if (UIMisc.Button("另存副本", _selectedPresetIndex < 0, "無法將新預設另存為副本"))
        {
            SaveCurrentPresetAsCopy();
            CompleteChangeCurrentPreset();
        }
        ImGui.SameLine();
        if (ImGui.Button("捨棄"))
        {
            CompleteChangeCurrentPreset();
        }
        ImGui.SameLine();
        if (ImGui.Button("取消") || !modalOpen)
        {
            _pendingSelectPresetIndex = -1;
        }
        if (_pendingSelectPresetIndex < 0)
            ImGui.CloseCurrentPopup();
    }

    private void DrawPresetSelector()
    {
        UIMisc.HelpMarker("""
            若要開始使用自動循環，請先建立一個「預設」。
            預設可設定循環「模組」及其「策略」。
            模組會評估遊戲狀態，並建立具有優先順序的候選技能清單。
            自動循環框架會從清單選出優先度最高的技能，在下一個可用時機執行。
            每個模組都能透過一組「策略」進一步設定，以調整不同面向的行為。
            例如，可建立「單體」與「範圍」預設，兩者使用相同模組，但採用不同策略。
            也可為每個策略值指定鍵盤輔助鍵；只有按住該鍵時，對應數值才會生效。
            例如，可設定成按住 Shift 時延後兩分鐘爆發。
            """);
        ImGui.SameLine();

        ImGui.SetNextItemWidth(200);
        using (var combo = ImRaii.Combo("預設", _selectedPreset == null ? "" : _selectedPresetIndex < 0 ? "<新增>" : (_selectedPresetDefault ? PresetDB.DefaultPresets : PresetDB.UserPresets)[_selectedPresetIndex].Name))
        {
            if (combo)
            {
                if (!_cfg.HideDefaultPreset)
                    DrawPresetListElements(true);
                DrawPresetListElements(false);
            }
        }

        ImGui.SameLine();
        if (DrawSaveCurrentPresetButton())
            SaveCurrentPreset();
        ImGui.SameLine();
        if (UIMisc.Button("另存副本", _selectedPresetIndex < 0, "無法將新預設另存為副本"))
            SaveCurrentPresetAsCopy();
        ImGui.SameLine();
        if (UIMisc.Button("還原", 0, (!HaveUnsavedModifications, "目前預設沒有變更"), (_selectedPresetIndex < 0, "尚未選擇預設")))
            RevertCurrentPreset();
        ImGui.SameLine();
        if (UIMisc.Button("新增", HaveUnsavedModifications, "目前預設已變更，請先儲存或捨棄變更"))
            CreateNewPreset(-1, false);
        ImGui.SameLine();
        if (UIMisc.Button("複製", 0, (HaveUnsavedModifications, "目前預設已變更，請先儲存或捨棄變更"), (_selectedPresetIndex < 0, "尚未選擇預設")))
            CreateNewPreset(_selectedPresetIndex, _selectedPresetDefault);
        ImGui.SameLine();
        if (UIMisc.Button("刪除", 0, (_selectedPresetDefault, "無法刪除內建預設；若要隱藏，請前往「設定 → 自動循環」。"), (!ImGui.GetIO().KeyShift, "按住 Shift 以刪除"), (_selectedPresetIndex < 0, "尚未選擇預設")))
            DeleteCurrentPreset();
        ImGui.SameLine();
        if (UIMisc.Button("匯出", _selectedPreset == null, "尚未選擇預設"))
            ExportToClipboard();
        ImGui.SameLine();
        if (UIMisc.Button("匯入", HaveUnsavedModifications, "目前預設已變更，請先儲存或捨棄變更"))
            ImportNewPresetFromClipboard();
    }

    private void DrawPresetListElements(bool defaultPresets)
    {
        var presets = defaultPresets ? PresetDB.DefaultPresets : PresetDB.UserPresets;
        for (int i = 0; i < presets.Count; ++i)
        {
            var preset = presets[i];
            if (ImGui.Selectable(preset.Name, _selectedPresetDefault == defaultPresets && _selectedPresetIndex == i))
            {
                _pendingSelectPresetIndex = i;
                _pendingSelectPresetDefault = defaultPresets;
            }

            if (!defaultPresets && ImGui.IsItemActive() && !ImGui.IsItemHovered())
            {
                var j = ImGui.GetMouseDragDelta().Y < 0 ? i - 1 : i + 1;
                if (j >= 0 && j < presets.Count)
                {
                    (presets[i], presets[j]) = (presets[j], presets[i]);
                    if (_selectedPresetIndex == i && _selectedPresetDefault == defaultPresets)
                        _selectedPresetIndex = j;
                    else if (_selectedPresetIndex == j && _selectedPresetDefault == defaultPresets)
                        _selectedPresetIndex = i;
                    PresetDB.Modify(-1, null);
                    ImGui.ResetMouseDragDelta();
                }
            }
        }
    }

    private bool DrawSaveCurrentPresetButton() => UIMisc.Button("儲存", 0, (!HaveUnsavedModifications, "目前預設沒有變更"), (_selectedPreset?.NameConflict ?? false, "目前預設名稱為空白，或與其他既有預設重複"));

    private void RevertCurrentPreset() => _selectedPreset = new(PresetDB, _selectedPresetIndex, _selectedPresetDefault, _selectedModuleType);

    private void SaveCurrentPreset()
    {
        if (!_selectedPresetDefault && _selectedPreset != null && _selectedPreset.Modified && !_selectedPreset.NameConflict)
        {
            PresetDB.Modify(_selectedPresetIndex, _selectedPreset.Preset);
            if (_selectedPresetIndex < 0)
                _selectedPresetIndex = PresetDB.UserPresets.Count - 1;
            RevertCurrentPreset();
        }
        else
        {
            Service.Log($"[PD] Save called when current preset #{_selectedPresetIndex} (default={_selectedPresetDefault}) is not modified or has bad name '{_selectedPreset?.Preset.Name}'");
        }
    }

    private void SaveCurrentPresetAsCopy()
    {
        if (_selectedPresetIndex >= 0 && _selectedPreset != null)
        {
            _selectedPreset.DetachFromSource();
            _selectedPreset.MakeNameUnique();
            _selectedPresetIndex = PresetDB.UserPresets.Count;
            _selectedPresetDefault = false;
            PresetDB.Modify(-1, _selectedPreset.Preset);
            RevertCurrentPreset();
        }
        else
        {
            Service.Log($"[PD] Save-as called when no preset is selected");
        }
    }

    private void CreateNewPreset(int referenceIndex, bool referenceDefault)
    {
        _selectedPresetIndex = -1;
        _selectedPresetDefault = false;
        _selectedPreset = new(PresetDB, referenceIndex, referenceDefault, _selectedModuleType);
        _selectedPreset.DetachFromSource();
        _selectedPreset.MakeNameUnique();
    }

    private void DeleteCurrentPreset()
    {
        if (!_selectedPresetDefault && _selectedPresetIndex >= 0)
        {
            PresetDB.Modify(_selectedPresetIndex, null);
            _selectedPresetIndex = -1;
            _selectedPreset = null;
        }
        else
        {
            Service.Log($"[PD] Delete called default or no preset is selected (index={_selectedPresetIndex}, default={_selectedPresetDefault})");
        }
    }

    private void CompleteChangeCurrentPreset()
    {
        _selectedPresetIndex = _pendingSelectPresetIndex;
        _selectedPresetDefault = _pendingSelectPresetDefault;
        _pendingSelectPresetIndex = -1;
        _pendingSelectPresetDefault = false;
        RevertCurrentPreset();
    }

    private void ExportToClipboard()
    {
        if (_selectedPreset != null)
        {
            ImGui.SetClipboardText(JsonSerializer.Serialize(_selectedPreset.Preset, Serialization.BuildSerializationOptions()));
        }
        else
        {
            Service.Log($"[PD] Export called no preset is selected");
        }
    }

    private void ImportNewPresetFromClipboard()
    {
        try
        {
            var finfo = new FileInfo("<import from clipboard>");

            // let users import encounter-specific plans from here for convenience
            var json = JsonNode.Parse(ImGui.GetClipboardText());
            if (json?.AsObject()?.ContainsKey("Encounter") == true)
            {
                foreach (var conv in PlanPresetConverter.PlanSchema.Converters)
                    json = conv(json, 0, finfo);

                var plan = JsonSerializer.Deserialize<Plan>(json, Serialization.BuildSerializationOptions())!;
                plan.Guid = Guid.NewGuid().ToString();

                rotationDB.Plans.ModifyPlan(null, plan);

                Service.Notifications.AddNotification(new()
                {
                    Content = $"已匯入 {plan.Class} 等級 {plan.Level} 的規劃「{plan.Name}」"
                });

                return;
            }

            json = new JsonArray(json);

            foreach (var conv in PlanPresetConverter.PresetSchema.Converters)
                json = conv(json, 0, finfo);

            var preset = JsonSerializer.Deserialize<Preset>(json.AsArray()[0], Serialization.BuildSerializationOptions())!;
            _selectedPresetIndex = -1;
            _selectedPresetDefault = false;
            _selectedPreset = new(PresetDB, preset, _selectedModuleType);
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to parse preset: {ex}");
        }
    }
}
