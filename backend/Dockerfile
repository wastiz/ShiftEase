FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln ./

COPY API/*.csproj ./API/
COPY API.DTO/*.csproj ./API.DTO/
COPY BLL/*.csproj ./BLL/
COPY BLL.DTO/*.csproj ./BLL.DTO/
COPY BLL.Contracts/*.csproj ./BLL.Contracts/
COPY DAL/*.csproj ./DAL/
COPY DAL.DTO/*.csproj ./DAL.DTO/
COPY DAL.Contracts/*.csproj ./DAL.Contracts/
COPY Domain/*.csproj ./Domain/
COPY Tests/*.csproj ./Tests/

RUN dotnet restore

COPY API/. ./API/
COPY API.DTO/. ./API.DTO/
COPY BLL/. ./BLL/
COPY BLL.DTO/. ./BLL.DTO/
COPY BLL.Contracts/. ./BLL.Contracts/
COPY DAL/. ./DAL/
COPY DAL.DTO/. ./DAL.DTO/
COPY DAL.Contracts/. ./DAL.Contracts/
COPY Domain/. ./Domain/
COPY Tests/. ./Tests/

WORKDIR /app/API
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "API.dll"]
