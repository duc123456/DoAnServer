﻿using Application.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Statistic;
using AutoMapper;
using Domain.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Application.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly ApplicationDbContext _context;
        public StatisticService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<CategoriesPieChart>> GetCategoryPieChartData()
        {
            var result = await _context.Categories
                .Select(c => new CategoriesPieChart
                {
                    CategoryName = c.CategoryName,
                    TotalProjects = c.Projects.Count(p => p.StatusId != 1  && p.IsDeleted == false)
                })
                .ToListAsync();

            return result;
        }

        public async Task<ProjectsPieChart> GetProjectPieChartData()
        {
            var totalProjects =
                await _context.Projects.CountAsync(p => p.StatusId != 1  && p.IsDeleted == false);
            var completedProjects = await _context.Projects.CountAsync(p => p.StatusId == 6);

            var result = new ProjectsPieChart
            {
                TotalAppovedProjects = totalProjects,
                CompletedProjects = completedProjects
            };

            return result;
        }

        public async Task<UsersPieChart> GetUserPieChartData()
        {
            var freelancerCount = await _context.UserRoles
                .Where(ur => ur.RoleId == 1)
                .Select(ur => ur.UserId)
                .CountAsync();
            var recruiterCount = await _context.UserRoles
                .Where(ur => ur.RoleId == 2)
                .Select(ur => ur.UserId)
                .CountAsync();

            var result = new UsersPieChart
            {
                FreelacerCount = freelancerCount,
                RecruiterCount = recruiterCount
            };

            return result;
        }

        public async Task<List<NewUser>> GetNewUserData()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var newUserCounts = await _context.Users
                .Join(_context.UserRoles,
                    u => u.Id,
                    ur => ur.UserId,
                    (u, ur) => new { User = u, UserRole = ur })
                .Where(j => (j.UserRole.RoleId == 1 || j.UserRole.RoleId == 2) && j.User.CreatedDate >= thirtyDaysAgo)
                .GroupBy(j => j.User.CreatedDate.Date)
                .Select(g => new NewUser
                {
                    CreatedDate = g.Key,
                    TotalUserCount = g.Count(),
                    FreelancerCount = g.Count(j => j.UserRole.RoleId == 1),
                    RecruiterCount = g.Count(j => j.UserRole.RoleId == 2)
                })
                .ToListAsync();

            foreach (var userCount in newUserCounts)
            {
                userCount.TotalUserCount = userCount.FreelancerCount + userCount.RecruiterCount;
            }

            return newUserCounts;
        }

        public async Task<Pagination<StatisticProjects>> GetProjectStatisticData(int pageIndex, int pageSize)
        {
            var query =
                from c in _context.Categories
                join p in _context.Projects on c.Id equals p.CategoryId into projectGroup
                from pg in projectGroup.DefaultIfEmpty()
                join b in _context.Bids on pg.Id equals b.ProjectId into bidGroup
                from bg in bidGroup.DefaultIfEmpty()
                where !c.IsDeleted && (pg == null || !pg.IsDeleted)
                group new { c, pg, bg } by c.CategoryName into g
                select new StatisticProjects
                {
                    CategoryName = g.Key,
                    MinimumBudget = g.Min(x => x.pg != null ? x.pg.MinBudget : (int?)null) ?? 0,
                    MaximumBudget = g.Max(x => x.pg != null ? x.pg.MaxBudget : (int?)null) ?? 0,
                    AverageBudget = (float)(g.Average(x => x.pg != null ? (x.pg.MinBudget + x.pg.MaxBudget) / 2.0 : (double?)null) ?? 0),
                    MinimumDuration = g.Min(x => x.pg != null ? x.pg.Duration : (int?)null) ?? 0,
                    MaximumDuration = g.Max(x => x.pg != null ? x.pg.Duration : (int?)null) ?? 0,
                    AverageDuration = (float)(g.Average(x => x.pg != null ? x.pg.Duration : (double?)null) ?? 0),
                    MinimumBid = g.Min(x => x.bg != null ? x.bg.Budget : (int?)null) ?? 0,
                    MaximumBid = g.Max(x => x.bg != null ? x.bg.Budget : (int?)null) ?? 0,
                    AverageBid = (int)Math.Ceiling(g.Average(x => x.bg != null ? (double?)x.bg.Budget : null) ?? 0),
                    TotalProjects = g.Count(x => x.pg != null)  // Count the total number of projects
                };

            var totalCount = await query.CountAsync();

            var paginatedResult = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Pagination<StatisticProjects>
            {
                TotalItemsCount = totalCount,
                PageSize = pageSize,
                PageIndex = pageIndex,
                Items = paginatedResult
            };
        }

        public async Task<Pagination<StatisticUsers>> GetUserStatisticData(int type, int pageIndex, int pageSize)
        {
            var query = from r in _context.Roles
                join ur in _context.UserRoles on r.Id equals ur.RoleId
                join u in _context.Users on ur.UserId equals u.Id
                from rt in _context.RateTransactions
                    .Where(rt => rt.ProjectUserId == u.Id || rt.BidUserId == u.Id)
                    .DefaultIfEmpty()
                join p in _context.Projects on rt.ProjectId equals p.Id into projects
                from p in projects.DefaultIfEmpty()
                where u != null &&
                      (rt == null ||
                       (rt.ProjectAcceptedDate != null && rt.BidCompletedDate != null))
                group new { User = u, Role = r, RateTransaction = rt, Project = p } by new { UserName = u.Name, RoleName = r.Name } into g
                select new StatisticUsers
                {
                    UserName = g.Key.UserName,
                    Role = g.Key.RoleName,
                    TotalCompletedProjects = g.Count(x => x.Project != null),
                    TotalPositiveRatings = g.Sum(x => x.RateTransaction != null && x.RateTransaction.Rated == true && x.RateTransaction.ProjectAcceptedDate != null ? 1 : 0),
                    TotalNegativeRatings = g.Sum(x => x.RateTransaction != null && x.RateTransaction.Rated == false && x.RateTransaction.ProjectAcceptedDate != null ? 1 : 0)
                };



            IQueryable<StatisticUsers> orderedQuery;

            if (type == 1)
            {
                orderedQuery = query.OrderByDescending(x => x.TotalPositiveRatings);
            }
            else if (type == 2)
            {
                orderedQuery = query.OrderByDescending(x => x.TotalNegativeRatings);
            }
            else
            {
                return null;
            }

            var totalCount = await query.CountAsync();

            var paginatedResult = await orderedQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Pagination<StatisticUsers>
            {
                TotalItemsCount = totalCount,
                PageSize = pageSize,
                PageIndex = pageIndex,
                Items = paginatedResult
            };
        }


        public async Task<Pagination<StatisticSkills>> GetSkillStatisticData(int type, int pageIndex, int pageSize)
        {
            var query =
                from c in _context.Categories
                join s in _context.Skills on c.Id equals s.CategoryId
                join ps in _context.ProjectSkills on s.Id equals ps.SkillId into projectSkills
                from ps in projectSkills.DefaultIfEmpty()
                join p in _context.Projects.Where(p => p.StatusId != 1 && p.StatusId != 5 && !p.IsDeleted) on ps.ProjectId equals p.Id into projects
                from p in projects.DefaultIfEmpty()
                join us in _context.UserSkills on s.Id equals us.SkillId into userSkills
                from us in userSkills.DefaultIfEmpty()
                group new { c.CategoryName, s.SkillName, Project = p, UserSkill = us } by new { c.CategoryName, s.SkillName } into g
                select new StatisticSkills
                {
                    CategoryName = g.Key.CategoryName,
                    SkillName = g.Key.SkillName,
                    TotalApprovedProject = g.Select(x => x.Project.Id).Distinct().Count(id => id != null),
                    TotalUsers = g.Select(x => x.UserSkill.UserId).Distinct().Count()
                };

            IQueryable<StatisticSkills> orderedQuery = null;

            if (type == 1)
            {
                orderedQuery = query.OrderByDescending(x => x.TotalApprovedProject);
            }
            else if (type == 2)
            {
                orderedQuery = query.OrderByDescending(x => x.TotalUsers);
            }
            else
            {
                return null;
            }

            var totalCount = await query.CountAsync();

            var paginatedResult = await orderedQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Pagination<StatisticSkills>
            {
                TotalItemsCount = totalCount,
                PageSize = pageSize,
                PageIndex = pageIndex,
                Items = paginatedResult
            };
        }
    }
}