﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IChatGPTService
    {
        Task<string> GetChatGPTAnswer(string questionText);
    }
}
