namespace CheckpointAPI.Models
{
    public class CheckpointRoleWithTitleID : CheckpointRole
    {
        public int CheckpointID { get; set; }
        public string CheckpointTitle { get; set; }
    }
}
