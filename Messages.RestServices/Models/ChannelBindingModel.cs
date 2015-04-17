namespace Messages.RestServices.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ChannelBindingModel
    {
        [Required]
        [MaxLength(100)]
        [MinLength(1)]
        public string Name { get; set; }
    }
}
