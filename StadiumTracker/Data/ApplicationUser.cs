using Microsoft.AspNetCore.Identity;
using StadiumTracker.Data.Models;

namespace StadiumTracker.Data;

public class ApplicationUser : IdentityUser
{
    public ICollection<StadiumVisit> StadiumVisits { get; set; } = [];
}

