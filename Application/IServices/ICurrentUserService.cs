﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string Name { get; }
        int UserIdCan0 { get; }
        string Email { get; }
        bool HasRole(string role);
        string? GetJwtToken();
    }
}
