﻿using Application.DTOs;
using Application.Extensions;
using Application.IServices;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IProjectService
    {
        Task<Pagination<ProjectDTO>> Get(int pageIndex, int pageSize);

        Task<Pagination<ProjectDTO>> GetProjectDTOs(ProjectSearchDTO search);

        Task<ProjectDTO> UpdateProjectStatus(int statusId, int projectId);

        Task<List<ProjectStatusDTO>> GetAllStatus();

        Task<Pagination<ProjectDTO>> GetWithFilter(Expression<Func<Project, bool>> filter, int pageIndex, int pageSize);


        Task<ProjectDTO> GetDetailProjectById(int id);

        Task<ProjectDTO> Add(AddProjectDTO request);

        Task<ProjectDTO> Update(UpdateProjectDTO request);

        Task<ProjectDTO> Delete(int id);

        Task<ProjectDTO> UpdateStatus(int projectId, int statusId);


        //Task<int> CreateAsync(Project request);

        //Task<Pagination<ProjectDTO>> GetProjectByCategory(int id, int pageIndex, int pageSize);

    }
}
