using System.ComponentModel.DataAnnotations;

namespace HospitalApi.Dtos;

public sealed record PatientResponse(
    string Pesel,
    string FirstName,
    string LastName,
    int Age,
    string Sex,
    IReadOnlyList<AdmissionResponse> Admissions,
    IReadOnlyList<BedAssignmentResponse> BedAssignments);

public sealed record AdmissionResponse(
    int Id,
    DateTime AdmissionDate,
    DateTime? DischargeDate,
    WardResponse Ward);

public sealed record BedAssignmentResponse(
    int Id,
    DateTime From,
    DateTime? To,
    BedResponse Bed);

public sealed record BedResponse(
    int Id,
    BedTypeResponse BedType,
    RoomResponse Room);

public sealed record BedTypeResponse(
    int Id,
    string Name,
    string Description);

public sealed record RoomResponse(
    string Id,
    bool HasTv,
    WardResponse Ward);

public sealed record WardResponse(
    int Id,
    string Name,
    string Description);

public sealed class CreateBedAssignmentRequest
{
    public DateTime From { get; init; }

    public DateTime? To { get; init; }

    [Required]
    public string BedType { get; init; } = null!;

    [Required]
    public string Ward { get; init; } = null!;
}

public sealed record CreatedBedAssignmentResponse(
    int Id,
    string PatientPesel,
    DateTime From,
    DateTime? To,
    BedResponse Bed);

public sealed record ErrorResponse(string Message);
