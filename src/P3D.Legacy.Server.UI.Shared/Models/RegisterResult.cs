﻿using System.Collections.Generic;

namespace P3D.Legacy.Server.UI.Shared.Models
{
    public class RegisterResult
    {
        public bool Successful { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}