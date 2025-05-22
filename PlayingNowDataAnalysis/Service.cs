using System.Text.Json;

namespace PlayingNowDataAnalysis;

public class ProfileView
{
    public Guid ProfileOwnerUserId { get; set; }
    public DateTime ViewedOn { get; set; }
}

public class TrackPlay
{
    public Guid UserId { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class Service
{
    public List<ProfileView> GetProfileViews()
    {
        string profileViewsListFileName = "/Users/belovanas/profile-views.json";
        if (File.Exists(profileViewsListFileName))
        {
            var loadedProfileViews = JsonSerializer.Deserialize<List<ProfileView>>(File.ReadAllText(profileViewsListFileName));
            if (loadedProfileViews != null && loadedProfileViews.Count > 0)
            {
                return loadedProfileViews;
            }
        }

        string rootFolder = @"/Users/belovanas/local-folder"; // Change this to your folder path
        var extractedData = new List<ProfileView>();

        foreach (var hourFolder in Directory.EnumerateDirectories(rootFolder))
        {
            foreach (var file in Directory.EnumerateFiles(hourFolder, "*"))
            {
                try
                {
                    string jsonContent = File.ReadAllText(file);
                    using var jsonDoc = JsonDocument.Parse(jsonContent);

                    var root = jsonDoc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var element in root.EnumerateArray())
                        {
                            if (element.TryGetProperty("profileOwnerUserId", out var userIdProp) &&
                                element.TryGetProperty("createdOn", out var createdOnProp))
                            {
                                Guid userId = userIdProp.GetGuid();
                                DateTime createdOn = createdOnProp.GetDateTime();

                                extractedData.Add(new ProfileView()
                                {
                                    ProfileOwnerUserId = userId,
                                    ViewedOn = createdOn
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {file}: {ex.Message}");
                }
            }
        }

        string json = JsonSerializer.Serialize(extractedData);
        File.WriteAllText(profileViewsListFileName, json);
        return extractedData;
    }

    public List<TrackPlay> GetTrackPlays()
    {
        string trackPlaysListFileName = "/Users/belovanas/track-plays.json";
        if (File.Exists(trackPlaysListFileName))
        {
            var loadedTrackPlays = JsonSerializer.Deserialize<List<TrackPlay>>(File.ReadAllText(trackPlaysListFileName));
            if (loadedTrackPlays != null && loadedTrackPlays.Count > 0)
            {
                return loadedTrackPlays;
            }
        }

        string rootFolder = @"/Users/belovanas/local-folder-1"; // Change this to your folder path
        var results = new List<TrackPlay>();

        foreach (var hourFolder in Directory.EnumerateDirectories(rootFolder))
        {
            foreach (var file in Directory.EnumerateFiles(hourFolder, "*"))
            {
                foreach (string line in File.ReadLines(file))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    using JsonDocument outerDoc = JsonDocument.Parse(line);
                    JsonElement root = outerDoc.RootElement;

                    if (root.TryGetProperty("serialized_event", out JsonElement serializedEventProp))
                    {
                        string serializedEvent = serializedEventProp.GetString();

                        // Parse the inner JSON string
                        using JsonDocument innerDoc = JsonDocument.Parse(serializedEvent);
                        JsonElement eventData = innerDoc.RootElement;

                        if (eventData.TryGetProperty("UserId", out var userIdProp) &&
                            eventData.TryGetProperty("CreatedOn", out var createdOnProp))
                        {
                            results.Add(new TrackPlay()
                            {
                                UserId = userIdProp.GetGuid(),
                                CreatedOn = createdOnProp.GetDateTime()
                            });
                        }
                    }
                }
            }
        }

        string json = JsonSerializer.Serialize(results);
        File.WriteAllText(trackPlaysListFileName, json);
        return results;
    }
}
