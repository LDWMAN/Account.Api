using System;

namespace AccountApi.Model.Configuration
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public double ExpriryTimeFrame { get; set; }
        public double RefreshExpriryDate { get; set; }
    }
}