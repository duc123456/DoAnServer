﻿using Application.DTOs;
using Domain.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IBlogService
    {
        public Task<Blog> CreateBlog(BlogCreateDTO dto);
        public Task<Pagination<BlogDTO>> GetBlogs(BlogSearch search);
    }
}