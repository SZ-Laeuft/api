using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Laufevent
{
    public class CreateUserVariablesFirstLastOrg
    {
        [Required] [DefaultValue("")] 
        public string firstname { get; set; }
        
        [Required][DefaultValue("")]   
         public string lastname { get; set; } 
        
        [Required][DefaultValue("")] 
         public string organisation { get; set; }
        

    }
}

