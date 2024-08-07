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
        public Task<BlogCreateDTO> UpdateBlog(BlogCreateDTO dto);

        public Task<List<BlogDTO>> GetBlogList(BlogFilter filter);
        public Task<Pagination<BlogDTO>> GetBlogs(BlogSearch search);

        public Task<BlogDTO> GetBlogDTOAsync(int id);

        public Task<bool> DeleteBlog(int id);
        
        public Task<int> PublishBlog(int id);

        public Task<LaziLoadDTO<RelatedBLogDTO>> GetOther (int blogId,string cursor, int limit);

        public Task<bool> AddRelatedBlog(RelatedAdd related);
    }
}
