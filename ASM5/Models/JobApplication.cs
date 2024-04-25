using ASM5.Areas.Data;

namespace ASM5.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public string Experience { get; set; }


        // Liên kết với công việc mà người nộp đơn muốn ứng tuyển
        public int JobListId { get; set; }
        public virtual JobList? JobList { get; set; }

        // Liên kết với người nộp đơn
        public string UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
