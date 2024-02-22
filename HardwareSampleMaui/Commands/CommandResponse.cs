using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

public class CommandResponseEventArgs(IResponse<object?> response) : EventArgs
{
    public IResponse<object?> Response { get; } = response;
}
public delegate void CommandResponseEventHandler(object sender, CommandResponseEventArgs eventArgs);