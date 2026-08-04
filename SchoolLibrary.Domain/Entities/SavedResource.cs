namespace SchoolLibrary.Domain.Entities
{
    public class SavedResource
    {
        public Guid UserId { get; set; }

        public Guid ResourceId { get; set; }

        public DateTime SavedAtUtc { get; set; }

        public Resource Resource { get; set; } = null!;
    }
}
