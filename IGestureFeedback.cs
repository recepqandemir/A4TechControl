using static A4TechCompanion.NativeMethods;

namespace A4TechCompanion;

public interface IGestureFeedback
{
    void Update(POINT start, POINT current, string? direction, string? action);
    void Hide();
}
