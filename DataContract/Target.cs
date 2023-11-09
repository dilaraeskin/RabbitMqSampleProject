using System.ComponentModel.DataAnnotations;

namespace DataContract
{
    public class Target
    {
        [Key]
        public int TargetId { get; set; } 
        public string Name { get; set; }
    }
}
