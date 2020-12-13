﻿using System;
namespace FireflySoft.RateLimit.Core
{
    public class RateLimitResponse<T>
    {
        public bool IsLimit { get; set; }
        public RateLimitError Error { get; set; }
        public RateLimitRule<T> Rule { get; set; }
    }
}
