using System;
using System.Collections.Generic;

namespace cw3_apbd.Models_2
{
    public partial class TeamMember
    {
        public TeamMember()
        {
            TaskIdAssignedToNavigation = new HashSet<Task>();
            TaskIdCreatorNavigation = new HashSet<Task>();
        }

        public int IdTeamMember { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Task> TaskIdAssignedToNavigation { get; set; }
        public virtual ICollection<Task> TaskIdCreatorNavigation { get; set; }
    }
}
