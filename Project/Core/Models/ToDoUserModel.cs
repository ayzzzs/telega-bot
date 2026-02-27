using System;
using LinqToDB.Mapping;

namespace bbbbb.Project.Core.DataAccess.Models
{
    [LinqToDB.Mapping.Table("ToDoUser")]
    public class ToDoUserModel
    {
        [LinqToDB.Mapping.PrimaryKey]
        [LinqToDB.Mapping.Column("UserId")]
        public Guid UserId { get; set; }

        [LinqToDB.Mapping.Column("TelegramUserId")]
        public long TelegramUserId { get; set; }

        [LinqToDB.Mapping.Column("Username")]
        public string Username { get; set; }

        [LinqToDB.Mapping.Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}