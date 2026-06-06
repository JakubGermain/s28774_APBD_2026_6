using System.Data;
using HospitalApi.Data;
using HospitalApi.Dtos;
using HospitalApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Controllers;

[ApiController]
[Route("api/patients")]
public sealed class PatientsController : ControllerBase
{
    private readonly HospitalContext _context;

    public PatientsController(HospitalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PatientResponse>>> GetPatients([FromQuery] string? search)
    {
        var query = _context.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(patient =>
                EF.Functions.Like(patient.FirstName, pattern) ||
                EF.Functions.Like(patient.LastName, pattern));
        }

        var patients = await query
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .Select(patient => new PatientResponse(
                patient.Pesel,
                patient.FirstName,
                patient.LastName,
                patient.Age,
                patient.Sex ? "Male" : "Female",
                patient.Admissions
                    .OrderBy(admission => admission.Id)
                    .Select(admission => new AdmissionResponse(
                        admission.Id,
                        admission.AdmissionDate,
                        admission.DischargeDate,
                        new WardResponse(
                            admission.Ward.Id,
                            admission.Ward.Name,
                            admission.Ward.Description)))
                    .ToList(),
                patient.BedAssignments
                    .OrderBy(assignment => assignment.Id)
                    .Select(assignment => new BedAssignmentResponse(
                        assignment.Id,
                        assignment.From,
                        assignment.To,
                        new BedResponse(
                            assignment.Bed.Id,
                            new BedTypeResponse(
                                assignment.Bed.BedType.Id,
                                assignment.Bed.BedType.Name,
                                assignment.Bed.BedType.Description),
                            new RoomResponse(
                                assignment.Bed.Room.Id,
                                assignment.Bed.Room.HasTv,
                                new WardResponse(
                                    assignment.Bed.Room.Ward.Id,
                                    assignment.Bed.Room.Ward.Name,
                                    assignment.Bed.Room.Ward.Description)))))
                    .ToList()))
            .ToListAsync();

        return Ok(patients);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<ActionResult<CreatedBedAssignmentResponse>> CreateBedAssignment(
        string pesel,
        [FromBody] CreateBedAssignmentRequest request)
    {
        if (request.From == default)
        {
            return BadRequest(new ErrorResponse("Field 'from' is required."));
        }

        if (request.To.HasValue && request.To.Value <= request.From)
        {
            return BadRequest(new ErrorResponse("Field 'to' must be later than field 'from'."));
        }

        var bedTypeName = request.BedType?.Trim();
        if (string.IsNullOrWhiteSpace(bedTypeName))
        {
            return BadRequest(new ErrorResponse("Field 'bedType' is required."));
        }

        var wardName = request.Ward?.Trim();
        if (string.IsNullOrWhiteSpace(wardName))
        {
            return BadRequest(new ErrorResponse("Field 'ward' is required."));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var patientExists = await _context.Patients.AnyAsync(patient => patient.Pesel == pesel);
        if (!patientExists)
        {
            return NotFound(new ErrorResponse($"Patient with PESEL '{pesel}' was not found."));
        }

        var bedTypeExists = await _context.BedTypes.AnyAsync(bedType => bedType.Name == bedTypeName);
        if (!bedTypeExists)
        {
            return NotFound(new ErrorResponse($"Bed type '{bedTypeName}' was not found."));
        }

        var wardExists = await _context.Wards.AnyAsync(ward => ward.Name == wardName);
        if (!wardExists)
        {
            return NotFound(new ErrorResponse($"Ward '{wardName}' was not found."));
        }

        var availableBeds = _context.Beds
            .Include(bed => bed.BedType)
            .Include(bed => bed.Room)
                .ThenInclude(room => room.Ward)
            .Where(bed => bed.BedType.Name == bedTypeName && bed.Room.Ward.Name == wardName);

        availableBeds = request.To.HasValue
            ? availableBeds.Where(bed => !bed.BedAssignments.Any(assignment =>
                assignment.From < request.To.Value &&
                (assignment.To == null || assignment.To > request.From)))
            : availableBeds.Where(bed => !bed.BedAssignments.Any(assignment =>
                assignment.To == null || assignment.To > request.From));

        var selectedBed = await availableBeds
            .OrderBy(bed => bed.Id)
            .FirstOrDefaultAsync();

        if (selectedBed is null)
        {
            return NotFound(new ErrorResponse(
                $"No available '{bedTypeName}' bed was found in ward '{wardName}' for the requested period."));
        }

        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = selectedBed.Id,
            From = request.From,
            To = request.To
        };

        _context.BedAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var response = new CreatedBedAssignmentResponse(
            assignment.Id,
            assignment.PatientPesel,
            assignment.From,
            assignment.To,
            MapBed(selectedBed));

        return Created($"/api/patients/{pesel}/bedassignments/{assignment.Id}", response);
    }

    private static BedResponse MapBed(Bed bed)
    {
        return new BedResponse(
            bed.Id,
            new BedTypeResponse(
                bed.BedType.Id,
                bed.BedType.Name,
                bed.BedType.Description),
            new RoomResponse(
                bed.Room.Id,
                bed.Room.HasTv,
                new WardResponse(
                    bed.Room.Ward.Id,
                    bed.Room.Ward.Name,
                    bed.Room.Ward.Description)));
    }
}
