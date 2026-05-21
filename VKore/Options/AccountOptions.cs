namespace VKore.Options;

public class AccountOptions 
{ 
    public string FirstName { get; set; } 
    public string LastName { get; set; } 
    public string Status { get; set; } 
    public DateTime? LastOnline { get; set; }
    public long SubscribersCount { get; set; } 
    public int FriendsCount { get; set; } 
}