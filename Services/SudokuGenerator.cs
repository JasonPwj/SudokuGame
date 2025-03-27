using SudokuGame.Models;

namespace SudokuGame.Services;

public class SudokuGenerator
{
  private static Random _random = new Random();
  private int[,] _board = new int[9, 9];

  public int[,] GeneratePuzzle(int emptyCells = 40)
  {
    FillDiagonalBoxes();
    Solve(0, 0);
    RemoveNumbers(emptyCells);
    return _board;
  }

  private void FillDiagonalBoxes()
  {
    for (int i = 0; i < 9; i += 3)
    {
      FillBox(i, i);
    }
  }

  private void FillBox(int row, int col)
  {
    HashSet<int> usedNumbers = new HashSet<int>();

    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        int num;
        do
        {
          num = _random.Next(1, 10);
        } while (usedNumbers.Contains(num));

        usedNumbers.Add(num);
        _board[row + i, col + j] = num;
      }
    }
  }

  private bool Solve(int row, int col)
  {
    if (row == 9)
      return true;
    if (col == 9)
      return Solve(row + 1, 0);
    if (_board[row, col] != 0)
      return Solve(row, col + 1);

    List<int> numbers = Enumerable.Range(1, 9).OrderBy(n => _random.Next()).ToList();
    foreach (var num in numbers)
    {
      if (IsValid(row, col, num))
      {
        _board[row, col] = num;
        if (Solve(row, col + 1))
          return true;
        _board[row, col] = 0;
      }
    }
    return false;
  }

  private bool IsValid(int row, int col, int num)
  {
    for (int i = 0; i < 9; i++)
    {
      if (_board[row, i] == num || _board[i, col] == num)
      {
        return false;
      }
    }
    int boxRow = (row / 3) * 3,
      boxCol = (col / 3) * 3;
    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        if (_board[boxRow + i, boxCol + j] == num)
        {
          return false;
        }
      }
    }
    return true;
  }

  private void RemoveNumbers(int emptyCells)
  {
    int count = emptyCells;
    while (count > 0)
    {
      int row = _random.Next(9);
      int col = _random.Next(9);
      if (_board[row, col] != 0)
      {
        _board[row, col] = 0;
        count--;
      }
    }
  }

  public List<SudokuCell> GenerateBoard(DifficultyLevel difficulty = DifficultyLevel.Easy)
  {
    //int[,] board = new int[9, 9];
    //FillBoard(board);

    int clues = difficulty switch
    {
      DifficultyLevel.Easy => 40,
      DifficultyLevel.Medium => 30,
      DifficultyLevel.Hard => 20,
      _ => 30
    };

    _board = new int[9, 9];
    _board = GeneratePuzzle(60 - clues);

    return ConvertToSudokuCells(_board, 81);
  }

  private void FillBoard(int[,] board)
  {
    for (int i = 0; i < 9; i++)
    {
      for (int j = 0; j < 9; j++)
      {
        board[i, j] = (i * 3 + i / 3 + j) % 9 + 1;
      }
    }

    for (int i = 0; i < 9; i++)
    {
      int swapIndex = _random.Next(0, 9);
      for (int j = 0; j < 9; j++)
      {
        (board[i, j], board[swapIndex, j]) = (board[swapIndex, j], board[i, j]);
      }
    }
  }

  private List<SudokuCell> ConvertToSudokuCells(int[,] board, int clues)
  {
    List<SudokuCell> cells = new List<SudokuCell>();
    int removedCells = 81 - clues;

    for (int i = 0; i < 9; i++)
    {
      for (int j = 0; j < 9; j++)
      {
        cells.Add(
          new SudokuCell
          {
            Value = board[i, j],
            IsFixed = board[i, j] != 0,
            Row = i,
            Col = j,
            CellMargin = (i, j) switch
            {
              (i: var x, j: var y) when x % 3 == 2 && y % 3 == 2 => new Thickness(1, 1, 2, 2),
              (i: var x, j: var y) when x % 3 == 0 && y % 3 == 2 => new Thickness(1, 2, 2, 1),
              (i: var x, j: var y) when x % 3 == 0 && y % 3 == 0 => new Thickness(2, 2, 1, 1),
              (i: var x, j: var y) when x % 3 == 2 && y % 3 == 0 => new Thickness(2, 1, 1, 2),
              (i: var x, j: var y) when x % 3 == 2 => new Thickness(1, 1, 1, 2),
              (i: var x, j: var y) when x % 3 == 0 => new Thickness(1, 2, 1, 1),
              (i: var x, j: var y) when y % 3 == 0 => new Thickness(2, 1, 1, 1),
              (i: var x, j: var y) when y % 3 == 2 => new Thickness(1, 1, 2, 1),
              _ => new Thickness(1),
            }
          }
        );
      }
    }

    while (removedCells > 0)
    {
      int index = _random.Next(0, 81);
      if (cells[index].Value != 0)
      {
        cells[index].Value = 0;
        cells[index].IsFixed = false;
        removedCells--;
      }
    }

    return cells;
  }
}

public enum DifficultyLevel
{
  Easy,
  Medium,
  Hard
}
