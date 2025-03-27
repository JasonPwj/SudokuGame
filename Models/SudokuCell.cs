using CommunityToolkit.Mvvm.ComponentModel;

namespace SudokuGame.Models;

public partial class SudokuCell : ObservableObject
{
  [ObservableProperty]
  private int _value = 0;

  [ObservableProperty]
  private bool _isFixed = false;

  [ObservableProperty]
  private string _text = "";

  [ObservableProperty]
  private Color _foreColor = Colors.Black;

  [ObservableProperty]
  private Color _backColor = Colors.Transparent;

  [ObservableProperty]
  private bool _isValid = false;

  [ObservableProperty]
  private Thickness _cellMargin = new Thickness(1, 1, 1, 1);

  public int Row { get; set; }
  public int Col { get; set; }

  partial void OnValueChanged(int value)
  {
    Text = value == 0 ? " " : $"{value}";
  }
}
