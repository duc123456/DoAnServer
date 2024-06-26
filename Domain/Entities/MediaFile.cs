﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class MediaFile
    {
        public long Id { get; set; }


        public string FileName { get; set; }

        public string? Description { get; set; }

        public string? Title { get; set; }


        public int? FolderId { get; set; } // chuyển thành null able

        public int? UserId { get; set; } // null able

        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public AppUser User { get; set; }

        public virtual MediaFolder? MediaFolder { get; set; } = null!;
    }
}
