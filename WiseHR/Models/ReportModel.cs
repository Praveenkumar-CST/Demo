namespace WiseHR.Models
{
    public class ReportModel
    {
        public string Id { get; set; }
        public string MenteeName { get; set; }
        public string MenteeEmail { get; set; }

        public string MentorEmail { get; set; }
        public string ProgressNote { get; set; }
        public int ProgressPercentage { get; set; }
        public byte[] UploadedFile { get; set; }
        public string FileName { get; set; }
        public DateTime SubmittedOn { get; set; }
    }

}
