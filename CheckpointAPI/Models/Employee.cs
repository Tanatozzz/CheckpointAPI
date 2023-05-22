namespace CheckpointAPI.Models
{
    public class Employee
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string Patronomyc { get; set; }
        public string LastName { get; set; }
        public DateTime LastVisitDate { get; set; }
        public bool isInside { get; set; }
        public int IDRole { get; set; }
        public int IDAdditionAccess { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public string INN { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
