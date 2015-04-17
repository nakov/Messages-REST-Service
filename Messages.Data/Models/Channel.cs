namespace Messages.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Channel
    {
        public Channel()
        {
            this.Messages = new HashSet<ChannelMessage>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(100)]
        [MinLength(1)]
        public string Name { get; set; }

        public virtual ICollection<ChannelMessage> Messages { get; set; }
    }
}
