using System;
using System.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VKore.Options;
using VKore.Logger;

namespace VKore.VK_Services;

public class VkSession 
{ 
    public VkApi Api { get; private set; } 

    public AccountOptions Me { get; private set; } 

    public void Initialize(string token) 
    { 
        Api = new VkApi(); 

        Api.Authorize(new ApiAuthParams 
        { 
            AccessToken = token 
        }); 

        var myProfile = Api.Users.Get(Array.Empty<long>(), ProfileFields.All).FirstOrDefault(); 

        if (myProfile != null) 
        { 
            Me = new AccountOptions
            {
                FirstName = myProfile.FirstName,
                LastName = myProfile.LastName,
                Status = myProfile.Status,
                LastOnline = myProfile.LastSeen?.Time,
                SubscribersCount = myProfile.FollowersCount ?? 0,
                FriendsCount = myProfile.Counters?.Friends ?? 0
            };
        } 
        else
        {
            throw new Exception("Не удалось получить данные о профиле. Возможно, токен недействителен.");
        }
    } 
}