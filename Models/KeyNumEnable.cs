using CommunityToolkit.Mvvm.ComponentModel;

namespace SudokuGame.Models
{
  public partial class KeyNumEnable : ObservableObject
  {
    [ObservableProperty]
    private bool _IsEnabled = false;
  }
}
