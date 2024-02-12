using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Models
{
    public class ResultModel
    {
        public int Wpm { get; set; }
        public int Keystrokes { get; set; }
        public float Accuracy { get; set;}
        public int CorrectWords { get; set; }
        public int WrongWords { get; set; }
    }
}
