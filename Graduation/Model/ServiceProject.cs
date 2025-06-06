﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class ServiceProject
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double PriceRange { get; set; }

        [ForeignKey(nameof(User))]
        public int UsersID { get; set; }
        public ApplicationUser User { get; set; }
        [ForeignKey(nameof(Type))]
        public int TypeId { get; set; }
        public TypeService Type { get; set; }

        [ForeignKey(nameof(Address))]
        public int AddressId { get; set; }
        public AddressToProject Address { get; set; }

        public IEnumerable<ImageDetails> ImageDetails { get; set; } = new HashSet<ImageDetails>();
        public IEnumerable<Review> Reviews { get; set; } = new HashSet<Review>();
        public IEnumerable<SaveProject> Saves { get; set; } = new HashSet<SaveProject>();
    }
}
