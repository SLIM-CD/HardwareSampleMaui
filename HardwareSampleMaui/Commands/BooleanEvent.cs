namespace HardwareSampleMaui.Commands;

public class BooleanEventArgs(bool value) : EventArgs
{
    public bool Value { get; } = value;
}
public delegate void BooleanEventHandler(object sender, BooleanEventArgs eventArgs);