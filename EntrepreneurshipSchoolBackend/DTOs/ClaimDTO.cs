namespace EntrepreneurshipSchoolBackend.DTOs;

/// <summary>
/// Full claim object from the database.
/// </summary>
public record ClaimDTO
{
    /// <summary>
    /// Id from the database.
    /// </summary>
    public int id { get; }
    
    /// <summary>
    /// Name of the claim's type.
    /// </summary>
    public string claimType { get; }
    
    /// <summary>
    /// An object that describes a lot.
    /// </summary>
    public Lot? lot { get; }
    
    /// <summary>
    /// An object that describes a task.
    /// </summary>
    public Task? task { get; }
    
    /// <summary>
    /// An object that describes a learner that receives a service or money.
    /// </summary>
    public Learner? receiver { get; }
    
    /// <summary>
    /// An object that describes a learner that provides a service or money.
    /// </summary>
    public Learner? learner { get; }
    
    /// <summary>
    /// A date and a time of an event in a format dd.MM.yyyy HH:mm:ss.
    /// </summary>
    public string dateTime { get; }
    
    /// <summary>
    /// A status of a claim: Waiting, Approved, Rejected.
    /// </summary>
    public string status { get; }
    
    /// <summary>
    /// A cost of a claim.
    /// </summary>
    public int? sum { get; }
    
    /// <summary>
    /// If a claim is a fine for skipping a deadline, this delay shows for how long a deadline was skipped (in ms). 
    /// </summary>
    public double? delay { get; }

    /// <summary>
    /// A constructor for a claimDTO from a Model of a claim.
    /// </summary>
    /// <param name="claim">A model of a claim</param>
    public ClaimDTO(Models.Claim claim)
    {
        this.id = claim.Id;
        this.claimType = claim.Type.Name;
        if (claim.Lot != null)
        {
            this.lot = new Lot(claim.Lot.Id, claim.Lot.Number);
        }
        if (claim.Learner != null)
        {
            this.learner = new Learner(claim.Learner);
        }
        if (claim.Receiver != null)
        {
            this.receiver = new Learner(claim.Receiver);
        }
        if (claim.Task != null)
        {
            this.task = new Task(claim.Task.Id, claim.Task.Title);
        }
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        DateTime dateTimeConverted = TimeZoneInfo.ConvertTime(claim.Date, tz);
        dateTime = dateTimeConverted.ToString("O");
        status = claim.Status.Name;
        if (claim.Sum != null)
        {
            sum = claim.Sum;
        }

        if (claim.Type.Name == "FailedDeadline" && claim.Task != null)
        {
            delay = (claim.Date - claim.Task.Deadline).TotalMilliseconds;
        }
    }

    public record Lot(int id, int number);
    public record Task(int id, string title);
    public record Learner
    {
        public Learner(Models.Learner learner)
        {
            this.id = learner.Id;
            this.name = $"{learner.Surname} {learner.Name} {learner.Lastname}";
        }

        public int id { get; }
        public string name { get; }
    }
}