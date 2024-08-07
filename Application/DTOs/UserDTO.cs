﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Qualification>? Qualifications { get; set; }
        public List<Experience>? Experiences { get; set; }
        public List<Education>? Educations { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsCompany { get; set; }
        public string? Description { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public bool? IsPaid { get; set; }
        public int AmountBid { get; set; }

        public int AmoutProject { get; set; }

        public int TotalProject { get; set; }
        public int  TotalBid { get; set; }

        public decimal? AvgRate { get; set; }
        public int? TotalRate { get; set; }

        public DateTime? LockoutEnd { get; set; }

        public bool? LockoutEnabled { get; set; }

        public bool? IsLock { get; set; }
        
        public string Role { get; set; }
        public string? PhoneNumber { get; set; }
        public List<MediaFileDTO> mediaFiles { get; set; }
        public List<RatingDTO>? ratings { get; set; }

        public List<string>? skills { get; set; }

        public bool? IsRated { get; set; }

        public AddressDTO Address { get; set; }

    }
}
