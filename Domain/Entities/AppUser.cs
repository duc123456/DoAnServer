﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AppUser : IdentityUser<int>    
    {
        public AppUser()
        {
            Bids = new List<Bid>();
            Bookmarks = new List<Bookmark>();
            UserProjects = new List<UserProject>();
            UserSkills = new List<UserSkill>();
            MediaFiles = new List<MediaFile>();
            UserReports = new List<UserReport>();
            Blogs = new List<Blog>();
            RecieveNavigations = new List<Notification>();
            SendNavigations = new List<Notification>();
            User1Navigations = new List<Conversation>();
            FavoriteProjects = new List<FavoriteProject>();
            User2Navigations = new List<Conversation>();
            Senders = new List<Message>();
        }
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public virtual ICollection<Bid> Bids { get; set; }
        public virtual ICollection<Bookmark> Bookmarks { get; set; }
        public virtual ICollection<UserProject> UserProjects { get; set; }
        public virtual ICollection<UserSkill> UserSkills { get; set; }
        public virtual ICollection<MediaFile> MediaFiles  { get; set; }
        public virtual ICollection<UserReport> UserReports  { get; set; }
        public virtual ICollection<Blog> Blogs  { get; set; }
        public virtual ICollection<Notification> RecieveNavigations { get; set; }
        public virtual ICollection<Notification> SendNavigations { get; set; }
        [JsonIgnore]
        public virtual ICollection<Conversation> User1Navigations { get; set; }
        [JsonIgnore]
        public virtual ICollection<FavoriteProject> FavoriteProjects { get; set; }
        public virtual ICollection<Conversation> User2Navigations { get; set; }
        public virtual ICollection<Message> Senders { get; set; }

        public virtual Address? Address { get; set; } = null!;

        public string? Description { get; set; }
        
        public string? TaxCode { get; set; }

        public bool IsCompany { get; set; }

        public int AmountBid { get; set; }
        public bool IsPaid { get; set; }
        public int AmoutProject { get; set; }

        public string? Education { get; set; }
        public string? Experience { get; set; }
        public string? Qualifications { get; set; }
        public string? Avatar { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
    }
}
