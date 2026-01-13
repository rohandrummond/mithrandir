using System.Text;
using mithrandir.Models;

namespace mithrandir.Services;

public class ZodiacService : IZodiacService
{
    private static readonly Dictionary<ZodiacSign, LotRCharacter> Characters = new()
    {
        [ZodiacSign.Aries] = new LotRCharacter
        {
            Name = "Gimli",
            Title = "Son of Gloin",
            Traits = ["Bold and fierce warrior", "Unwavering loyalty", "Proud of his heritage"],
            Quote = "Certainty of death. Small chance of success. What are we waiting for?"
        },
        [ZodiacSign.Taurus] = new LotRCharacter
        {
            Name = "Samwise",
            Title = "The Brave",
            Traits = ["Steadfast loyalty", "Down-to-earth wisdom", "Unwavering courage"],
            Quote = "There's some good in this world, Mr. Frodo, and it's worth fighting for."
        },
        [ZodiacSign.Gemini] = new LotRCharacter
        {
            Name = "Gollum",
            Title = "Smeagol",
            Traits = ["Dual nature", "Resourceful survivor", "Obsessive passion"],
            Quote = "We wants it, we needs it. Must have the precious."
        },
        [ZodiacSign.Cancer] = new LotRCharacter
        {
            Name = "Galadriel",
            Title = "Lady of Light",
            Traits = ["Intuitive wisdom", "Protective nature", "Inner strength"],
            Quote = "Even the smallest person can change the course of the future."
        },
        [ZodiacSign.Leo] = new LotRCharacter
        {
            Name = "Aragorn",
            Title = "King Elessar",
            Traits = ["Natural leader", "Noble courage", "Rightful king"],
            Quote = "I do not fear death."
        },
        [ZodiacSign.Virgo] = new LotRCharacter
        {
            Name = "Elrond",
            Title = "Lord of Rivendell",
            Traits = ["Wise counselor", "Analytical mind", "Patient guardian"],
            Quote = "The time of the Elves is over. My people are leaving these shores."
        },
        [ZodiacSign.Libra] = new LotRCharacter
        {
            Name = "Frodo",
            Title = "Ring-bearer",
            Traits = ["Balanced judgment", "Burden of choice", "Quiet courage"],
            Quote = "I will take the Ring, though I do not know the way."
        },
        [ZodiacSign.Scorpio] = new LotRCharacter
        {
            Name = "Saruman",
            Title = "The White",
            Traits = ["Intense focus", "Transformative power", "Deep knowledge"],
            Quote = "Against the power of Mordor there can be no victory."
        },
        [ZodiacSign.Sagittarius] = new LotRCharacter
        {
            Name = "Legolas",
            Title = "Prince of Mirkwood",
            Traits = ["Adventurous spirit", "Keen perception", "Swift action"],
            Quote = "A red sun rises. Blood has been spilled this night."
        },
        [ZodiacSign.Capricorn] = new LotRCharacter
        {
            Name = "Thorin",
            Title = "Oakenshield",
            Traits = ["Ambitious leader", "Determined will", "Proud heritage"],
            Quote = "If more people valued home above gold, this world would be a merrier place."
        },
        [ZodiacSign.Aquarius] = new LotRCharacter
        {
            Name = "Gandalf",
            Title = "The Grey Pilgrim",
            Traits = ["Unconventional wisdom", "Visionary thinking", "Guardian of the free"],
            Quote = "All we have to decide is what to do with the time that is given us."
        },
        [ZodiacSign.Pisces] = new LotRCharacter
        {
            Name = "Tom Bombadil",
            Title = "Oldest and Fatherless",
            Traits = ["Mystical nature", "Carefree spirit", "Ancient wisdom"],
            Quote = "Bright blue his jacket is, and his boots are yellow."
        }
    };

    public string GenerateResponse(DateOnly dateOfBirth)
    {
        var sign = GetZodiacSign(dateOfBirth);
        var character = Characters[sign];

        var sb = new StringBuilder();
        sb.AppendLine($"Zodiac: {sign}");
        sb.AppendLine($"Character: {character.Name}, {character.Title}");
        sb.AppendLine();
        sb.AppendLine("Traits:");
        foreach (var trait in character.Traits)
        {
            sb.AppendLine($"  - {trait}");
        }
        sb.AppendLine();
        sb.AppendLine($"\"{character.Quote}\"");

        return sb.ToString();
    }

    private static ZodiacSign GetZodiacSign(DateOnly date)
    {
        var month = date.Month;
        var day = date.Day;

        return (month, day) switch
        {
            (3, >= 21) or (4, <= 19) => ZodiacSign.Aries,
            (4, >= 20) or (5, <= 20) => ZodiacSign.Taurus,
            (5, >= 21) or (6, <= 20) => ZodiacSign.Gemini,
            (6, >= 21) or (7, <= 22) => ZodiacSign.Cancer,
            (7, >= 23) or (8, <= 22) => ZodiacSign.Leo,
            (8, >= 23) or (9, <= 22) => ZodiacSign.Virgo,
            (9, >= 23) or (10, <= 22) => ZodiacSign.Libra,
            (10, >= 23) or (11, <= 21) => ZodiacSign.Scorpio,
            (11, >= 22) or (12, <= 21) => ZodiacSign.Sagittarius,
            (12, >= 22) or (1, <= 19) => ZodiacSign.Capricorn,
            (1, >= 20) or (2, <= 18) => ZodiacSign.Aquarius,
            _ => ZodiacSign.Pisces // Feb 19 - Mar 20
        };
    }
}
