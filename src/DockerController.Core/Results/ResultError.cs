namespace DockerController.Core.Results;

public enum ResultError
{
    // 0 = ingen orsak: ett oinitierat Result failar, det blir aldrig en falsk framgång.
    None = 0,
    NotFound,
    Conflict,
    Validation,
}
