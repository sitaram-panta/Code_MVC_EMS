    using System.ComponentModel.DataAnnotations;

    namespace employeeDailyTaskRecorder.Models
    {
        public class EmployeeMigration
        {
            [Key]
            public int Id { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string Email { get; set; }
            [Required]
            public string Address { get; set; }
            public string ContactNumber { get; set; }
            public EnumEmployeeGender Gender { get; set; }
            public EnumEmployeeStage EmpStage { get; set; }
            public EnumMajorRole EmpRole { get; set; }
            public DateTime JoinDate { get; set; }
            public EnumEmployeeType EmpType { get; set; }
      
        }
    }
