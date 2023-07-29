namespace EntrepreneurshipSchoolBackend.DTOs;

public record ClaimInfoDTO
{
    public int id { get; }
    public string claimType { get; }
    public string dateTime { get; }
    public int? fine { get; }
    public string status { get; }
    public Lot? lot { get; }
    public ClaimInfoDTO(Models.Claim claim)
    {
        
    }

    public record Lot
    {
        public int id { get; }
        public int number { get; }
        public string title { get; }
        public string description { get; }
        public string terms { get; }
        public Learner performer { get; }
        public Lot(Models.Lot lot)
        {
            id = lot.Id;
            number = lot.Number;
            title = lot.Title;
            description = lot.Description;
            terms = lot.Terms;
            performer = new Learner(lot.Learner);
        }
    }
    
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