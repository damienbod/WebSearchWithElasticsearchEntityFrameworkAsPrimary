using Newtonsoft.Json;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Person.Address")]
    public partial class Address
    {
        public Address()
        {
            BusinessEntityAddress = new HashSet<BusinessEntityAddress>();
        }

		[Key]
        public int AddressID { get; set; }

        [Required]
        [StringLength(60)]
        public string AddressLine1 { get; set; }

        [StringLength(60)]
        public string AddressLine2 { get; set; }

        [Required]
        [StringLength(30)]
        public string City { get; set; }

        public int StateProvinceID { get; set; }

        [Required]
        [StringLength(15)]
        public string PostalCode { get; set; }

		[JsonIgnore]
        public DbGeography SpatialLocation { get; set; }

        public Guid rowguid { get; set; }

        public DateTime ModifiedDate { get; set; }

        public virtual StateProvince StateProvince { get; set; }

		[JsonIgnore]
        public virtual ICollection<BusinessEntityAddress> BusinessEntityAddress { get; set; }
    }
}
