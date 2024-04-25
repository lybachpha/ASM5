using ASM5.Areas.Data;

namespace ASM5.Models
{
    public class JobList
    {
        public int Id { get; set; }
        public string Vacancies { get; set; }
        public string JobDescription { get; set; }
        public string Request { get; set; }
        public decimal Salary { get; set; }
        public DateTime Time { get; set; }



        public string UserId { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<JobApplication>? JobApplication { get; set; }
    }
}
