﻿namespace Gzpbot.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
    }
}
