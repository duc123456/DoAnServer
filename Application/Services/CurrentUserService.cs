﻿
using Application.Extensions;
using Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtTokenService _jwtService;

    public CurrentUserService(
        
        IJwtTokenService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            string jwtToken = GetJwtToken();
            if (jwtToken == null || jwtToken.IsMissing())
                throw new ("Bạn chưa đăng nhập");

            var id = _jwtService.DecodeAccessToken(jwtToken).Claims.First(c => c.Type == "Id").Value;
            if(id == null)
            {
                throw new("You are not logged in yet");
            }
            return int.Parse(id);
        }
    }

    

    public string Email
    {
        get
        {
            string jwtToken = GetJwtToken();
            if (jwtToken == null || jwtToken.IsMissing())
                throw new ("Bạn chưa đăng nhập");

            return _jwtService.DecodeAccessToken(jwtToken).Claims.First(c => c.Type == "Email").Value ??
                   throw new ("Bạn chưa đăng nhập");
        }
    }


    public bool HasRole(string role)
    {
        string jwtToken = GetJwtToken();
        if (jwtToken == null || jwtToken.IsMissing())
            throw new ("Bạn chưa đăng nhập");
        return _jwtService.DecodeAccessToken(jwtToken).Claims
            .Any(claim => claim.Type == nameof(role) && claim.Value == role);
    }

    public string? GetJwtToken()
    {
        HttpContext httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return null;
        }
        if (httpContext.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
        {
            string token = authHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
            {
                return token.Substring("Bearer ".Length).Trim();
            }
        }
        return null;
    }
}