using ConsoleTest.CosmosDbQueryTest.Model;
using QueryBuilderLibrary.CosmosDbQuery;

namespace ConsoleTest.CosmosDbQueryTest;

public class Example
{
    public static string CreatePersonQuery1()
    {
        var qb = QueryBuilderHelper<Person>.Initialize();

        return qb.Build();
    }

    public static string CreatePersonQuery2()
    {
        var qb = QueryBuilderHelper<Person>.Initialize();

        qb.Select(x => new List<string> { nameof(x.Email), nameof(x.FirstName), nameof(x.DateOfBirth) });

        return qb.Build();
    }

    public static string CreatePersonQuery3()
    {
        var qb = QueryBuilderHelper<Person>.Initialize();

        qb.Select(x => new List<string> { nameof(x.Email), nameof(x.FirstName), nameof(x.DateOfBirth) });
        qb.Where(x => nameof(x.EducationLevel), Operation.Equal, "S3");

        return qb.Build();
    }

    public static string CreatePersonQuery4()
    {
        var qb = QueryBuilderHelper<Person>.Initialize();

        qb.Select(x => new List<string> { nameof(x.Email), nameof(x.FirstName), nameof(x.DateOfBirth) });
        qb.Where(x => nameof(x.EducationLevel), Operation.Equal, "S3");
        qb.OrderBy(x => nameof(x.FirstName));

        return qb.Build();
    }

    public static string CreatePersonQuery5()
    {
        var qb = QueryBuilderHelper<Person>.Initialize();

        qb.Select(x => new List<string> { nameof(x.Email), nameof(x.FirstName), nameof(x.DateOfBirth) });
        qb.Where(x => nameof(x.EducationLevel), Operation.Equal, "S3");
        qb.OrderBy(x => nameof(x.FirstName));
        qb.Skip(10).Take(10);

        return qb.Build();
    }

    public static string CreatePersonQuery6()
    {
        var qb = QueryBuilderHelper<Person>.Initialize();

        qb.Select(x => new List<string> { nameof(x.Email), nameof(x.FirstName), nameof(x.DateOfBirth), nameof(x.EducationLevel) });
        qb.GroupBy(x => new List<string> { nameof(x.Email), nameof(x.FirstName), nameof(x.DateOfBirth), nameof(x.EducationLevel) });
        qb.Where(x => nameof(x.EducationLevel), Operation.Equal, "S3");
        qb.OrderBy(x => nameof(x.FirstName));
        qb.Skip(10).Take(10);

        return qb.Build();
    }

    public static string CreatePersonQuery7()
    {
        var qb = QueryBuilderHelper<Person>.Initialize();

        qb.Select(x => new List<string> { nameof(x.Email), nameof(x.FirstName), nameof(x.DateOfBirth) });
        qb.SelectRawFunctionAs($"iif({qb.InitialTable}.educationLevel = 'S3', 'Doctor','Not Doctor')", "isDoctor");
        qb.OrderBy(x => nameof(x.FirstName));
        qb.Skip(10).Take(10);

        return qb.Build();
    }

    public static string CreatePersonQuery8()
    {
        var qb = QueryBuilderHelper<Person>.Initialize();

        qb.FromSubQuery(qb1 =>
        {
            qb1.Select(x => new List<string> { nameof(x.Email), nameof(x.FirstName), nameof(x.DateOfBirth) });
            qb1.SelectRawFunctionAs<Person>($"iif({qb.InitialTable}.educationLevel = 'S3', 'Doctor','Not Doctor')", x => nameof(x.EducationLevel));
        });
        qb.OrderBy(x => nameof(x.FirstName));
        qb.Skip(10).Take(10);

        return qb.Build();
    }
}
