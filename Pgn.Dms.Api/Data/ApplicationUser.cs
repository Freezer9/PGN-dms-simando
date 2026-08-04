using Microsoft.AspNetCore.Identity;

namespace Pgn.Dms.Api.Data;

public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }
    public int? AreaId { get; set; }
    public Area? Area { get; set; }
}
