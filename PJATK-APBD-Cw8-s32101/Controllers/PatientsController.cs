using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // [HttpPost("{pesel}/bedassignments")]
    // public async Task<IActionResult> AssignBed(string pesel, CancellationToken cancel = default)
    // {
    //     throw new NotImplementedException();
    // }
}