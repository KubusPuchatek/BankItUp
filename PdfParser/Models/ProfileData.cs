using System.Collections.Generic;

namespace PdfParser.Models
{
    public class ProfileData
    {
        /* public string Id { get; set; }
         public List<Experience> ProfessionalExperience { get; set; }
         public List<Experience> Education { get; set; }
         public List<string> HonorsAndAwards { get; set; }*/

        public string Id { get; set; }
        public int ProfessionalExperience { get; set; }
        public int Education { get; set; }
        public int HonorsAndAwards { get; set; }
        public string CurrentCompany { get; set; }
        public string Position { get; set; }

        public ProfileData()
        {
            ProfessionalExperience = 0;
            Education = 0;
            HonorsAndAwards = 0;
        }

        
    }
}
