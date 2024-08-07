﻿using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Notification 
    {
        public int NotificationId { get; set; }
        public int? SendId { get; set; }
        public string? SendUserName { get; set; }
        public string? ProjectName { get; set; }
        public int? RecieveId { get; set; }
        public string? Description { get; set; }
        public DateTime? Datetime { get; set; }
        public int? NotificationType { get; set; }
        public int? IsRead { get; set; }
        public string? Link { get; set; }
        public virtual AppUser? RecieveNavigation { get; set; }
        public virtual AppUser? SendNavigation { get; set; }
    }
}
