using System.Diagnostics;

namespace Portic.Core.Observability;

/// <summary>
/// Telemetry surface for the gateway. Portic emits standard <see cref="ActivitySource"/> spans rather
/// than re-implementing a telemetry stack — a host binds this to OpenTelemetry / Fabric telemetry.
///
/// GUARDRAIL (AGENTS.md): telemetry is a Fabric concern. This is a thin, standard emission point, not
/// a local telemetry pipeline — see docs/adr/0002. Spans carry provider/model/token tags only;
/// <b>never prompt or completion content</b>.
/// </summary>
public static class PorticTelemetry
{
    public const string ActivitySourceName = "Portic.Gateway";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
