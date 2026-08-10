namespace BossMod.Stormblood.Ultimate.UWU;

class P1Plumes(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _razor = module.Enemies((uint)OID.RazorPlume);
    private readonly List<Actor> _spiny = module.Enemies((uint)OID.SpinyPlume);
    private readonly List<Actor> _satin = module.Enemies((uint)OID.SatinPlume);

    public bool Active => _razor.Any(p => p.IsTargetable) || _spiny.Any(p => p.IsTargetable) || _satin.Any(p => p.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_razor);
        Arena.Actors(_spiny);
        Arena.Actors(_satin);
    }
}

sealed class P1PlumeShield(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _shield = module.Enemies((uint)OID.SpinyShield);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_shield.Count > 0)
        {
            var targetpos = _shield[0].Position;
            Arena.AddCircle(targetpos, 6f, (pc.Position - targetpos).Length() <= 6f ? Colors.Safe : Colors.Danger);
        }
    }
}
