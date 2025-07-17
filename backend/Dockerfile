FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ./ShiftEaseAPI/*.csproj ./ShiftEaseAPI/
COPY ./BLL/*.csproj ./BLL/
COPY ./DAL/*.csproj ./DAL/
COPY ./Domain/*.csproj ./Domain/

RUN dotnet restore ./ShiftEaseAPI/ShiftEaseAPI.csproj
COPY . .

RUN dotnet publish ./ShiftEaseAPI/ShiftEaseAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 3007

ENTRYPOINT ["dotnet", "ShiftEaseAPI.dll"]