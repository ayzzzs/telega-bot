using System;
using LinqToDB.Mapping;

namespace bbbbb.Project.Core.DataAccess.Models
{
    [LinqToDB.Mapping.Table("ToDoItem")]
    public class ToDoItemModel
    {
        [LinqToDB.Mapping.PrimaryKey]
        [LinqToDB.Mapping.Column("Id")]
        public Guid Id { get; set; }

        [LinqToDB.Mapping.Column("Name")]
        public string Name { get; set; }

        [LinqToDB.Mapping.Column("UserId")]
        public Guid UserId { get; set; }

        [LinqToDB.Mapping.Column("ListId")]
        public Guid? ListId { get; set; }

        [LinqToDB.Mapping.Column("Deadline")]
        public DateTime Deadline { get; set; }

        [LinqToDB.Mapping.Column("State")]
        public int State { get; set; }

        [LinqToDB.Mapping.Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; }

        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel? List { get; set; }
    }
}