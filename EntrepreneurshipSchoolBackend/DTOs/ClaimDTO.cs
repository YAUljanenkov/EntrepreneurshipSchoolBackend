namespace EntrepreneurshipSchoolBackend.DTOs;

public record ClaimDTO
{
    public int id { get; }
    public string claimType { get; }
    public Lot? lot { get; }
    public Task? task { get; }
    public Learner? receiver { get; }
    public Learner? learner { get; }
    public string dateTime { get; }
    public string status { get; }
    public int? sum { get; }
    public double? delay { get; }

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

        dateTime = claim.Date.ToString("dd.MM.yyyy HH:mm:ss");
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

        public int id { get; init; }
        public string name { get; init; }
    }
}