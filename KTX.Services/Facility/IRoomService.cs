using KTX.Domain.DTOs;

namespace KTX.Services.Facility;

public interface IRoomService
{
    Task<List<BuildingDto>> GetBuildingsAsync(CancellationToken ct = default);
    Task<List<RoomTypeDto>> GetRoomTypesAsync(CancellationToken ct = default);
    Task<List<RoomSummaryDto>> GetAllRoomsAsync(CancellationToken ct = default);
    Task<List<RoomSummaryDto>> GetRoomsByBuildingAndFloorAsync(int buildingId, int floor, CancellationToken ct = default);
    Task<RoomDetailDto?> GetRoomDetailAsync(int roomId, CancellationToken ct = default);
    Task<bool> UpdateRoomStatusAsync(int roomId, bool isUnderMaintenance, CancellationToken ct = default);
}
