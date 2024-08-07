﻿using Domain.Entities;
using Domain.Enums;
using Domain.IRepositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SkillRepository : GenericRepository<Skill>, ISkillRepository
    {
        private readonly ApplicationDbContext _context;
        public SkillRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Skill> GetByNameAsync(string skillName)
        {
            var skill = await  _context.Skills.FirstOrDefaultAsync(x=>x.SkillName.ToLower().Equals(skillName));
            return skill;
        }

        public async Task<List<Skill>> GetsByNameAsync(string skillName)
        {
            var Skills = await _context.Skills.Where(x => x.SkillName.ToLower().Contains(skillName.ToLower())).ToListAsync();
            return Skills;
                }

        public async Task<List<Skill>> GetSkillByUser(int UserId)
        {
            var skillids =await _context.UserSkills.Where(x=>x.UserId == UserId).Select(x=>x.SkillId).ToListAsync();
            var Skills = await _context.Skills.Where(x=> skillids.Contains(x.Id)).ToListAsync();
            return Skills;
        }

        public async Task<List<Skill>> SaveSkillsToOtherCategory(List<Skill> skills)
        {
            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync(); 
            return skills;
        }
    }
}
