namespace OrderFlow.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public sealed class OrderNotFoundException : DomainException
{
    public OrderNotFoundException(Guid orderId)
        : base($"Order with ID '{orderId}' was not found.") { }
}

public sealed class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(string currentState, string attemptedAction)
        : base($"Cannot {attemptedAction} an order in '{currentState}' state.") { }
}
