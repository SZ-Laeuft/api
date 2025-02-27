using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Laufevent;
public class UpdateUserModel
{
    [Required][DefaultValue("")]
    public string? FirstName { get; set; }
    [Required][DefaultValue("")]
    public string? LastName { get; set; }
    [DefaultValue("")]
    public double? uid { get; set; }
    [DefaultValue("")]
    public string? SchoolClass { get; set; }
    [DefaultValue("")]
    public string? Organisation { get; set; }
}