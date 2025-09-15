namespace VitalSignApp.Models
{
    public class VitalSign
    {
        public int? Id { get; set; }
        public string? Hn { get; set; }

        public string? Name { get; set; }
        public int? Systolic { get; set; }
        public int? Diastolic { get; set; }
        public int? Pulse { get; set; }
        public int? Sp02 { get; set; }
        public int? RespiratoryRate { get; set; }
        public float? Height { get; set; }
        public float? Weight { get; set; }
        public float? Bmi { get; set; }
        public float? Temp { get; set; }
        public DateTime? RecordedAt { get; set; }
    }
}
