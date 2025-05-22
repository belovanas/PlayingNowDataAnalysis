// See https://aka.ms/new-console-template for more information

using PlayingNowDataAnalysis;

var service = new Service();

var profileViews = service.GetProfileViews();

var trackPlays = service.GetTrackPlays();

var trackPlaysByUser = trackPlays
    .GroupBy(tp => tp.UserId)
    .ToDictionary(g => g.Key, g => g.OrderBy(tp => tp.CreatedOn).ToList());

int totalViews = profileViews.Count;
int matchingViews = 0;

foreach (var view in profileViews)
{
    if (trackPlaysByUser.TryGetValue(view.ProfileOwnerUserId, out var plays))
    {
        if (plays.Any(tp =>
                view.ViewedOn > tp.CreatedOn &&
                (view.ViewedOn - tp.CreatedOn).TotalMinutes <= 5))
        {
            matchingViews++;
        }
    }
}

double percentage = totalViews > 0 ? (matchingViews * 100.0 / totalViews) : 0;
Console.WriteLine($"Matching views percentage: {percentage}");
