﻿using API.Utilities;
using Application.DTOs;
using Application.IServices;

using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using static API.Common.Url;

namespace API.Controllers
{

    public class ProjectsController : ApiControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISkillService _skillService;
        private readonly IProjectRepository _projectRepository;
        public ProjectsController(IProjectService projectService, ICurrentUserService currentUserService, ISkillService skillService, IProjectRepository projectRepository)
        {
            _projectService = projectService;
            _currentUserService = currentUserService;
            _skillService = skillService;
            _projectRepository = projectRepository;

        }

        [HttpGet]
        [Route(Common.Url.Project.GetAll)]
        public async Task<IActionResult> Index(int pageIndex, int pageSize)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về BadRequest với ModelState khi ModelState không hợp lệ
            }

            if (pageIndex < 1 || pageSize < 1)
            {
                ModelState.AddModelError("", "Số trang hoặc kích cỡ trang lớn hơn 1"); // Thêm lỗi vào ModelState
                return BadRequest(ModelState); // Trả về BadRequest với ModelState đã thêm lỗi
            }
            else
            {
                return Ok(await _projectService.Get(pageIndex, pageSize));
            }
        }

        [HttpGet]
        [Route(Common.Url.Project.Search)]
        public async Task<IActionResult> Search([FromQuery] ProjectSearchDTO projects)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }
            Expression<Func<Domain.Entities.Project, bool>> filter = item => true;
            if (projects != null && !string.IsNullOrEmpty(projects.Keyword))
            {
                var keyword = projects.Keyword.ToLower().Trim();
                filter = item => item.Title.ToLower().Contains(keyword);
            }
            return Ok(await _projectService.GetWithFilter(filter, projects.PageIndex, projects.PageSize));
        }

        [HttpGet]
        [Route(Common.Url.Project.Gets)]
        public async Task<IActionResult> Gets([FromQuery] ProjectSearchDTO projects)
        {
            var projectDTOs = await _projectService.GetProjectDTOs(projects);
            return Ok(projectDTOs); 
        }

        [HttpPost]
        [Route(Common.Url.Project.UpdateStatus)]
        public async Task<IActionResult> UpdateStatus([FromQuery] int statusId, [FromQuery] int projectId)
        {
            var projectDTOs = await _projectService.UpdateProjectStatus(statusId , projectId);
            return Ok(projectDTOs);
        }

        [HttpGet]
        [Route(Common.Url.Project.AllStatus)]
        public async Task<IActionResult> AllStatus()
        {
            var DTOs = await _projectService.GetAllStatus();
            return Ok(DTOs);
        }



        [HttpPost]
        [Route(Common.Url.Project.Filter)]
        public async Task<IActionResult> Filter(ProjectFilter projects)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new SerializableError(ModelState));
            }
            Expression<Func<Domain.Entities.Project, bool>> filter = item => true;
            if (projects != null)
            {
                if (!string.IsNullOrWhiteSpace(projects.Keyword))
                {
                    var keyword = projects.Keyword.ToLower().Trim();
                    filter = filter.And(item => item.Title.Contains(keyword));
                }
                if (projects.CategoryId > 0)
                {
                    filter = filter.And(item => item.CategoryId == projects.CategoryId);
                }

                if (projects.CategoryId > 0)
                {
                    filter = filter.And(item => item.CategoryId == projects.CategoryId);
                }

                if (projects.SkillIds != null && projects.SkillIds.Any())
                {
                    filter = filter.And(item => item.ProjectSkills.Any(skill => projects.SkillIds.Contains(skill.SkillId)));
                }

                if (projects.Duration > 0)
                {
                    filter = filter.And(item => item.Duration <= projects.Duration);
                }

                if (projects.MinBudget > 0)
                {
                    filter = filter.And(item => item.MinBudget >= projects.MinBudget);
                }

                if (projects.MaxBudget > 0)
                {
                    filter = filter.And(item => item.MaxBudget <= projects.MaxBudget);
                }
            }
            return Ok(await _projectService.GetWithFilter(filter, projects.PageIndex, projects.PageSize));
        }

        [HttpGet]
        [Route(Common.Url.Project.GetProjectsByUserId)]
        public async Task<IActionResult> GetListByUserId([FromQuery] ProjectListDTO projects)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }
            var filter = PredicateBuilder.True<Domain.Entities.Project>();
            var userid =  _currentUserService.UserId;

            filter = filter.And(item => item.CreatedBy == userid);

            if (projects.StatusId != null)
            {
                filter = filter.And(item => item.StatusId == projects.StatusId);
            }

            return Ok(await _projectService.GetWithFilter(filter, projects.PageIndex, projects.PageSize));
        }

        [HttpGet]
        [Route(Common.Url.Project.GetProjectDetails)]
        public async Task<IActionResult> GetDetailProject(int id)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }

            var projectDetail = await _projectService.GetDetailProjectById(id);
            if (projectDetail == null)
            {
                return NotFound();
            }

            return Ok(projectDetail);
        }


        [HttpPost]
        [Route(Common.Url.Project.Add)]
        public async Task<IActionResult> AddAsync(AddProjectDTO DTOs, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }

            var project = await _projectService.Add(DTOs);

            if (project == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProjectResponse { Success = false, Message = "Failed to create project." });
            }

            await _skillService.AddSkillForProject(DTOs.Skill, project.Id);

            var response = new ProjectResponse
            {
                Success = true,
                Message = "Bạn vừa tạo dự án thành công",
                Data = project
            };

            return Ok(response);
        }



        [HttpPut]
        [Route(Common.Url.Project.Update)]
        public async Task<IActionResult> UpdateAsync(UpdateProjectDTO DTOs, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fetchedProject = await _projectRepository.GetByIdAsync(DTOs.Id);
            if (fetchedProject == null)
            {
                return NotFound(new { message = "Không tìm thấy dự án phù hợp!" });
            }

            var project = await _projectService.Update(DTOs);
            if (project == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProjectResponse { Success = false, Message = "Failed to update project." });
            }

            await _skillService.AddSkillForProject(DTOs.Skill, project.Id);

            var response = new ProjectResponse
            {
                Success = true,
                Message = "Bạn vừa cập nhật dự án thành công",
                Data = project
            };

            return Ok(response);
        }

        [HttpDelete]
        [Route(Common.Url.Project.Delete)]
        public async Task<IActionResult> DeleteAsync(int projectId, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fetchedProject = await _projectRepository.GetByIdAsync(projectId);
            if (fetchedProject == null)
            {
                return NotFound(new { message = "Không tìm thấy dự án phù hợp!" });
            }

            await _projectService.Delete(projectId);

            return Ok(new ProjectResponse
            {
                Success = true,
                Message = "Bạn vừa xóa dự án thành công"
            });
        }

        [HttpPut]
        [Route(Common.Url.Project.UpdateStatus)]
        public async Task<IActionResult> UpdateStatus(Application.DTOs.ProjectStatus DTOs, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fetchedProject = await _projectRepository.GetByIdAsync(DTOs.Id);
            if (fetchedProject == null)
            {
                return NotFound(new ProjectResponse { Success = false, Message = "Không tìm thấy dự án phù hợp!" });
            }

            var project = await _projectService.UpdateStatus(DTOs.Id, DTOs.StatusId);

            if (project == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProjectResponse { Success = false, Message = "Failed to update project status." });
            }

            return Ok(new ProjectResponse
            {
                Success = true,
                Message = "Bạn vừa thay đổi trạng thái dự án thành công",
                Data = project
            });
        }


    }
}
