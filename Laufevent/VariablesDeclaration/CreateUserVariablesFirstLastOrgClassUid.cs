using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Laufevent;

public class CreateUserVariablesFirstLastOrgClassUid
{
    [Required] [DefaultValue("")] 
    public string firstname { get; set; }
        
    [Required][DefaultValue("")]   
    public string lastname { get; set; } 
        
    [Required][DefaultValue("")] 
    public string organisation { get; set; }
    
    [Required][DefaultValue("")] 
    public string school_class { get; set; }
    
    [Required][DefaultValue("")] 
    public double uid { get; set; }
}
