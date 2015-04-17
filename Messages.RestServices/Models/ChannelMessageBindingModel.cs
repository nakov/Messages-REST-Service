namespace Messages.RestServices.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ChannelMessageBindingModel
    {
        [Required]
        public string Text { get; set; }
    }
}
