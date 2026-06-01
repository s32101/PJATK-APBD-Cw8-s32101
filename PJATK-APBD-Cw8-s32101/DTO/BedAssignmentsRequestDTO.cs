using System.ComponentModel.DataAnnotations;

namespace PJATK_APBD_Cw8_s32101.DTO;

public class BedAssignmentsRequestDTO
{
    [Required]
    public DateTime From { get; set; }

    public DateTime? To { get; set; }

    [Required] public string BedType { get; set; } = null!;

    [Required] public string Ward { get; set; } = null!;
}