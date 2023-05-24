namespace CheckpointAPI.Models
{
    public class CheckpointAdditionalAccessWithTitleID : CheckpointAdditionalAccess
    {
        public int CheckpointID { get; set; }
        public string CheckpointTitle { get; set; }
    }
}
