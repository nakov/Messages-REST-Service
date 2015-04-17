namespace Messages.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UserMessage
    {
        public int Id { get; set; }

        public virtual User SenderUser { get; set; }

        [Required]
        public virtual User RecipientUser { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime DateSent { get; set; }
    }
}
