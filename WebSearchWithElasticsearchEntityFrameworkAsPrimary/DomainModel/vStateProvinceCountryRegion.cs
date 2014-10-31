namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Person.vStateProvinceCountryRegion")]
    public partial class vStateProvinceCountryRegion
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StateProvinceID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string StateProvinceCode { get; set; }

        [Key]
        [Column(Order = 2)]
        public bool IsOnlyStateProvinceFlag { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(50)]
        public string StateProvinceName { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TerritoryID { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(3)]
        public string CountryRegionCode { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(50)]
        public string CountryRegionName { get; set; }
    }
}
