﻿using Application.DTOs;
using Application.IServices;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Text.Json;

namespace API.Controllers
{

    public class UsersController : ApiControllerBase
    {
        private readonly IAppUserService _appUserService;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ISkillService _skillService;
        private readonly IMediaService _mediaFileService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        public UsersController(IAppUserService appUserService, ICurrentUserService currentUserService, UserManager<AppUser> userManager, ISkillService skillService, IPasswordGeneratorService passwordGeneratorService, IMediaService mediaFileService)
        {
            _appUserService = appUserService;
            _currentUserService = currentUserService;
            _userManager = userManager;
            _skillService = skillService;
            _passwordGeneratorService = passwordGeneratorService;
            _mediaFileService = mediaFileService;
        }
        [HttpGet]
        [Route(Common.Url.User.Profile)]
        public async Task<IActionResult> Profile()
        {
            var userId = _currentUserService.UserId;
            //var userId = 9;
            var userDtos = await _appUserService.GetUserDTOAsync(userId);
            return (Ok(userDtos));
        }

        [HttpPost]
        [Route(Common.Url.User.Update)]
        public async Task<IActionResult> Update([FromBody] UserUpdateDTO dto)
        {
            var userId = _currentUserService.UserId;
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }
            
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return BadRequest("Bạn hãy đăng nhập");
            }
            user.PhoneNumber = String.IsNullOrEmpty(dto.PhoneNumber) ? "" : dto.PhoneNumber;
            user.Email = dto.Email;
            user.Name = dto.Name;
            user.TaxCode = String.IsNullOrEmpty(dto.TaxCode) ? "": dto.TaxCode;
            var userResult = await _userManager.UpdateAsync(user);
            if (userResult.Succeeded)
            {
                await _skillService.UpdateSkillForUser(dto.Skills, userId);
            }
            return Ok(dto);  
        }

        [HttpPut]
        [Route(Common.Url.User.ChangePassword)]
        public async Task<IActionResult> ChangePassword(UserChangePasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }
            var userId = _currentUserService.UserId;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (!_passwordGeneratorService.VerifyHashPassword(user.PasswordHash, dto.OldPassword))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mật khẩu cũ sai !"
                });
            }
            user.PasswordHash = _passwordGeneratorService.HashPassword(dto.NewPassword);
            var userResult = await _userManager.UpdateAsync(user);
            
            return Ok(new
            {
                success = true,
            });
        }

        [HttpPut]
        [Route(Common.Url.User.UpdateEducation)]
        public async Task<IActionResult> UpdateEducation(List<Education> educations)
        {
            var userId = _currentUserService.UserId;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            user.Education=  JsonSerializer.Serialize(educations, options);
            var userResult = await _userManager.UpdateAsync(user);
            return Ok(new
            {
                educations=  educations,
                success = true,
            });
        }
        [HttpPut]
        [Route(Common.Url.User.UpdateExperience)]
        public async Task<IActionResult> UpdateExperience(List<Experience> experiences)
        {
            var userId = _currentUserService.UserId;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            user.Experience = JsonSerializer.Serialize(experiences, options);
            var userResult = await _userManager.UpdateAsync(user);
            return Ok(new
            {
                experiences = experiences,
                success = true,
            });
        }
        [HttpPut]
        [Route(Common.Url.User.UpdateQualification)]
        public async Task<IActionResult> UpdateQualification(List<Qualification> qualifications)
        {
            var userId = _currentUserService.UserId;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            user.Qualifications = JsonSerializer.Serialize(qualifications, options);
            var userResult = await _userManager.UpdateAsync(user);
            return Ok(new
            {
                qualifications = qualifications,
                success = true,
            });
        }


        [HttpPost]
        [Route(Common.Url.User.AddPortfolio)]
        public async Task<IActionResult> AddPortfolio(MediaFileDTO mediaFile)
        {
            var userId = _currentUserService.UserId;
            var file =await _mediaFileService.AddMediaFile(mediaFile);
            return Ok(file);
        }

        [HttpPut]
        [Route(Common.Url.User.UpdatePortfolio)]
        public async Task<IActionResult> UpdatePortfolio(MediaFileDTO mediaFile)
        {
            var userId = _currentUserService.UserId;
            var file = await _mediaFileService.UpdateMediaFile(mediaFile);
            return Ok(file);
        }

        [HttpDelete]
        [Route(Common.Url.User.DeletePortfolio)]
        public async Task<IActionResult> DeletePortfolio(long id)
        {
            var userId = _currentUserService.UserId;
            var fileId = await _mediaFileService.DeleteMediaFile(id);
            return Ok(fileId);
        }
    }
}