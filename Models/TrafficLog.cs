using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models
{
    public class TrafficLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public int Count { get; set; }
    }
}