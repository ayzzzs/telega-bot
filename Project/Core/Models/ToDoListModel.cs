using System;
using LinqToDB.Mapping;

namespace bbbbb.Project.Core.DataAccess.Models
{
    [LinqToDB.Mapping.Table("ToDoList")]
    public class ToDoListModel
    {
        [LinqToDB.Mapping.PrimaryKey]
        [LinqToDB.Mapping.Column("Id")]
        public Guid Id { get; set; }

        [LinqToDB.Mapping.Column("Name")]
        public string Name { get; set; }

        [LinqToDB.Mapping.Column("UserId")]
        public Guid UserId { get; set; }

        [LinqToDB.Mapping.Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; }
    }
}