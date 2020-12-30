using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class ConnectedUsers
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
