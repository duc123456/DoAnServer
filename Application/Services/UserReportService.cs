﻿using Application.DTOs;
using Application.Extensions;
using Application.IServices;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserReportService : IUserReportService
    {
        private readonly IReportRepository _repository;
        private readonly IReportCategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly PaginationService<ReportDTO> _paginationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IProjectService _projectService;
        private readonly IBidService _bidService;

        public UserReportService(IReportRepository repository, IMapper mapper, PaginationService<ReportDTO> paginationService, IReportCategoryRepository categoryRepository, UserManager<AppUser> userManager, IProjectService projectService, IBidService bidService)
        {
            _repository = repository;
            _mapper = mapper;
            _paginationService = paginationService;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _projectService = projectService;
            _bidService = bidService;
        }

        public async Task CreateReport(ReportDTO dto)
        {
            var userReport = _mapper.Map<UserReport>(dto);
            userReport.CreatedDate = DateTime.Now;
            await _repository.AddAsync(userReport);
        }

        public async Task<Pagination<ReportDTO>> GetReports(ReportSearchDTO searchDTO)
        {

            var reports =await _repository.GetAll();
            var reportDTOs = reports.Select( report =>
            {
                var reportDTO = _mapper.Map<ReportDTO>(report);
                var cateReport =  _categoryRepository.GetByIdAsync(reportDTO.ReportCategoryId);
                reportDTO.ReportType = cateReport.Result.Description;
                reportDTO.ReportName = cateReport.Result.Name;
                var userReport = _userManager.FindByIdAsync(report.CreatedBy.ToString());
                reportDTO.NameCreatedBy = userReport.Result.Name;

                if(report.BidId != null) {
                    var Bid = _bidService.GetBidById((int)report.BidId);
                    var userBid = _userManager.FindByIdAsync(Bid.Result.UserId.ToString());
                    reportDTO.BidUser = userBid.Result.Name;
                }else if(report.ProjectId != null) {
                    var project = _projectService.GetDetailProjectById((int)report.ProjectId);
                    reportDTO.ProjectName = project.Result.Title;
                    var userProject = _userManager.FindByIdAsync(project.Result.CreatedBy.ToString());
                    reportDTO.ProjectUser = userProject.Result.Name;
                }
                return reportDTO;
            }).ToList();
            if(searchDTO.typeDes != null)
            {
                reportDTOs =  reportDTOs.Where(x=>x.ReportType == searchDTO.typeDes).ToList();
            }
            if(searchDTO.approved != null)
            {
                reportDTOs = reportDTOs.Where(x => x.IsApproved == searchDTO.approved).ToList();
            }
            return await _paginationService.ToPagination(reportDTOs, searchDTO.PageIndex, searchDTO.PageSize);
        }

        public async Task<bool> ApproveReport(int id)
        {
            var report = await _repository.GetByIdAsync(id);
            if(report == null)
            {
                return false;
            }else
            {
                report.IsApproved = true;
                _repository.Update(report);
                return true;
            }

        }
    }
}
