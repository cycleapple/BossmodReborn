using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayAnalysis;

sealed class AnalysisManager : IDisposable
{
    private class Lazy<T>(Func<T> init)
    {
        private readonly Func<T> _init = init;
        private T? _impl;

        public T Get() => _impl ??= _init();
    }

    private sealed class Global(List<Replay> replays)
    {
        private readonly Lazy<UnknownActionEffects> _unkEffects = new(() => new(replays));
        private readonly Lazy<ParticipantInfo> _participantInfo = new(() => new(replays));
        private readonly Lazy<AbilityInfo> _abilityInfo = new(() => new(replays));
        private readonly Lazy<ClassDefinitions> _classDefinitions = new(() => new(replays));
        private readonly Lazy<ClientActions> _clientActions = new(() => new(replays));
        private readonly Lazy<EffectResultMispredict> _effectResultMissing = new(() => new(replays, true));
        private readonly Lazy<EffectResultMispredict> _effectResultUnexpected = new(() => new(replays, false));
        private readonly Lazy<EffectResultReorder> _effectResultReorder = new(() => new(replays));

        public void Draw(UITree tree)
        {
            foreach (var n in tree.Node("未知技能效果"))
                _unkEffects.Get().Draw(tree);

            foreach (var n in tree.Node("參與者資訊", false, Colors.TextColor1, () => _participantInfo.Get().DrawContextMenu()))
                _participantInfo.Get().Draw(tree);

            foreach (var n in tree.Node("技能資訊", false, Colors.TextColor1, () => _abilityInfo.Get().DrawContextMenu()))
                _abilityInfo.Get().Draw(tree);

            foreach (var n in tree.Node("玩家職業定義"))
                _classDefinitions.Get().Draw(tree);

            foreach (var n in tree.Node("客戶端技能異常"))
                _clientActions.Get().Draw(tree);

            foreach (var n in tree.Node("效果結果：缺少確認"))
                _effectResultMissing.Get().Draw(tree);

            foreach (var n in tree.Node("效果結果：非預期確認"))
                _effectResultUnexpected.Get().Draw(tree);

            foreach (var n in tree.Node("效果結果：順序變更"))
                _effectResultReorder.Get().Draw(tree);
        }
    }

    private class PerEncounter
    {
        private readonly Lazy<StateTransitionTimings> _transitionTimings;
        private readonly Lazy<ParticipantInfo> _participantInfo;
        private readonly Lazy<AbilityInfo> _abilityInfo;
        private readonly Lazy<StatusInfo> _statusInfo;
        private readonly Lazy<IconInfo> _iconInfo;
        private readonly Lazy<TetherInfo> _tetherInfo;
        private readonly Lazy<MapEffectInfo> _mapEffectInfo;
        private readonly Lazy<DirectorInfo> _directorInfo;
        private readonly Lazy<ArenaBounds> _arenaBounds;
        private readonly Lazy<TEASpecific>? _teaSpecific;
        private readonly Lazy<TOPSpecific>? _topSpecific;

        public PerEncounter(List<Replay> replays, uint oid)
        {
            _transitionTimings = new(() => new(replays, oid));
            _participantInfo = new(() => new(replays, oid));
            _abilityInfo = new(() => new(replays, oid));
            _statusInfo = new(() => new(replays, oid));
            _iconInfo = new(() => new(replays, oid));
            _tetherInfo = new(() => new(replays, oid));
            _mapEffectInfo = new(() => new(replays, oid));
            _directorInfo = new(() => new(replays, oid));
            _arenaBounds = new(() => new(replays, oid));
            if (oid == (uint)Shadowbringers.Ultimate.TEA.OID.BossP1)
                _teaSpecific = new(() => new(replays, oid));
            if (oid == (uint)Endwalker.Ultimate.TOP.OID.Boss)
                _topSpecific = new(() => new(replays, oid));
        }

        public void Draw(UITree tree)
        {
            foreach (var n in tree.Node("狀態轉換時間"))
                _transitionTimings.Get().Draw(tree);

            foreach (var n in tree.Node("參與者資訊", false, Colors.TextColor1, () => _participantInfo.Get().DrawContextMenu()))
                _participantInfo.Get().Draw(tree);

            foreach (var n in tree.Node("技能資訊", false, Colors.TextColor1, () => _abilityInfo.Get().DrawContextMenu()))
                _abilityInfo.Get().Draw(tree);

            foreach (var n in tree.Node("狀態資訊", false, Colors.TextColor1, () => _statusInfo.Get().DrawContextMenu()))
                _statusInfo.Get().Draw(tree);

            foreach (var n in tree.Node("圖示資訊", false, Colors.TextColor1, () => _iconInfo.Get().DrawContextMenu()))
                _iconInfo.Get().Draw(tree);

            foreach (var n in tree.Node("連線資訊", false, Colors.TextColor1, () => _tetherInfo.Get().DrawContextMenu()))
                _tetherInfo.Get().Draw(tree);

            foreach (var n in tree.Node("地圖效果資訊", false, Colors.TextColor1))
                _mapEffectInfo.Get().Draw(tree);
            foreach (var n in tree.Node("Director 更新資訊", false, Colors.TextColor1))
                _directorInfo.Get().Draw(tree);

            foreach (var n in tree.Node("場地邊界", false, Colors.TextColor1, () => _arenaBounds.Get().DrawContextMenu()))
                _arenaBounds.Get().Draw(tree);

            if (_teaSpecific != null)
                foreach (var n in tree.Node("TEA-specific analysis"))
                    _teaSpecific.Get().Draw(tree);

            if (_topSpecific != null)
                foreach (var n in tree.Node("TOP-specific analysis"))
                    _topSpecific.Get().Draw(tree);
        }
    }

    private readonly List<Replay> _replays;
    private readonly Global _global;
    private readonly Dictionary<uint, PerEncounter> _perEncounter = []; // key = encounter OID
    private readonly UITree _tree = new();

    public AnalysisManager(List<Replay> replays)
    {
        _replays = replays;
        _global = new(_replays);
        InitEncounters();
    }

    public void Dispose()
    {
    }

    public void Draw()
    {
        ImGui.TextUnformatted($"找到 {_replays.Count} 份記錄");
        foreach (var n in _tree.Node("全域分析"))
        {
            _global.Draw(_tree);
        }
        foreach (var n in _tree.Nodes(_perEncounter, kv => new($"遭遇戰分析：{kv.Key:X}（{BossModuleRegistry.FindByOID(kv.Key)?.ModuleType.Name}）")))
        {
            n.Value.Draw(_tree);
        }
    }

    private void InitEncounters()
    {
        foreach (var replay in _replays)
            foreach (var e in replay.Encounters)
                if (!_perEncounter.ContainsKey(e.OID))
                    _perEncounter[e.OID] = new(_replays, e.OID);
    }
}
