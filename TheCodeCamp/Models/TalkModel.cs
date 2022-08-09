using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheCodeCamp.Models
{
    public class TalkModel
    {
        public int TalkID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [StringLength(4096, MinimumLength = 1)]
        public string Abstract { get; set; }
        [Required]
        [Range(100, 500)]
        public int Level { get; set; }
        public SpeakerModel Speaker { get; set; }
    }
}