using Cinema.Api.Data;
using Cinema.Api.DTOs;
using Cinema.Api.Exceptions;
using Cinema.Api.Models;
using Cinema.Api.Repositories;

namespace Cinema.Api.Services;

public interface IRoomService
{
    Task<RoomDto> CreateRoomAsync(CreateRoomRequest request);
    Task<List<RoomDto>> GetAllRoomsAsync();
    Task SoftDeleteRoomAsync(int roomId);
    Task RestoreRoomAsync(int roomId);
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _repo;
    private readonly IUnitOfWork _uow;

    public RoomService(IRoomRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request)
    {
        var room = new Room
        {
            Name = request.Name,
            Rows = request.Rows,
            Columns = request.Columns
        };

        using var transaction = await _uow.BeginTransactionAsync();

        try
        {
            await _repo.AddAsync(room);

            string[] rowLabels = Enumerable.Range(0, request.Rows)
                .Select(i => ((char)('A' + i)).ToString())
                .ToArray();

            var seats = new List<Seat>();
            foreach (var row in rowLabels)
                for (int col = 1; col <= request.Columns; col++)
                    seats.Add(new Seat { RoomId = room.Id, Row = row, Number = col });

            await _repo.AddSeatsAsync(seats);
            await transaction.CommitAsync();

            return new RoomDto(room.Id, room.Name, room.Rows, room.Columns, room.IsDeleted);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await _repo.GetAllAsync(includeDeleted: true);
        return rooms.Select(r => new RoomDto(r.Id, r.Name, r.Rows, r.Columns, r.IsDeleted)).ToList();
    }

    public async Task SoftDeleteRoomAsync(int roomId)
    {
        var room = await _repo.GetByIdAsync(roomId)
            ?? throw new RoomNotFoundException(roomId);

        if (room.IsDeleted)
            throw new DomainException("Esta sala já está desativada.", 400);

        room.IsDeleted = true;
        await _repo.UpdateAsync(room);
    }

    public async Task RestoreRoomAsync(int roomId)
    {
        var room = await _repo.GetByIdAsync(roomId, includeDeleted: true)
            ?? throw new RoomNotFoundException(roomId);

        if (!room.IsDeleted)
            throw new DomainException("Esta sala já está ativa.", 400);

        room.IsDeleted = false;
        await _repo.UpdateAsync(room);
    }
}
