using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TuringMachineApp.Models;


namespace TuringMachineApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private TuringMachine machine;
    private List<Transition> currentTransitions;

    private string _input;
    public string Input
    {
        get => _input;
        set { _input = value; OnPropertyChanged(); }
    }

    private string _currentState = "Выберите режим и введите выражение";
    public string CurrentState
    {
        get => _currentState;
        set { _currentState = value; OnPropertyChanged(); }
    }

    private string _currentMode;
    public string CurrentMode
    {
        get => _currentMode;
        set { _currentMode = value; OnPropertyChanged(); }
    }

    public ObservableCollection<TapeCell> Tape { get; } = new();

    public int HeadPosition { get; set; }

    public ICommand StartCommand { get; }
    public ICommand StepCommand { get; }

    public MainViewModel()
    {
        StartCommand = new Command(ResetMachine);
        StepCommand = new Command(Step);
    }

    public void ResetMachine()
    {
        if (string.IsNullOrWhiteSpace(Input))
        {
            CurrentState = "Введите выражение";
            return;
        }

        try
        {
            currentTransitions = CurrentMode switch
            {
                "Унарное сложение (+)" => GetUnaryAdditionRules(),
                "Унарное умножение (*)" => GetUnaryMultiplicationRules(),
                "Бинарное сложение" => GetBinaryAdditionRules(),
                _ => throw new InvalidOperationException("Режим не выбран")
            };

            machine = new TuringMachine(Input, currentTransitions);
            UpdateUI();
        }
        catch (Exception ex)
        {
            CurrentState = $"Ошибка: {ex.Message}";
            Tape.Clear();
        }
    }

    private void Step()
    {
        if (machine == null || machine.IsHalted) return;

        machine.Step();
        UpdateUI();
    }

    private void UpdateUI()
    {
        Tape.Clear();

        var cells = machine.Tape.GetCells().ToArray(); // Преобразование IEnumerable<char> в char[]

        for (int i = 0; i < cells.Length; i++)
        {
            Tape.Add(new TapeCell
            {
                Symbol = cells[i],
                Index = i
            });
        }

        HeadPosition = machine.Tape.HeadPosition;
        CurrentState = $"Состояние: {machine.CurrentState}";
        OnPropertyChanged(nameof(HeadPosition));
        OnPropertyChanged(nameof(Tape));
    }


    private List<Transition> GetUnaryAdditionRules()
    {
        if (!Input.Contains('+'))
            throw new ArgumentException("Используйте + для сложения (например: 111+11)");

        return new List<Transition>
        {
            new() { CurrentState = "q0", ReadSymbol = '1', NextState = "q0", WriteSymbol = '1', Direction = 'R' },
            new() { CurrentState = "q0", ReadSymbol = '+', NextState = "q1", WriteSymbol = '1', Direction = 'R' },
            new() { CurrentState = "q1", ReadSymbol = '1', NextState = "q1", WriteSymbol = '1', Direction = 'R' },
            new() { CurrentState = "q1", ReadSymbol = '_', NextState = "q2", WriteSymbol = '_', Direction = 'L' },
            new() { CurrentState = "q2", ReadSymbol = '1', NextState = "qf", WriteSymbol = '_', Direction = 'S' }
        };
    }

    private List<Transition> GetUnaryMultiplicationRules()
    {
        if (!Input.Contains('*'))
            throw new ArgumentException("Используйте * для умножения (например: 11*111)");

        return new List<Transition>
{
    // q0: Идем вправо, пока не найдем '*'
    new() { CurrentState = "q0", ReadSymbol = '1', NextState = "q0", WriteSymbol = '1', Direction = 'R' },
    new() { CurrentState = "q0", ReadSymbol = '*', NextState = "q1", WriteSymbol = '*', Direction = 'R' },

    // q1: Ищем первую 1 во втором числе, помечаем как X
    new() { CurrentState = "q1", ReadSymbol = '1', NextState = "q2", WriteSymbol = 'X', Direction = 'L' },
    new() { CurrentState = "q1", ReadSymbol = '*', NextState = "q1", WriteSymbol = '*', Direction = 'R' },
    new() { CurrentState = "q1", ReadSymbol = 'X', NextState = "q1", WriteSymbol = 'X', Direction = 'R' },
    new() { CurrentState = "q1", ReadSymbol = '_', NextState = "q9", WriteSymbol = '_', Direction = 'L' },

    // q2: Идем влево к правой 1 левого числа
    new() { CurrentState = "q2", ReadSymbol = '*', NextState = "q2", WriteSymbol = '*', Direction = 'L' },
    new() { CurrentState = "q2", ReadSymbol = '1', NextState = "q3", WriteSymbol = 'X', Direction = 'R' },
    new() { CurrentState = "q2", ReadSymbol = 'X', NextState = "q2", WriteSymbol = 'X', Direction = 'L' },
    new() { CurrentState = "q2", ReadSymbol = '_', NextState = "q6", WriteSymbol = '_', Direction = 'R' },

    // q3: Идем вправо до первого пробела
    new() { CurrentState = "q3", ReadSymbol = '*', NextState = "q3", WriteSymbol = '*', Direction = 'R' },
    new() { CurrentState = "q3", ReadSymbol = 'X', NextState = "q3", WriteSymbol = 'X', Direction = 'R' },
    new() { CurrentState = "q3", ReadSymbol = '1', NextState = "q3", WriteSymbol = '1', Direction = 'R' },
    new() { CurrentState = "q3", ReadSymbol = '_', NextState = "q4", WriteSymbol = '_', Direction = 'R' },

    // q4: Копируем 1 в конец
    new() { CurrentState = "q4", ReadSymbol = '_', NextState = "q5", WriteSymbol = '1', Direction = 'L' },
    new() { CurrentState = "q4", ReadSymbol = '1', NextState = "q4", WriteSymbol = '1', Direction = 'R' },

    // q5: Возвращаемся к правому числу
    new() { CurrentState = "q5", ReadSymbol = '_', NextState = "q5", WriteSymbol = '_', Direction = 'L' },
    new() { CurrentState = "q5", ReadSymbol = '1', NextState = "q5", WriteSymbol = '1', Direction = 'L' },
    new() { CurrentState = "q5", ReadSymbol = 'X', NextState = "q2", WriteSymbol = 'X', Direction = 'L' },

    // q6: Восстанавливаем левое число
    new() { CurrentState = "q6", ReadSymbol = 'X', NextState = "q6", WriteSymbol = '1', Direction = 'R' },
    new() { CurrentState = "q6", ReadSymbol = '*', NextState = "q1", WriteSymbol = '*', Direction = 'R' },

    // q9: Очищаем * и X
    new() { CurrentState = "q9", ReadSymbol = 'X', NextState = "q9", WriteSymbol = '_', Direction = 'L' },
    new() { CurrentState = "q9", ReadSymbol = '*', NextState = "q9", WriteSymbol = '_', Direction = 'L' },
    new() { CurrentState = "q9", ReadSymbol = '1', NextState = "q9", WriteSymbol = '_', Direction = 'L' },
    new() { CurrentState = "q9", ReadSymbol = '_', NextState = "qf", WriteSymbol = '_', Direction = 'S' },
};



    }

    private List<Transition> GetBinaryAdditionRules()
    {
        if (!Input.Contains('+'))
            throw new ArgumentException("Используйте + для сложения (например: 101+110)");

        return new List<Transition>
        {
            // Основной цикл (без изменений)
            new() { CurrentState = "q0", ReadSymbol = '0', NextState = "q0", WriteSymbol = '0', Direction = 'R' },
            new() { CurrentState = "q0", ReadSymbol = '1', NextState = "q0", WriteSymbol = '1', Direction = 'R' },
            new() { CurrentState = "q0", ReadSymbol = '+', NextState = "q1", WriteSymbol = '+', Direction = 'R' },

            new() { CurrentState = "q1", ReadSymbol = '0', NextState = "q1", WriteSymbol = '0', Direction = 'R' },
            new() { CurrentState = "q1", ReadSymbol = '1', NextState = "q1", WriteSymbol = '1', Direction = 'R' },
            new() { CurrentState = "q1", ReadSymbol = '_', NextState = "q2", WriteSymbol = '_', Direction = 'L' },

            // Вычитание 1 из второго числа (без изменений)
            new() { CurrentState = "q2", ReadSymbol = '1', NextState = "q3", WriteSymbol = '0', Direction = 'L' },
            new() { CurrentState = "q2", ReadSymbol = '0', NextState = "q2", WriteSymbol = '1', Direction = 'L' },
    
            // Критическое изменение здесь!
            new() { CurrentState = "q2", ReadSymbol = '+', NextState = "q6", WriteSymbol = '+', Direction = 'S' },

            // Новый механизм проверки нулей
            // q6: проверяем первый символ после +
            new() { CurrentState = "q6", ReadSymbol = '0', NextState = "q6_scan", WriteSymbol = '0', Direction = 'R' },
            new() { CurrentState = "q6", ReadSymbol = '_', NextState = "q_clean", WriteSymbol = '_', Direction = 'L' }, // Число нулевое
            new() { CurrentState = "q6", ReadSymbol = '+', NextState = "q_clean", WriteSymbol = '_', Direction = 'R' },

            // q_clean: очистка нулей и +
            new() { CurrentState = "q_clean", ReadSymbol = '0', NextState = "q_clean", WriteSymbol = '_', Direction = 'R' },
            new() { CurrentState = "q_clean", ReadSymbol = '1', NextState = "q_clean", WriteSymbol = '_', Direction = 'R' },
            new() { CurrentState = "q_clean", ReadSymbol = '_', NextState = "qf", WriteSymbol = '_', Direction = 'S' },

            // Остальные состояния (без изменений)
            new() { CurrentState = "q3", ReadSymbol = '0', NextState = "q3", WriteSymbol = '0', Direction = 'L' },
            new() { CurrentState = "q3", ReadSymbol = '1', NextState = "q3", WriteSymbol = '1', Direction = 'L' },
            new() { CurrentState = "q3", ReadSymbol = '+', NextState = "q4", WriteSymbol = '+', Direction = 'L' },

            new() { CurrentState = "q4", ReadSymbol = '0', NextState = "q0", WriteSymbol = '1', Direction = 'R' },
            new() { CurrentState = "q4", ReadSymbol = '1', NextState = "q4", WriteSymbol = '0', Direction = 'L' },
            new() { CurrentState = "q4", ReadSymbol = '_', NextState = "q0", WriteSymbol = '1', Direction = 'R' }
        };
    }
}

// 111*11x