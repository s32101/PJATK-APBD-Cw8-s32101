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

        return Ok(await patients.ToListAsync(cancellationToken: cancel));
    }
}