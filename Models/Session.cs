using System;
using System.Collections.Generic;

namespace TodoApi.Models;

 public partial class Session  // כיתה חדשה ל-SESSION
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string IpAddress { get; set; } = null!;
        public bool IsValid { get; set; }
    }
