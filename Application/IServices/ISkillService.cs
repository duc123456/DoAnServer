﻿using Application.DTOs;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ISkillService
    {
        Task<List<Skill>> AddSkillForUser(List<string> skillNames, int uid);
        Task<List<Skill>> AddSkillForProject(List<string> skillNames, int pId);

        Task<List<Skill>> UpdateSkillForUser(List<string> skillNames, int uid);

        Task<Pagination<SkillDTO>> Get(int pageIndex, int pageSize);
        Task<List<SkillDTO>> GetAll();

        Task<List<SkillDTO>> GetForUser(int uid);

        Task<Pagination<SkillDTO>> GetWithFilter(Expression<Func<Skill, bool>> filter, int pageIndex, int pageSize);
        Task<int> Add(SkillDTO request);

        Task<bool> DeleteAsync(int id);
        Task<int> UpdateAsync(SkillDTO request);
        Task<Pagination<SkillDTO>> Gets(SkillSearchDTO search);

        Task<Skill> GetSkillByNameAsyn(string skillName);
        //Task<List<SkillDTO>> GetByProjectId(int pId);

    }
}
