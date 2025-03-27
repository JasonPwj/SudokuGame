using SudokuGame.Services;

namespace SudokuGame.Models
{
  public class DiffItem
  {
    public string Name { get; set; } = "Easy";
    public DifficultyLevel Diff { get; set; }
  }
}
