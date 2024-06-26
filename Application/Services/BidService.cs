﻿using Application.DTOs;
using Application.Extensions;
using Application.IServices;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.IRepositories;
using FluentValidation;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BidService : IBidService
    {
        private readonly IMapper _mapper;
        private readonly IBidRepository _bidRepository;
        private readonly IUrlRepository _urlRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProjectSkillRepository _projectSkillRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStatusRepository _statusRepository;
        private readonly IAddressRepository _addressRepository;


        public BidService(IMapper mapper, IBidRepository bidRepository, IUrlRepository urlRepository, IProjectRepository projectRepository, IAppUserRepository appUserRepository, ICategoryRepository categoryRepository, IProjectSkillRepository projectSkillRepository, ICurrentUserService currentUserService, IStatusRepository statusRepository, IAddressRepository addressRepository)
        {
            _mapper = mapper;
            _bidRepository = bidRepository;
            _urlRepository = urlRepository;

            _projectRepository = projectRepository;
            _appUserRepository = appUserRepository;
            _categoryRepository = categoryRepository;
            _projectSkillRepository = projectSkillRepository;
            _currentUserService = currentUserService;
            _statusRepository = statusRepository;
            _addressRepository = addressRepository;
        }

        public async Task<BidDTO> GetBidById(int id)
        {
            var bid = await _bidRepository.GetByIdAsync(id);
            var bidDTO = _mapper.Map<BidDTO>(bid);
            return bidDTO;
        }


        public async Task<BidDTO> Add(BiddingDTO request)
        {
            var userId = _currentUserService.UserId;
            var bid = _mapper.Map<Bid>(request);

            
            
            bid.ProjectId = request.ProjectId;
            bid.UserId = userId;
            bid.Proposal = request.Proposal;
            bid.Duration = request.Duration;
            bid.Budget = request.Budget;
            bid.CreatedDate = DateTime.Now;

            var bidDTO = _mapper.Map<BidDTO>(bid);

            // Retrieve and map the user who created the project
            
            await _bidRepository.AddAsync(bid);

            var user = await _appUserRepository.GetByIdAsync(userId);
            bidDTO.AppUser = _mapper.Map<AppUserDTO>(user);
            var address = await _addressRepository.GetAddressByUserId(userId);
            bidDTO.AppUser.Address = _mapper.Map<AddressDTO>(address);

            //var status = await _statusRepository.GetByIdAsync(bid.Project.StatusId);
            //bidDTO.Project.ProjectStatus = _mapper.Map<ProjectStatusDTO>(status);

            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            bidDTO.Project = _mapper.Map<ProjectDTO>(project);

            //var category = await _categoryRepository.GetByIdAsync(request.Project.CategoryId);
            //bidDTO.Project.Category = _mapper.Map<CategoryDTO>(category);

            var listSkills = await _projectSkillRepository.GetListProjectSkillByProjectId(project.Id);
            foreach (var skill in listSkills)
            {
                bidDTO.Project.Skill.Add(skill.SkillName);
            }
            //var urlRecord = bid.CreateUrlRecordAsync("du-an", bid.ProjectId.ToString());
            //await _urlRepository.AddAsync(urlRecord);
            return bidDTO;
        }

        public async Task<Pagination<BidDTO>> GetWithFilter(Expression<Func<Bid, bool>> filter, int pageIndex, int pageSize)
        {
            var bids = await _bidRepository.GetAsync(filter, pageIndex, pageSize);
            var bidDTOs = _mapper.Map<Pagination<BidDTO>>(bids);
            var updatedItems = new List<BidDTO>();

            foreach (var bid in bidDTOs.Items)
            {
                var bidDTO = _mapper.Map<BidDTO>(bid);
                var user = await _appUserRepository.GetByIdAsync(bid.UserId);
                bidDTO.AppUser = _mapper.Map<AppUserDTO>(user);
                var address = await _addressRepository.GetAddressByUserId((int)bid.UserId);
                bidDTO.AppUser.Address = _mapper.Map<AddressDTO>(address);
                updatedItems.Add(bidDTO);
            }

            bidDTOs.Items = updatedItems;
            return bidDTOs;
        }

        public async Task<BidDTO> Update(UpdateBidDTO request)
        {

            var bid = await _bidRepository.GetByIdAsync(request.Id);
            if (bid == null)
            {
                return _mapper.Map<BidDTO>(bid);
            }
            // Update the project's properties
            bid.Proposal = request.Proposal;
            bid.Duration = request.Duration;
            bid.Budget = request.Budget;
            bid.UpdatedDate = DateTime.Now;

            // Update the project in the repository
            _bidRepository.Update(bid);

            // Handle URL record update
            //var urlRecord = project.CreateUrlRecordAsync("chinh-sua-du-an", project.Title);
            //await _urlRepository.AddAsync(urlRecord); // assuming there's a method for updating URLs

            // Map the updated project back to a DTO
            var bidDTO = _mapper.Map<BidDTO>(bid);

            // Retrieve and map the user who created the project
            var user = await _appUserRepository.GetByIdAsync(bid.UserId);
            bidDTO.AppUser = _mapper.Map<AppUserDTO>(user);
            var address = await _addressRepository.GetAddressByUserId((int)bid.UserId);
            bidDTO.AppUser.Address = _mapper.Map<AddressDTO>(address);

            //var project = await _projectRepository.GetByIdAsync(bid.ProjectId);
            //bidDTO.Project = _mapper.Map<ProjectDTO>(project);

            //var category = await _categoryRepository.GetByIdAsync(bid.Project.CategoryId);
            //bidDTO.Project.Category = _mapper.Map<CategoryDTO>(category);

            //var status = await _statusRepository.GetByIdAsync(bid.Project.StatusId);
            //bidDTO.Project.ProjectStatus = _mapper.Map<ProjectStatusDTO>(status);

            //var listSkills = await _projectSkillRepository.GetListProjectSkillByProjectId(project.Id);
            //foreach (var skill in listSkills)
            //{
            //    bidDTO.Project.Skill.Add(skill.SkillName);
            //}
            
            return bidDTO;
        }
        public async Task<BidDTO> AcceptBidding(long id)
        {

            var bid = await _bidRepository.GetByIdAsync(id);

            var project = await _projectRepository.GetByIdAsync(bid.ProjectId);

            //sau sẽ chỉnh theo statusId trong db
            project.StatusId = 2;
            _projectRepository.Update(project);

            bid.AcceptedDate = DateTime.Now;
            //mediafile

            // Update the project in the repository
            _bidRepository.Update(bid);

            // Handle URL record update
            //var urlRecord = project.CreateUrlRecordAsync("chinh-sua-du-an", project.Title);
            //await _urlRepository.AddAsync(urlRecord); // assuming there's a method for updating URLs

            // Map the updated project back to a DTO
            var bidDTO = _mapper.Map<BidDTO>(bid);
            
            return bidDTO;
        }

        public Task<BidDTO> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
