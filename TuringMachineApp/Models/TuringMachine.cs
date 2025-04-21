namespace TuringMachineApp.Models;

public class TuringMachine
{
    public Tape Tape { get; private set; }
    public string CurrentState { get; private set; }
    public bool IsHalted => CurrentState == "qf";

    private List<Transition> _transitions;

    public TuringMachine(string input, List<Transition> transitions)
    {
        Tape = new Tape(input);
        CurrentState = "q0";
        _transitions = transitions;
    }

    public void Step()
    {
        char currentSymbol = Tape.Read();
        var transition = _transitions.FirstOrDefault(t =>
            t.CurrentState == CurrentState && t.ReadSymbol == currentSymbol);

        if (transition == null)
        {
            CurrentState = "qf"; // Остановка, если нет перехода
            return;
        }

        Tape.Write(transition.WriteSymbol);
        Tape.Move(transition.Direction);
        CurrentState = transition.NextState;
    }
}

public class Transition
{
    public string CurrentState { get; set; }
    public char ReadSymbol { get; set; }
    public string NextState { get; set; }
    public char WriteSymbol { get; set; }
    public char Direction { get; set; } // 'L', 'R', 'S'
}