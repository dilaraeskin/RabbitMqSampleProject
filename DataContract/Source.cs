using System.ComponentModel.DataAnnotations;

namespace DataContract
{
    public class Source
    {
        [Key]
        public int SourceId { get; set; }
        public string Name { get; set; }
    }
}