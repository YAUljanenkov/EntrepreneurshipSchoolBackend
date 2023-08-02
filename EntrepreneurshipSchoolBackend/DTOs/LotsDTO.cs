namespace EntrepreneurshipSchoolBackend.DTOs.Lots;

public record CreateLotDTO(int? id, string title, string description, string terms, int price, string performer);
public record UpdateLotDTO(int id, string? title, string? description, string? terms, int? price, string? performer);

public record LotInfoDTO
{
    public LotInfoDTO(Models.Lot lot)
    {
        this.id = lot.Id;
        this.title = lot.Title;
        this.description = lot.Description;
        this.terms = lot.Terms;
        this.price = lot.Price;
        if (lot.Learner != null)
        {
            performerUser = new Learner(lot.Learner);
            performerOther = null;
        }
        else if (lot.Performer != null && lot.Learner == null)
        {
            performerUser = null;
            performerOther = lot.Performer;
        }
        
    }

    public int? id { get; }
    public string title { get; }
    public string description { get; }
    public string terms { get; }
    public int price { get;  }
    public Learner? performerUser { get; }
    public string? performerOther { get; }
}


public record LotShortInfoDTO
{
    public LotShortInfoDTO(Models.Lot lot)
    {
        this.id = lot.Id;
        this.title = lot.Title;
        this.price = lot.Price;
        if (lot.Learner != null)
        {
            performerUser = new Learner(lot.Learner);
            performerOther = null;
        }
        else if (lot.Performer != null && lot.Learner == null)
        {
            performerUser = null;
            performerOther = lot.Performer;
        }
        
    }

    public int? id { get; }
    public string title { get; }
    public int price { get;  }
    public Learner? performerUser { get; }
    public string? performerOther { get; }
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