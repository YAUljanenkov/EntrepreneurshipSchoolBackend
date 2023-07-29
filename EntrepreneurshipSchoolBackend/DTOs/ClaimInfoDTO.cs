namespace EntrepreneurshipSchoolBackend.DTOs;

public record ClaimInfoDTO
{
    public int id { get; }
    public string claimType { get; }
    public string dateTime { get; }
    public int? fine { get; }
    public string status { get; }
    public Lot? lot { get; }
    public Learner? learner { get; }
    public Deadline? deadline { get; }
    public ClaimInfoDTO(Models.Claim claim)
    {
        claimType = claim.Type.Name;
        id = claim.Id;
        status = claim.Status.Name;
        dateTime = claim.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        if (claimType == "BuyingLot" && claim.Lot != null)
        {
            lot = new Lot(claim.Lot);
            learner = new Learner(claim.Learner);
        }
        if (claimType == "FailedDeadline" && claim.Task != null )
        {
            deadline = new Deadline(claim);
            if (status == "Approved")
            {
                fine = claim.Sum;
            }
        }
        
        if (claimType == "PlacingLot" && claim.Lot != null)
        {
            lot = new Lot(claim.Lot);
        }
    }

    public record Lot
    {
        public int id { get; }
        public int number { get; }
        public string title { get; }
        public string description { get; }
        public string terms { get; }
        public string performer { get; }
        public Lot(Models.Lot lot)
        {
            id = lot.Id;
            number = lot.Number;
            title = lot.Title;
            description = lot.Description;
            terms = lot.Terms;
            performer = lot.Performer;
        }
    }
    
    public record Learner
    {
        public Learner(Models.Learner? learner)
        {
            if (learner != null)
            {
                this.id = learner.Id;
                this.name = $"{learner.Surname} {learner.Name} {learner.Lastname}";
            }
        }

        public int id { get; }
        public string name { get; }
    }

    public record Lesson(int id, int number);
    public record Task(int id, string title);

    public record Deadline
    {
        public Lesson lesson { get; }
        public Task task { get; }
        public string deadlineTime { get; }
        public Deadline(Models.Claim claim)
        {
            lesson = new Lesson(claim.Task.Lesson.Id, claim.Task.Lesson.Number);
            task = new Task(claim.Task.Id, claim.Task.Title);
            deadlineTime = claim.Task.Deadline.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        }
    }
}