namespace Messages.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ChannelMessage
    {
        public int Id { get; set; }

        public virtual User SenderUser { get; set; }

        [Required]
        public virtual Channel Channel { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime DateSent { get; set; }
    }
}
