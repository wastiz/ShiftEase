namespace ShiftEaseAPI.Middlewares;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class AllowEmployeeAccessAttribute : Attribute
{
}