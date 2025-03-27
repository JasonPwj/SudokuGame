using CommunityToolkit.Maui.Animations;
using CommunityToolkit.Maui.Extensions;

namespace SudokuGame.Animations
{
  internal class PaintTheRainbowAnimation : BaseAnimation
  {
    public override async Task Animate(VisualElement view, CancellationToken token = default)
    {
      await view.BackgroundColorTo(Colors.Red);
      await view.BackgroundColorTo(Colors.Orange);
      await view.BackgroundColorTo(Colors.Yellow);
      await view.BackgroundColorTo(Colors.Green);
      await view.BackgroundColorTo(Colors.Blue);
      await view.BackgroundColorTo(Colors.Indigo);
      await view.BackgroundColorTo(Colors.Violet);
      await view.BackgroundColorTo(Colors.Transparent);
    }
  }
}
