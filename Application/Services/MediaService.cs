﻿using Application.DTOs;
using Application.DTOs;
using Application.IServices;
using Application.Repositories;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepositories;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MediaService : IMediaService
    {
        private readonly IMediaFileRepository _mediaRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public MediaService(IMediaFileRepository mediaRepository, ApplicationDbContext context, IMapper mapper)
        {
            _mediaRepository = mediaRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<MediaFileDTO>> GetByUserId(int uid)
        {
            var mediaDTOs = new List<MediaFileDTO>();
            var medias = await _mediaRepository.GetByUserId(uid);
            if (medias.Any())
            {
                mediaDTOs = _mapper.Map<List<MediaFileDTO>>(medias);
            }
            return mediaDTOs;
        }

        public async Task<string> UploadAsync(IFormFile request, CancellationToken token)
        {
            try
            {
                await _mediaRepository.UploadAsync(request); // lưu trong File
                int FolderIdRequest =0;
                var extension = Path.GetExtension(request.FileName);
                var folder = "";
                if (Media.extensionFiles.Contains(extension))
                {
                    FolderIdRequest = (int) EnumCommon.File.FilesFolder;
                }
                else if (Media.extensionImages.Contains(extension))
                {
                    FolderIdRequest = (int)EnumCommon.File.ImageFolder;
                }
                var mediaFile = new MediaFile
                {
                    FileName = request.FileName,
                    FolderId = FolderIdRequest,
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                };
                  await _mediaRepository.AddAsync(mediaFile);
                await _context.SaveChangesAsync(token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return request.FileName;
        }

        public Task<string> UploadAsync(IFormFile request)
        {
            throw new NotImplementedException();
        }

        public async Task<MediaFileDTO> AddMediaFile(MediaFileDTO mediaFile)
        {
            var media = new MediaFile()
            {
                FileName = mediaFile.FileName,
                UserId = mediaFile.UserId,
                CreateAt = DateTime.Now,
                Description = mediaFile.Description,
                Title = mediaFile.Title,
            };
            await _mediaRepository.AddAsync(media);
            return mediaFile;
        }

        public async Task<MediaFileDTO> UpdateMediaFile(MediaFileDTO mediaFile)
        {
            var media = await _mediaRepository.GetByIdAsync(mediaFile.Id);
            media.Description = mediaFile.Description;
            media.Title = mediaFile.Title;
            media.FileName = mediaFile.FileName;
            _mediaRepository.Update(media);
            return mediaFile;
        }

        public async Task<long> DeleteMediaFile(long id)
        {
             await _mediaRepository.Delete(id);
            return id;
        }
    }
}
