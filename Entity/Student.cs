using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdonetMapper_DataAccess.Entity
{
    [Table("Student")]
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50), Required]
        public string Name { get; set; }

        [StringLength(50)]
        public string Surname { get; set; }

        public int? Age { get; set; }
    }
}
