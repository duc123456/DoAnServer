﻿using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configurations
{
    public class SkillConfiguration : IEntityTypeConfiguration<Skill>
    {
        public void Configure(EntityTypeBuilder<Skill> builder)
        {
            builder.ToTable("Skills");
            builder.HasKey(c => c.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(t => t.SkillName)
                .HasMaxLength(1000)
                .IsRequired();

            //relation
            builder.HasOne(x => x.Category).WithMany(x => x.Skills).HasForeignKey(x => x.CategoryId);

            builder.HasMany(s => s.ProjectSkills)
            .WithOne(ps => ps.Skill)
            .HasForeignKey(ps => ps.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
