﻿namespace TasksPrakticodeServer.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsComplete { get; set; } = false;

        public int UserId { get; set; } = 1;
    }
}
