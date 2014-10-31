namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Person.vAdditionalContactInfo")]
    public partial class vAdditionalContactInfo
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BusinessEntityID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string TelephoneNumber { get; set; }

        public string TelephoneSpecialInstructions { get; set; }

        [StringLength(50)]
        public string Street { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string StateProvince { get; set; }

        [StringLength(50)]
        public string PostalCode { get; set; }

        [StringLength(50)]
        public string CountryRegion { get; set; }

        public string HomeAddressSpecialInstructions { get; set; }

        [StringLength(128)]
        public string EMailAddress { get; set; }

        public string EMailSpecialInstructions { get; set; }

        [StringLength(50)]
        public string EMailTelephoneNumber { get; set; }

        [Key]
        [Column(Order = 3)]
        public Guid rowguid { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime ModifiedDate { get; set; }
    }
}
