namespace MurderBot.Infrastructure.Routines;

public interface IServiceRoutine
{
    Task Execute();
    
    public bool CascadeFailure { get; }
}