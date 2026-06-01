using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJATK_APBD_Cw8_s32101.DTO;
using PJATK_APBD_Cw8_s32101.Models;

namespace PJATK_APBD_Cw8_s32101.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController(HospitalContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPatients(string? search = null, CancellationToken cancel = default)
    {
        var patients = db.Patients.AsQueryable();

        if (search != null)
            patients = patients.Where(p => p.FirstName.Contains(search) || p.LastName.Contains(search));

        var result = await patients
            .Include(p => p.Admissions)
                .ThenInclude(ad => ad.Ward)
            .Include(p => p.BedAssignments)
            .Select(p => new
            {
                p.Pesel,
                p.FirstName,
                p.LastName,
                p.Age,
                p.Sex,
                Admissions = p.Admissions.Select(ad => new
                {
                    ad.Id,
                    ad.AdmissionDate,
                    ad.DischargeDate,
                    Ward = new
                    {
                        Id = ad.WardId,
                        ad.Ward.Name,
                        ad.Ward.Description
                    }
                }),
                BedAssignments = p.BedAssignments.Select(ass => new
                {
                    ass.Id,
                    ass.From,
                    ass.To,
                    Bed = new
                    {
                        ass.Bed.Id,
                        BedType = new
                        {
                            ass.Bed.BedType.Id,
                            ass.Bed.BedType.Name,
                            ass.Bed.BedType.Description
                        },
                        Room = new
                        {
                            ass.Bed.Room.Id,
                            ass.Bed.Room.HasTv,
                            Ward = new
                            {
                                Id = ass.Bed.Room.Ward,
                                ass.Bed.Room.Ward.Name,
                                ass.Bed.Room.Ward.Description
                            }
                        }
                    }
                })
            })
            .ToListAsync(cancellationToken: cancel);
        
        return Ok(result);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed([FromRoute] string pesel, [FromBody] BedAssignmentsRequestDTO input,
        CancellationToken cancel = default)
    {
        await using var tran = await db.Database.BeginTransactionAsync(cancel);
        
        var patient = await db.Patients.FirstOrDefaultAsync(p => p.Pesel == pesel, cancel);
        if (patient == null)
            return NotFound("Pacjent nie istnieje");
        
        var availableBed = await db.Beds.Include(b => b.Room)
            .ThenInclude(r => r.Ward)
            .Include(b => b.BedType)
            .Include(b => b.BedAssignments)
            .Where(b => b.BedType.Name == input.BedType && b.Room.Ward.Name == input.Ward &&
                        !b.BedAssignments.Any(ass =>
                            // istniejące przypisanie bez końca -> zawsze blokuje
                            ass.To == null ||

                            // nowe przypisanie bez końca
                            (input.To == null
                                ? ass.To > input.From

                                // oba mają zakres
                                : ass.From < input.To.Value &&
                                  ass.To > input.From)))
            .FirstOrDefaultAsync(cancel);

        if (availableBed == null)
            return NotFound("Łóżko nie jest dostępne");
        
        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = input.From,
            To = input.To
        };

        db.BedAssignments.Add(assignment);

        await db.SaveChangesAsync(cancel);
        await tran.CommitAsync(cancel);

        return NoContent();
    }
}