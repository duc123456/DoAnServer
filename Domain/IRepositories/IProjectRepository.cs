﻿using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.IRepositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<int> GetTotalBids(int projectId);
        Task<int> GetAverageBudget(int projectId);
        Task<List<Project>> GetAllProject();
        Task<Pagination<Project>> ProjectToPagination(int pageIndex, int pageSize);
        public Task<Pagination<Project>> ProjectGetAsync(
            Expression<Func<Project, bool>> filter,
            int pageIndex,
            int pageSize);
        public Task<Pagination<Project>> RecruiterGetAsync(
            Expression<Func<Project, bool>> filter,
            int pageIndex,
            int pageSize);
        public Task<Pagination<Project>> GetAsyncForRecruiter(
            Expression<Func<Project, bool>> filter, int userId,
            int pageIndex,
            int pageSize);

    }
}
