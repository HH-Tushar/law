// Models/Failure.cs
public class Failure
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public  int Code { get; set; }

    public Failure(string? title = null, string? description = null, int? code = null)
    {
        Title = title;
        Description = description;
        Code = code ?? 400;
    }
}

public class Attempt<T>
{
    public T? Data { get; set; }
    public Failure? Error { get; set; }

    public bool IsSuccess => Error == null;

    public static Attempt<T> Success(T data) => new Attempt<T> { Data = data };
    public static Attempt<T> Failed(Failure error) => new Attempt<T> { Error = error };
}