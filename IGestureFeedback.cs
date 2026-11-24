using static A4TechControl.NativeMethods;

namespace A4TechControl;

public interface IGestureFeedback
{
    void Update(POINT start, POINT current, string? direction, string? action);
    void Hide();
}
