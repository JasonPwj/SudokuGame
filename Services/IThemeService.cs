namespace SudokuGame.Services
{
  public interface IThemeService
  {
    public Color FixedColor { get; }
    public Color CorrectColor { get; }
    public Color WrongColor { get; }
    public Color CollisionBack { get; }
    public Color SelectedBack { get; }
    public void SetTheme(AppTheme theme);
  }
}
