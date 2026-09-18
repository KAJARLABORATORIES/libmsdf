using osu.Framework.Graphics.Primitives;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Text;

public readonly record struct MsdfGlyph(float Advance, RectangleF? PlaneBounds, RectangleF? AtlasBounds);
