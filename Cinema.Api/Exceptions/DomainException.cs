namespace Cinema.Api.Exceptions;

public class DomainException : Exception
{
    public int StatusCode { get; }

    public DomainException(string message, int statusCode = 409) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class SeatAlreadyOccupiedException : DomainException
{
    public SeatAlreadyOccupiedException(int sessionId, int seatId)
        : base($"Assento {seatId} já está ocupado na sessão {sessionId}.", 409) { }
}

public class SessionNotFoundException : DomainException
{
    public SessionNotFoundException(int sessionId)
        : base($"Sessão {sessionId} não encontrada.", 404) { }
}

public class SeatNotFoundException : DomainException
{
    public SeatNotFoundException(int seatId)
        : base($"Assento {seatId} não encontrado.", 404) { }
}

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Email ou senha inválidos.", 401) { }
}

public class MovieNotFoundException : DomainException
{
    public MovieNotFoundException(int movieId)
        : base($"Filme {movieId} não encontrado.", 404) { }
}

public class RoomNotFoundException : DomainException
{
    public RoomNotFoundException(int roomId)
        : base($"Sala {roomId} não encontrada.", 404) { }
}
