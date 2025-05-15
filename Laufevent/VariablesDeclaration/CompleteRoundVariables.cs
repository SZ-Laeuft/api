using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Laufevent;
public class CompleteRoundVariables
{
    [Required] 
    [DefaultValue(0.0)]  
    public decimal uid { get; set; }
}
