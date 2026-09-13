using SchoolLibrary.Domain.Enums;

namespace SchoolLibrary.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ResourceCollectionType CollectionType { get; set; }

        public ICollection<Resource> Resources { get; set; }
            = new HashSet<Resource>();
    }
}
