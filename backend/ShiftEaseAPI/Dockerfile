FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln ./

COPY BLL/*.csproj ./BLL/
COPY DAL/*.csproj ./DAL/
COPY Domain/*.csproj ./Domain/
COPY Localization/*.csproj ./Localization/
COPY ShiftEaseAPI/*.csproj ./ShiftEaseAPI/
COPY WebAdminApp/*.csproj ./WebAdminApp/
COPY TestProject/*.csproj ./TestProject/

RUN dotnet restore

COPY BLL/. ./BLL/
COPY DAL/. ./DAL/
COPY Domain/. ./Domain/
COPY Localization/. ./Localization/
COPY ShiftEaseAPI/. ./ShiftEaseAPI/
COPY WebAdminApp/. ./WebAdminApp/
COPY TestProject/. ./TestProject/

WORKDIR /app/ShiftEaseAPI
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 5258
COPY --from=build /app/out .

ENV ConnectionStrings__DefaultConnection="Host=db;Port=5432;Database=shift_ease;Username=postgres;Password=admin"

ENTRYPOINT ["dotnet", "ShiftEaseAPI.dll"]
