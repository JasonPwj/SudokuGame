namespace SudokuGame.Services
{
  public class ThemeService : IThemeService
  {
    public Color FixedColor { get; set; } = Colors.Black;
    public Color CorrectColor { get; set; } = Colors.DeepSkyBlue;
    public Color WrongColor { get; set; } = Colors.Red;
    public Color CollisionBack { get; set; } = Colors.AntiqueWhite;
    public Color SelectedBack { get; set; } = Colors.LightYellow;

    public void SetTheme(AppTheme theme)
    {
      switch (theme)
      {
        default:
        case AppTheme.Light:
          FixedColor = Colors.Black;
          CorrectColor = Colors.DeepSkyBlue;
          WrongColor = Colors.Red;
          CollisionBack = Colors.AntiqueWhite;
          SelectedBack = Colors.LightYellow;
          break;
        case AppTheme.Dark:
          FixedColor = Colors.White;
          CorrectColor = Colors.Lime;
          WrongColor = Colors.Red;
          CollisionBack = Colors.DarkSlateGray;
          SelectedBack = Colors.DarkKhaki;
          break;
      }
    }
  }
}
