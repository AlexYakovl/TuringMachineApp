namespace TuringMachineApp.Models;

public class TapeCell
{
    public char Symbol { get; set; }
    public int Index { get; set; }
}


public class Tape
{
    private List<char> _cells;
    public int HeadPosition { get; private set; }

    public Tape(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be empty");

        _cells = input.ToList();
        HeadPosition = 0;
    }

    public char Read()
    {
        if (HeadPosition < 0 || HeadPosition >= _cells.Count)
            return '_';
        return _cells[HeadPosition];
    }

    public void Write(char symbol)
    {
        if (HeadPosition < 0 || HeadPosition >= _cells.Count)
            return;
        _cells[HeadPosition] = symbol;
    }

    public void Move(char direction)
    {
        switch (direction)
        {
            case 'R':
                HeadPosition++;
                if (HeadPosition >= _cells.Count)
                    _cells.Add('_');
                break;

            case 'L':
                HeadPosition--;
                if (HeadPosition < 0)
                {
                    // Добавляем новую ячейку в начало
                    _cells.Insert(0, '_');
                    HeadPosition = 0; // Сбрасываем позицию на новый начальный элемент
                }
                break;

            case 'S':
                // Ничего не делаем для остановки
                break;

            default:
                throw new ArgumentException($"Invalid direction: {direction}");
        }
    }

    public IEnumerable<char> GetCells() => _cells;
}