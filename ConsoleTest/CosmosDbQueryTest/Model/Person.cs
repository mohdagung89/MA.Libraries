using Newtonsoft.Json;

namespace ConsoleTest.CosmosDbQueryTest.Model;

public class Person
{
    /// <summary>
    /// Gets or sets the unique identifier of the person.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the first name of the person.
    /// </summary>
    [JsonProperty("firstName")]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last name of the person.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the date of birth of the person.
    /// </summary>
    [JsonProperty("dob")]
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the email address of the person.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the education level.
    /// </summary>
    public string? EducationLevel { get; set; }
}
