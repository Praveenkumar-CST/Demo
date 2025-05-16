namespace FingerFrontend.Models
{
        public class AttendanceDto
        {
            public string UserId { get; set; } = "";
            public string Name { get; set; } = "";
            public DateTime Timestamp { get; set; }
            public string Status { get; set; } = "";
        }

        public class AttendanceSession
        {
            public DateTime ClockIn { get; set; }
            public DateTime? ClockOut { get; set; }
            public double Hours => CalculateHours();

            private double CalculateHours()
            {
                if (!ClockOut.HasValue)
                    return 0;
                var hours = (ClockOut.Value - ClockIn).TotalHours;
                return hours < 0 ? 0 : Math.Round(hours, 2);
            }
        }

        public class AbsencePeriod
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string? Reason { get; set; }
        }

        public class AttendanceDay
        {
            public DateTime Date { get; set; }
            public List<AttendanceSession> Sessions { get; set; } = new();
            public List<AbsencePeriod> AbsencePeriods { get; set; } = new();
            public double TotalHours => Math.Round(Sessions.Sum(s => s.Hours), 2);
            public bool IsWeeklyOff { get; set; }
        public string Name { get; set; } = "";
        public string UserId { get; set; } = "";
    }

        public class AttendanceMonth
        {
            public DateTime Month { get; set; }
            public List<AttendanceDay> Days { get; set; } = new();
        }

        public class AbsenceReasonDto
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Reason { get; set; }
            public bool IsApproved { get; set; }
        }




    
}
