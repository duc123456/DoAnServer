﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Application.IServices
{
    public interface IExportService
    {
        Task<Stream> GenerateExcelFileStream(string fileName, bool isChat);
        Task<string> GetChatGPTAnswer(string questionText);
        Task<string> GetChatGPTAnswer2(string questionText);
        Task<string> SensitiveWord(string questionText);
    }
}
