using CommunityToolkit.Maui.Animations;

namespace SudokuGame.Animations
{
  public class LabelMixedAnimation : BaseAnimation
  {
    private static Random _random = new Random();
    public const int ColorMax = 7;
    private static Color[] _colors = new Color[ColorMax]
    {
      Colors.Red,
      Colors.Orange,
      Colors.Yellow,
      Colors.Green,
      Colors.Blue,
      Colors.Indigo,
      Colors.Violet
    };

    public override async Task Animate(VisualElement view, CancellationToken token = default)
    {
      if (view is Label label)
      {
        var half = Length / 2;
        var color = label.TextColor;
        var rotate = _random.Next(360);
        await Task.WhenAll(
          view.ScaleTo(1.5, half, Easing).WaitAsync(token),
          view.RotateTo(rotate, half, Easing).WaitAsync(token),
          label.TextColorTo(_colors[rotate % 7], 16, half).WaitAsync(token)
        );
        await Task.WhenAll(
          view.ScaleTo(1, half, Easing).WaitAsync(token),
          view.RotateTo(0, half, Easing).WaitAsync(token),
          label.TextColorTo(color, 16, half).WaitAsync(token)
        );
      }
    }
  }
}
