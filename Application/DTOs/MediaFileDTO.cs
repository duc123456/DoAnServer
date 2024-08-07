﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class MediaFileDTO
    {
        public long Id { get; set; }
        public string FileName { get; set; }

        public string? Description { get; set; }

        public string? Title { get; set; }

        public int? UserId { get; set; } // null able
    }

    public class MediaFileDTOValidator : AbstractValidator<MediaFileDTO>
    {
        public MediaFileDTOValidator()
        {
            RuleFor(v => v.FileName)
                .NotEmpty().WithMessage("Ảnh không được để trống");
            RuleFor(v => v.Description)
                .NotEmpty().WithMessage("Mô tả không được để trống")
                .MaximumLength(1000).WithMessage("Mô tả không quá 1000 kí tự");

            RuleFor(v => v.Title)
                .NotEmpty().WithMessage("Tiêu đề không được để trống")
                .MaximumLength(200).WithMessage("Tiêu đề không quá 200 kí tự    "); ;
        }
    }
}
