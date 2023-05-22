namespace CheckpointAPI.Models
{
    public class Checkpoint
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public int IDOffice { get; set; }
        public bool IsActive { get; set; }
    }
}
